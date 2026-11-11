using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GarageMotor.Api.Infrastructure;
using Npgsql;

namespace GarageMotor.Api.Operations;

public enum OperationScopeKind
{
    Actor,
    Garage,
}

public sealed record OperationScope(OperationScopeKind Kind, Guid ScopeId, Guid? GarageId);

public sealed record OperationResponse(
    int StatusCode,
    Guid? ResourceId,
    long? Version,
    string? ETag,
    string? JsonPayload,
    Guid? GarageId = null);

public sealed record OperationResult(OperationResponse Response, bool IsReplay);

public sealed class OperationStore(Database database)
{
    private readonly Database database = database ?? throw new ArgumentNullException(nameof(database));

    public async Task<OperationResult> ExecuteAsync(
        Guid actorId,
        Guid operationKey,
        string operationName,
        OperationScope scope,
        JsonElement normalizedRequest,
        Func<NpgsqlConnection, NpgsqlTransaction, CancellationToken, Task<OperationResponse>> createAsync,
        Func<NpgsqlConnection, NpgsqlTransaction, OperationResponse, CancellationToken, Task<bool>> reauthorizeReplayAsync,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(actorId, operationKey, operationName, scope, createAsync, reauthorizeReplayAsync);
        var requestHash = ComputeRequestHash(operationName, normalizedRequest);
        var scopeName = scope.Kind == OperationScopeKind.Actor ? "actor" : "garage";

        return await database.ExecuteInTransactionAsync(
            actorId,
            scope.GarageId,
            async (connection, transaction, token) =>
            {
                await AcquireOperationLockAsync(
                    connection,
                    transaction,
                    actorId,
                    operationKey,
                    scope,
                    token);

                var existing = await FindReceiptAsync(
                    connection,
                    transaction,
                    actorId,
                    operationKey,
                    scopeName,
                    scope.ScopeId,
                    token);
                if (existing is not null)
                {
                    if (!CryptographicOperations.FixedTimeEquals(existing.RequestHash, requestHash))
                    {
                        return new OperationResult(new OperationResponse(409, null, null, null, null), true);
                    }

                    if (existing.Response is null || existing.Status != "succeeded")
                    {
                        throw new InvalidOperationException("A committed operation receipt is incomplete.");
                    }

                    if (!await reauthorizeReplayAsync(connection, transaction, existing.Response, token))
                    {
                        return new OperationResult(new OperationResponse(404, null, null, null, null), true);
                    }

                    return new OperationResult(existing.Response, true);
                }

                var createdUtc = await InsertPendingReceiptAsync(
                    connection,
                    transaction,
                    actorId,
                    operationKey,
                    operationName,
                    scopeName,
                    scope.ScopeId,
                    scope.GarageId,
                    requestHash,
                    token);
                var response = await createAsync(connection, transaction, token);
                ValidateSuccessfulResponse(response);
                await UpdateReceiptAsync(
                    connection,
                    transaction,
                    actorId,
                    operationKey,
                    createdUtc,
                    response,
                    token);

                return new OperationResult(response, false);
            },
            cancellationToken);
    }

