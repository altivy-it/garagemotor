using System.Security.Cryptography;
using System.Text;
using GarageMotor.Api.Infrastructure;
using Npgsql;

namespace GarageMotor.Api.Identity;

public sealed record RecoveryAttemptDecision(bool Allowed, int AccountFailures, int OriginFailures);

public sealed class RecoveryAttemptStore(Database database)
{
    private const int AccountFailureLimit = 5;
    private const int OriginFailureLimit = 100;
    private static readonly TimeSpan AttemptWindow = TimeSpan.FromMinutes(15);

    private readonly Database database = database ?? throw new ArgumentNullException(nameof(database));

    public async Task<RecoveryAttemptDecision> RecordFailureAsync(
        string normalizedAccount,
        string origin,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedAccount);
        ArgumentException.ThrowIfNullOrWhiteSpace(origin);

        var accountHash = HashKey("account", IdentityStore.NormalizeEmail(normalizedAccount));
        var originHash = HashKey("origin", origin.Trim());
        var transactionActorId = Guid.NewGuid();

        return await database.ExecuteInTransactionAsync(
            transactionActorId,
            null,
            async (connection, transaction, token) =>
            {
                await AcquireRateLimitLockAsync(connection, transaction, accountHash, originHash, token);
                await DeleteExpiredAttemptsAsync(connection, transaction, accountHash, originHash, token);

                var accountFailures = await CountFailuresAsync(connection, transaction, "account_key_hash", accountHash, token);
                var originFailures = await CountFailuresAsync(connection, transaction, "origin_key_hash", originHash, token);
                if (accountFailures >= AccountFailureLimit || originFailures >= OriginFailureLimit)
                {
                    return new RecoveryAttemptDecision(false, accountFailures, originFailures);
                }

                await using var insert = new NpgsqlCommand(
                    "INSERT INTO garagemotor.recovery_attempts (account_key_hash, origin_key_hash) " +
                    "VALUES (@account_hash, @origin_hash)",
                    connection,
                    transaction);
                insert.Parameters.AddWithValue("account_hash", accountHash);
                insert.Parameters.AddWithValue("origin_hash", originHash);
                await insert.ExecuteNonQueryAsync(token);

                return new RecoveryAttemptDecision(true, accountFailures + 1, originFailures + 1);
            },
            cancellationToken);
    }

    private static async Task AcquireRateLimitLockAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        byte[] accountHash,
        byte[] originHash,
        CancellationToken cancellationToken)
    {
        await using var accountLock = new NpgsqlCommand(
            "SELECT pg_advisory_xact_lock(hashtextextended(@lock_key, 0))",
            connection,
            transaction);
        accountLock.Parameters.AddWithValue("lock_key", $"account:{Convert.ToHexString(accountHash)}");
        await accountLock.ExecuteNonQueryAsync(cancellationToken);

        await using var originLock = new NpgsqlCommand(
            "SELECT pg_advisory_xact_lock(hashtextextended(@lock_key, 0))",
            connection,
            transaction);
        originLock.Parameters.AddWithValue("lock_key", $"origin:{Convert.ToHexString(originHash)}");
        await originLock.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task DeleteExpiredAttemptsAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        byte[] accountHash,
        byte[] originHash,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(
            "DELETE FROM garagemotor.recovery_attempts " +
            "WHERE occurred_utc <= clock_timestamp() - interval '15 minutes' " +
            "AND (account_key_hash = @account_hash OR origin_key_hash = @origin_hash)",
            connection,
            transaction);
        command.Parameters.AddWithValue("account_hash", accountHash);
        command.Parameters.AddWithValue("origin_hash", originHash);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<int> CountFailuresAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string columnName,
        byte[] keyHash,
        CancellationToken cancellationToken)
    {
        if (columnName is not ("account_key_hash" or "origin_key_hash"))
        {
            throw new ArgumentOutOfRangeException(nameof(columnName));
        }

        await using var command = new NpgsqlCommand(
            $"SELECT count(*) FROM garagemotor.recovery_attempts " +
            $"WHERE {columnName} = @key_hash AND occurred_utc > clock_timestamp() - interval '15 minutes'",
            connection,
            transaction);
        command.Parameters.AddWithValue("key_hash", keyHash);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken), System.Globalization.CultureInfo.InvariantCulture);
    }

    private static byte[] HashKey(string kind, string value)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes($"GarageMotor.RecoveryAttempt.v1:{kind}:{value}"));
    }
}
