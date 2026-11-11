using System.Security.Cryptography;
using System.Text;
using GarageMotor.Api.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Npgsql;

namespace GarageMotor.Api.Identity;

public sealed record AccountRecord(Guid UserId, string EmailNormalized, string DisplayName, string PasswordHash);

public sealed record SessionCredential(string Token, DateTimeOffset ExpiresUtc);

public sealed record SessionPrincipal(Guid UserId, Guid SessionId, DateTimeOffset ExpiresUtc);

public sealed record OfflineRecoveryBatch(Guid BatchId, DateTimeOffset ExpiresUtc, IReadOnlyList<string> Codes);

public sealed class IdentityStore(Database database)
{
    private const int SessionTokenBytes = 32;
    private const int RecoveryCodeBytes = 16;
    private const int RecoveryCodeCount = 10;

    private readonly Database database = database ?? throw new ArgumentNullException(nameof(database));
    private readonly PasswordHasher<IdentityUser> passwordHasher = new();

    public async Task<Guid> RegisterAccountAsync(
        string email,
        string displayName,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        ValidateDisplayName(displayName);
        ValidatePassword(password);

        var userId = Guid.NewGuid();
        var user = new IdentityUser { Id = userId.ToString("D"), UserName = normalizedEmail, Email = normalizedEmail };
        var passwordHash = passwordHasher.HashPassword(user, password);
        await database.ExecuteInTransactionAsync(
            userId,
            null,
            async (connection, transaction, token) =>
            {
                await using var command = new NpgsqlCommand(
                    "INSERT INTO garagemotor.users (user_id, email_normalized, display_name, password_hash) " +
                    "VALUES (@user_id, @email, @display_name, @password_hash)",
                    connection,
                    transaction);
                command.Parameters.AddWithValue("user_id", userId);
                command.Parameters.AddWithValue("email", normalizedEmail);
                command.Parameters.AddWithValue("display_name", displayName.Trim());
                command.Parameters.AddWithValue("password_hash", passwordHash);
                await command.ExecuteNonQueryAsync(token);
                return true;
            },
            cancellationToken);

        return userId;
    }