    public static byte[] ComputeRequestHash(string operationName, JsonElement normalizedRequest)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteString("operation", operationName);
            writer.WritePropertyName("request");
            WriteCanonicalJson(writer, normalizedRequest);
            writer.WriteEndObject();
        }

        return SHA256.HashData(buffer.ToArray());
    }

    private static void ValidateRequest(
        Guid actorId,
        Guid operationKey,
        string operationName,
        OperationScope scope,
        Func<NpgsqlConnection, NpgsqlTransaction, CancellationToken, Task<OperationResponse>> createAsync,
        Func<NpgsqlConnection, NpgsqlTransaction, OperationResponse, CancellationToken, Task<bool>> reauthorizeReplayAsync)
    {
        ArgumentNullException.ThrowIfNull(scope);
        if (actorId == Guid.Empty || operationKey == Guid.Empty || scope.ScopeId == Guid.Empty)
        {
            throw new ArgumentException("Actor, operation, and scope identifiers must be non-empty UUIDs.");
        }
        if (string.IsNullOrWhiteSpace(operationName) || operationName.Length > 100)
        {
            throw new ArgumentException("Operation name must contain 1 to 100 characters.", nameof(operationName));
        }
        ArgumentNullException.ThrowIfNull(createAsync);
        ArgumentNullException.ThrowIfNull(reauthorizeReplayAsync);

        if (scope.Kind == OperationScopeKind.Actor && (scope.ScopeId != actorId || scope.GarageId is not null))
        {
            throw new ArgumentException("Actor-scoped operations must use the actor ID and no garage ID.", nameof(scope));
        }
        if (scope.Kind == OperationScopeKind.Garage &&
            (scope.GarageId is null || scope.ScopeId != scope.GarageId || scope.GarageId == Guid.Empty))
        {
            throw new ArgumentException("Garage-scoped operations must use the garage ID as scope ID.", nameof(scope));
        }
    }

    private static async Task AcquireOperationLockAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Guid actorId,
        Guid operationKey,
        OperationScope scope,
        CancellationToken cancellationToken)
    {
        var lockKey = $"{actorId:N}:{scope.Kind}:{scope.ScopeId:N}:{operationKey:N}";
        await using var command = new NpgsqlCommand(
            "SELECT pg_advisory_xact_lock(hashtextextended(@lock_key, 0))",
            connection,
            transaction);
        command.Parameters.AddWithValue("lock_key", lockKey);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<StoredReceipt?> FindReceiptAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Guid actorId,
        Guid operationKey,
        string scopeKind,
        Guid scopeId,
        CancellationToken cancellationToken)
    {
        const string query = """
            SELECT request_hash, status, resource_id, response_status, response_version,
                   response_etag, response_payload::text, garage_id
            FROM garagemotor.operation_receipts
            WHERE actor_id = @actor_id
              AND operation_key = @operation_key
              AND scope_kind = @scope_kind
              AND scope_id = @scope_id
            ORDER BY created_utc DESC
            LIMIT 1
            """;
        await using var command = new NpgsqlCommand(query, connection, transaction);
        command.Parameters.AddWithValue("actor_id", actorId);
        command.Parameters.AddWithValue("operation_key", operationKey);
        command.Parameters.AddWithValue("scope_kind", scopeKind);
        command.Parameters.AddWithValue("scope_id", scopeId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var requestHash = (byte[])reader[0];
        var status = reader.GetString(1);
        if (reader.IsDBNull(2) || reader.IsDBNull(3) || reader.IsDBNull(4))
        {
            return new StoredReceipt(requestHash, status, null);
        }

        var response = new OperationResponse(
            reader.GetInt16(3),
            reader.GetGuid(2),
            reader.GetInt64(4),
            reader.IsDBNull(5) ? null : reader.GetString(5),
            reader.IsDBNull(6) ? null : reader.GetString(6),
            reader.IsDBNull(7) ? null : reader.GetGuid(7));
        return new StoredReceipt(requestHash, status, response);
    }

    private static async Task<DateTime> InsertPendingReceiptAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Guid actorId,
        Guid operationKey,
        string operationName,
        string scopeKind,
        Guid scopeId,
        Guid? garageId,
        byte[] requestHash,
        CancellationToken cancellationToken)
    {
        const string query = """
            INSERT INTO garagemotor.operation_receipts
                (operation_key, actor_id, operation_name, scope_kind, scope_id, garage_id, request_hash, status)
            VALUES
                (@operation_key, @actor_id, @operation_name, @scope_kind, @scope_id, @garage_id, @request_hash, 'pending')
            RETURNING created_utc
            """;
        await using var command = new NpgsqlCommand(query, connection, transaction);
        command.Parameters.AddWithValue("operation_key", operationKey);
        command.Parameters.AddWithValue("actor_id", actorId);
        command.Parameters.AddWithValue("operation_name", operationName);
        command.Parameters.AddWithValue("scope_kind", scopeKind);
        command.Parameters.AddWithValue("scope_id", scopeId);
        command.Parameters.AddWithValue("garage_id", garageId is null ? DBNull.Value : garageId.Value);
        command.Parameters.AddWithValue("request_hash", requestHash);
        return (DateTime)(await command.ExecuteScalarAsync(cancellationToken))!;
    }

    private static async Task UpdateReceiptAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Guid actorId,
        Guid operationKey,
        DateTime createdUtc,
        OperationResponse response,
        CancellationToken cancellationToken)
    {
        const string query = """
            UPDATE garagemotor.operation_receipts
            SET status = 'succeeded',
                resource_id = @resource_id,
                response_status = @response_status,
                response_version = @response_version,
                response_etag = @response_etag,
                response_payload = @response_payload::jsonb,
                garage_id = COALESCE(@garage_id, garage_id)
            WHERE actor_id = @actor_id
              AND operation_key = @operation_key
              AND created_utc = @created_utc
            """;
        await using var command = new NpgsqlCommand(query, connection, transaction);
        command.Parameters.AddWithValue("resource_id", response.ResourceId!.Value);
        command.Parameters.AddWithValue("response_status", checked((short)response.StatusCode));
        command.Parameters.AddWithValue("response_version", response.Version!.Value);
        command.Parameters.AddWithValue("response_etag", response.ETag is null ? DBNull.Value : response.ETag);
        command.Parameters.AddWithValue("response_payload", response.JsonPayload is null ? DBNull.Value : response.JsonPayload);
        command.Parameters.AddWithValue("garage_id", response.GarageId is null ? DBNull.Value : response.GarageId.Value);
        command.Parameters.AddWithValue("actor_id", actorId);
        command.Parameters.AddWithValue("operation_key", operationKey);
        command.Parameters.AddWithValue("created_utc", createdUtc);
        if (await command.ExecuteNonQueryAsync(cancellationToken) != 1)
        {
            throw new InvalidOperationException("The operation receipt could not be completed.");
        }
    }

    private static void ValidateSuccessfulResponse(OperationResponse response)
    {
        if (response.StatusCode is < 200 or >= 300 ||
            response.ResourceId is null || response.ResourceId == Guid.Empty ||
            response.Version is null or <= 0)
        {
            throw new InvalidOperationException("A successful domain operation must return a resource ID and positive version.");
        }

        if (response.JsonPayload is not null)
        {
            using var document = JsonDocument.Parse(response.JsonPayload);
            if (ContainsSecretProperty(document.RootElement))
            {
                throw new InvalidOperationException("Operation receipt payload contains a prohibited secret field.");
            }
        }
    }

    private static bool ContainsSecretProperty(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject().Any(property =>
                IsSecretPropertyName(property.Name) || ContainsSecretProperty(property.Value)),
            JsonValueKind.Array => element.EnumerateArray().Any(ContainsSecretProperty),
            _ => false,
        };
    }

    private static bool IsSecretPropertyName(string name)
    {
        var normalizedName = name.ToLowerInvariant();
        return normalizedName.Contains("password", StringComparison.Ordinal) ||
            normalizedName.Contains("token", StringComparison.Ordinal) ||
            normalizedName.Contains("secret", StringComparison.Ordinal) ||
            normalizedName.Contains("recoverycode", StringComparison.Ordinal) ||
            normalizedName.EndsWith("code", StringComparison.Ordinal) ||
            normalizedName.EndsWith("hash", StringComparison.Ordinal) ||
            normalizedName.Equals("storagekey", StringComparison.Ordinal);
    }

    private static void WriteCanonicalJson(Utf8JsonWriter writer, JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in element.EnumerateObject().OrderBy(property => property.Name, StringComparer.Ordinal))
                {
                    writer.WritePropertyName(property.Name);
                    WriteCanonicalJson(writer, property.Value);
                }
                writer.WriteEndObject();
                break;
            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray())
                {
                    WriteCanonicalJson(writer, item);
                }
                writer.WriteEndArray();
                break;
            case JsonValueKind.String:
                writer.WriteStringValue(element.GetString());
                break;
            case JsonValueKind.Number:
                writer.WriteRawValue(element.GetRawText());
                break;
            case JsonValueKind.True:
                writer.WriteBooleanValue(true);
                break;
            case JsonValueKind.False:
                writer.WriteBooleanValue(false);
                break;
            case JsonValueKind.Null:
                writer.WriteNullValue();
                break;
            default:
                throw new JsonException("Unsupported JSON value in an idempotency request.");
        }
    }

    private sealed record StoredReceipt(byte[] RequestHash, string Status, OperationResponse? Response);
}
