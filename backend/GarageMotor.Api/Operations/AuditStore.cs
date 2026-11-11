using System.Security.Cryptography;
using Npgsql;

namespace GarageMotor.Api.Operations;

public sealed class AuditStore
{
    public async Task AppendAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Guid? garageId,
        Guid actorId,
        string action,
        string? targetId,
        IReadOnlyCollection<string> changedFields,
        string? traceId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(transaction);
        if (actorId == Guid.Empty)
        {
            throw new ArgumentException("Actor ID must be a non-empty UUID.", nameof(actorId));
        }
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentNullException.ThrowIfNull(changedFields);

        if (action.Length > 100 || targetId?.Length > 100 || traceId?.Length > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(action), "Audit identifiers and action names are limited to 100 characters.");
        }
        if (changedFields.Count > 50 || changedFields.Any(field => string.IsNullOrWhiteSpace(field) || field.Length > 100))
        {
            throw new ArgumentOutOfRangeException(nameof(changedFields), "Audit stores field names only, up to 50 fields.");
        }
        if (changedFields.Distinct(StringComparer.Ordinal).Count() != changedFields.Count)
        {
            throw new ArgumentException("Changed field names must be unique.", nameof(changedFields));
        }

        const string query = """
            INSERT INTO garagemotor.audit_events
                (garage_id, actor_id, action, target_id, changed_fields, trace_id)
            VALUES
                (@garage_id, @actor_id, @action, @target_id, @changed_fields, @trace_id)
            """;
        await using var command = new NpgsqlCommand(query, connection, transaction);
        command.Parameters.AddWithValue("garage_id", garageId is null ? DBNull.Value : garageId.Value);
        command.Parameters.AddWithValue("actor_id", PseudonymizeActor(actorId));
        command.Parameters.AddWithValue("action", action);
        command.Parameters.AddWithValue("target_id", targetId is null ? DBNull.Value : targetId);
        command.Parameters.AddWithValue("changed_fields", changedFields.ToArray());
        command.Parameters.AddWithValue("trace_id", traceId is null ? DBNull.Value : traceId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static string PseudonymizeActor(Guid actorId)
    {
        var domainSeparatedId = $"GarageMotor.Audit.Actor.v1:{actorId:N}";
        return Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(domainSeparatedId))).ToLowerInvariant();
    }

    public static async Task<int> DeleteExpiredAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(transaction);
        if (nowUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("Retention cutoff must be UTC.", nameof(nowUtc));
        }

        await using var command = new NpgsqlCommand(
            "DELETE FROM garagemotor.audit_events WHERE occurred_utc < @cutoff",
            connection,
            transaction);
        command.Parameters.AddWithValue("cutoff", nowUtc.AddDays(-90));
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