    public async Task<AccountRecord?> FindAccountByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        var transactionActorId = Guid.NewGuid();
        return await database.ExecuteInTransactionAsync(
            transactionActorId,
            null,
            async (connection, transaction, token) =>
            {
                await using var command = new NpgsqlCommand(
                    "SELECT user_id, email_normalized, display_name, password_hash " +
                    "FROM garagemotor.users WHERE lower(email_normalized) = @email AND state = 'active'",
                    connection,
                    transaction);
                command.Parameters.AddWithValue("email", normalizedEmail);
                await using var reader = await command.ExecuteReaderAsync(token);
                return await reader.ReadAsync(token)
                    ? new AccountRecord(reader.GetGuid(0), reader.GetString(1), reader.GetString(2), reader.GetString(3))
                    : null;
            },
            cancellationToken);
    }

    public bool VerifyPassword(AccountRecord account, string providedPassword)
    {
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(providedPassword);
        var user = new IdentityUser { Id = account.UserId.ToString("D"), UserName = account.EmailNormalized };
        return passwordHasher.VerifyHashedPassword(user, account.PasswordHash, providedPassword) != PasswordVerificationResult.Failed;
    }

    public async Task<SessionCredential> CreateSessionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        EnsureIdentifier(userId, nameof(userId));
        var token = EncodeBase64Url(RandomNumberGenerator.GetBytes(SessionTokenBytes));
        var tokenHash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        var createdByActorId = userId;
        return await database.ExecuteInTransactionAsync(
            createdByActorId,
            null,
            async (connection, transaction, tokenCancellation) =>
            {
                await using var command = new NpgsqlCommand(
                    "WITH created AS (SELECT clock_timestamp() AS instant) " +
                    "INSERT INTO garagemotor.sessions (user_id, token_hash, created_utc, expires_utc) " +
                    "SELECT @user_id, @token_hash, created.instant, created.instant + interval '12 hours' " +
                    "FROM created RETURNING expires_utc",
                    connection,
                    transaction);
                command.Parameters.AddWithValue("user_id", userId);
                command.Parameters.AddWithValue("token_hash", tokenHash);
                var expiresUtc = (DateTime)(await command.ExecuteScalarAsync(tokenCancellation))!;
                return new SessionCredential(token, new DateTimeOffset(DateTime.SpecifyKind(expiresUtc, DateTimeKind.Utc)));
            },
            cancellationToken);
    }

    public async Task<bool> RevokeSessionAsync(Guid userId, string token, CancellationToken cancellationToken = default)
    {
        EnsureIdentifier(userId, nameof(userId));
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        var tokenHash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return await database.ExecuteInTransactionAsync(
            userId,
            null,
            async (connection, transaction, tokenCancellation) =>
            {
                await using var command = new NpgsqlCommand(
                    "UPDATE garagemotor.sessions SET revoked_utc = clock_timestamp() " +
                    "WHERE user_id = @user_id AND token_hash = @token_hash " +
                    "AND revoked_utc IS NULL AND expires_utc > clock_timestamp()",
                    connection,
                    transaction);
                command.Parameters.AddWithValue("user_id", userId);
                command.Parameters.AddWithValue("token_hash", tokenHash);
                return await command.ExecuteNonQueryAsync(tokenCancellation) == 1;
            },
            cancellationToken);
    }

    public async Task<SessionPrincipal?> FindActiveSessionAsync(string token, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        if (token.Length is < 40 or > 64)
        {
            return null;
        }

        var tokenHash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return await database.ExecuteInTransactionAsync(
            Guid.NewGuid(),
            null,
            async (connection, transaction, tokenCancellation) =>
            {
                await using var command = new NpgsqlCommand(
                    "SELECT session.session_id, session.user_id, session.expires_utc " +
                    "FROM garagemotor.sessions AS session " +
                    "JOIN garagemotor.users AS account ON account.user_id = session.user_id " +
                    "WHERE session.token_hash = @token_hash AND session.revoked_utc IS NULL " +
                    "AND session.expires_utc > clock_timestamp() AND account.state = 'active'",
                    connection,
                    transaction);
                command.Parameters.AddWithValue("token_hash", tokenHash);
                await using var reader = await command.ExecuteReaderAsync(tokenCancellation);
                if (!await reader.ReadAsync(tokenCancellation))
                {
                    return null;
                }

                var expiresUtc = reader.GetDateTime(2);
                return new SessionPrincipal(
                    reader.GetGuid(1),
                    reader.GetGuid(0),
                    new DateTimeOffset(DateTime.SpecifyKind(expiresUtc, DateTimeKind.Utc)));
            },
            cancellationToken);
    }

    public async Task<OfflineRecoveryBatch> RenewOfflineRecoveryCodesAsync(
        Guid userId,
        string currentPassword,
        CancellationToken cancellationToken = default)
    {
        var account = await FindAccountByIdAsync(userId, cancellationToken);
        if (account is null || !VerifyPassword(account, currentPassword))
        {
            throw new InvalidOperationException("Reauthentication failed.");
        }

        var codes = Enumerable.Range(0, RecoveryCodeCount)
            .Select(_ => EncodeBase64Url(RandomNumberGenerator.GetBytes(RecoveryCodeBytes)))
            .ToArray();
        var hashes = codes.Select(HashRecoveryCode).ToArray();

        return await database.ExecuteInTransactionAsync(
            userId,
            null,
            async (connection, transaction, token) =>
            {
                await RevokeActiveCodesAsync(connection, transaction, userId, "offline", token);
                await using (var revokeBatches = new NpgsqlCommand(
                    "UPDATE garagemotor.recovery_batches SET revoked_utc = clock_timestamp() " +
                    "WHERE user_id = @user_id AND revoked_utc IS NULL",
                    connection,
                    transaction))
                {
                    revokeBatches.Parameters.AddWithValue("user_id", userId);
                    await revokeBatches.ExecuteNonQueryAsync(token);
                }

                Guid batchId;
                DateTimeOffset expiresUtc;
                await using (var createBatch = new NpgsqlCommand(
                    "WITH created AS (SELECT clock_timestamp() AS instant) " +
                    "INSERT INTO garagemotor.recovery_batches (user_id, created_utc, expires_utc) " +
                    "SELECT @user_id, created.instant, created.instant + interval '12 months' FROM created " +
                    "RETURNING batch_id, expires_utc",
                    connection,
                    transaction))
                {
                    createBatch.Parameters.AddWithValue("user_id", userId);
                    await using var reader = await createBatch.ExecuteReaderAsync(token);
                    await reader.ReadAsync(token);
                    batchId = reader.GetGuid(0);
                    var expiration = reader.GetDateTime(1);
                    expiresUtc = new DateTimeOffset(DateTime.SpecifyKind(expiration, DateTimeKind.Utc));
                }

                for (var index = 0; index < codes.Length; index++)
                {
                    await InsertRecoveryCodeAsync(connection, transaction, userId, batchId, "offline", hashes[index], null, token);
                }

                return new OfflineRecoveryBatch(batchId, expiresUtc, codes);
            },
            cancellationToken);
    }

    public async Task<string?> CreateSmtpRecoveryCodeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        EnsureIdentifier(userId, nameof(userId));
        var code = EncodeBase64Url(RandomNumberGenerator.GetBytes(RecoveryCodeBytes));
        var codeHash = HashRecoveryCode(code);

        return await database.ExecuteInTransactionAsync(
            userId,
            null,
            async (connection, transaction, token) =>
            {
                await RevokeActiveCodesAsync(connection, transaction, userId, "smtp", token);
                var inserted = await InsertRecoveryCodeAsync(connection, transaction, userId, null, "smtp", codeHash, 15, token);
                return inserted ? code : null;
            },
            cancellationToken);
    }

    public async Task<bool> ConfirmRecoveryAsync(
        string email,
        string code,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        ValidatePassword(newPassword);
        if (!IsValidRecoveryCode(code))
        {
            return false;
        }

        var transactionActorId = Guid.NewGuid();
        return await database.ExecuteInTransactionAsync(
            transactionActorId,
            null,
            async (connection, transaction, token) =>
            {
                await using var findUser = new NpgsqlCommand(
                    "SELECT user_id, email_normalized, display_name, password_hash FROM garagemotor.users " +
                    "WHERE lower(email_normalized) = @email AND state = 'active' FOR UPDATE",
                    connection,
                    transaction);
                findUser.Parameters.AddWithValue("email", normalizedEmail);
                AccountRecord? account;
                await using (var reader = await findUser.ExecuteReaderAsync(token))
                {
                    account = await reader.ReadAsync(token)
                        ? new AccountRecord(reader.GetGuid(0), reader.GetString(1), reader.GetString(2), reader.GetString(3))
                        : null;
                }
                if (account is null)
                {
                    return false;
                }

                var presentedHash = HashRecoveryCode(code);
                Guid? matchedCodeId = null;
                await using (var findCodes = new NpgsqlCommand(
                    "SELECT recovery_code_id, code_hash FROM garagemotor.recovery_codes " +
                    "WHERE user_id = @user_id AND expires_utc > clock_timestamp() " +
                    "AND consumed_utc IS NULL AND revoked_utc IS NULL " +
                    "AND (batch_id IS NULL OR EXISTS (SELECT 1 FROM garagemotor.recovery_batches batch " +
                    "WHERE batch.batch_id = recovery_codes.batch_id AND batch.revoked_utc IS NULL AND batch.expires_utc > clock_timestamp()))",
                    connection,
                    transaction))
                {
                    findCodes.Parameters.AddWithValue("user_id", account.UserId);
                    await using var reader = await findCodes.ExecuteReaderAsync(token);
                    while (await reader.ReadAsync(token))
                    {
                        var storedHash = reader.GetFieldValue<byte[]>(1);
                        if (CryptographicOperations.FixedTimeEquals(storedHash, presentedHash))
                        {
                            matchedCodeId = reader.GetGuid(0);
                        }
                    }
                }
                if (matchedCodeId is null)
                {
                    return false;
                }

                var user = new IdentityUser { Id = account.UserId.ToString("D"), UserName = account.EmailNormalized };
                var newPasswordHash = passwordHasher.HashPassword(user, newPassword);
                await using (var updateUser = new NpgsqlCommand(
                    "UPDATE garagemotor.users SET password_hash = @password_hash, version = version + 1, changed_utc = clock_timestamp() WHERE user_id = @user_id",
                    connection,
                    transaction))
                {
                    updateUser.Parameters.AddWithValue("password_hash", newPasswordHash);
                    updateUser.Parameters.AddWithValue("user_id", account.UserId);
                    await updateUser.ExecuteNonQueryAsync(token);
                }

                await using (var consumeCode = new NpgsqlCommand(
                    "UPDATE garagemotor.recovery_codes SET consumed_utc = clock_timestamp() " +
                    "WHERE recovery_code_id = @code_id AND consumed_utc IS NULL AND revoked_utc IS NULL",
                    connection,
                    transaction))
                {
                    consumeCode.Parameters.AddWithValue("code_id", matchedCodeId.Value);
                    await consumeCode.ExecuteNonQueryAsync(token);
                }

                await RevokeAllSessionsAsync(connection, transaction, account.UserId, token);
                await RevokeActiveCodesAsync(connection, transaction, account.UserId, "smtp", token);
                await RevokeActiveCodesAsync(connection, transaction, account.UserId, "offline", token);
                await using var revokeBatches = new NpgsqlCommand(
                    "UPDATE garagemotor.recovery_batches SET revoked_utc = clock_timestamp() " +
                    "WHERE user_id = @user_id AND revoked_utc IS NULL",
                    connection,
                    transaction);
                revokeBatches.Parameters.AddWithValue("user_id", account.UserId);
                await revokeBatches.ExecuteNonQueryAsync(token);
                return true;
            },
            cancellationToken);
    }

    public static string NormalizeEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        var normalized = email.Trim().ToLowerInvariant();
        if (normalized.Length > 254 || !normalized.Contains('@', StringComparison.Ordinal))
        {
            throw new ArgumentException("Email format is invalid.", nameof(email));
        }

        return normalized;
    }

    public static bool IsValidRecoveryCode(string code)
    {
        return code.Length == 22 && code.All(character =>
            character is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '-' or '_');
    }

    private async Task<AccountRecord?> FindAccountByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        EnsureIdentifier(userId, nameof(userId));
        return await database.ExecuteInTransactionAsync(
            userId,
            null,
            async (connection, transaction, token) =>
            {
                await using var command = new NpgsqlCommand(
                    "SELECT user_id, email_normalized, display_name, password_hash " +
                    "FROM garagemotor.users WHERE user_id = @user_id AND state = 'active'",
                    connection,
                    transaction);
                command.Parameters.AddWithValue("user_id", userId);
                await using var reader = await command.ExecuteReaderAsync(token);
                return await reader.ReadAsync(token)
                    ? new AccountRecord(reader.GetGuid(0), reader.GetString(1), reader.GetString(2), reader.GetString(3))
                    : null;
            },
            cancellationToken);
    }

    private static async Task RevokeActiveCodesAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Guid userId,
        string kind,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(
            "UPDATE garagemotor.recovery_codes SET revoked_utc = clock_timestamp() " +
            "WHERE user_id = @user_id AND kind = @kind AND consumed_utc IS NULL AND revoked_utc IS NULL",
            connection,
            transaction);
        command.Parameters.AddWithValue("user_id", userId);
        command.Parameters.AddWithValue("kind", kind);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<bool> InsertRecoveryCodeAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Guid userId,
        Guid? batchId,
        string kind,
        byte[] codeHash,
        int? lifetimeMinutes,
        CancellationToken cancellationToken)
    {
        var lifetimeExpression = lifetimeMinutes is null
            ? "created.instant + interval '12 months'"
            : $"created.instant + interval '{lifetimeMinutes.Value} minutes'";
        var query = "WITH created AS (SELECT clock_timestamp() AS instant) " +
                    "INSERT INTO garagemotor.recovery_codes (user_id, batch_id, kind, code_hash, created_utc, expires_utc) " +
                    $"SELECT @user_id, @batch_id, @kind, @code_hash, created.instant, {lifetimeExpression} FROM created";
        await using var command = new NpgsqlCommand(query, connection, transaction);
        command.Parameters.AddWithValue("user_id", userId);
        command.Parameters.AddWithValue("batch_id", batchId is null ? DBNull.Value : batchId.Value);
        command.Parameters.AddWithValue("kind", kind);
        command.Parameters.AddWithValue("code_hash", codeHash);
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    private static async Task RevokeAllSessionsAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Guid userId,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(
            "UPDATE garagemotor.sessions SET revoked_utc = clock_timestamp() " +
            "WHERE user_id = @user_id AND revoked_utc IS NULL",
            connection,
            transaction);
        command.Parameters.AddWithValue("user_id", userId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static byte[] HashRecoveryCode(string code)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes(code));
    }

    private static string EncodeBase64Url(byte[] value)
    {
        return Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static void ValidateDisplayName(string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        if (displayName.Trim().Length > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(displayName), "Display name must not exceed 100 characters.");
        }
    }

    private static void ValidatePassword(string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        if (password.Length is < 12 or > 128)
        {
            throw new ArgumentOutOfRangeException(nameof(password), "Password must contain 12 to 128 characters.");
        }
    }

    private static void EnsureIdentifier(Guid id, string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Identifier must be a non-empty UUID.", parameterName);
        }
    }
}
