using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace GarageMotor.Api.Infrastructure;

public sealed class PartitionManager(
    NpgsqlDataSource migrationDataSource,
    ILogger<PartitionManager> logger) : BackgroundService
{
    private const int PreviousMonths = 12;
    private const int FutureMonths = 3;
    private const string SchemaName = "garagemotor";

    private static readonly PartitionDefinition[] ManagedTables =
    [
        new("recovery_attempts", "occurred_utc"),
        new("operation_receipts", "created_utc"),
        new("audit_events", "created_utc"),
        new("expenses", "spent_on"),
        new("fuel_records", "occurred_on"),
        new("electric_charges", "occurred_on"),
    ];

    private static readonly Action<ILogger, string, Exception?> PartitionMaintenanceFailed =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(1301, nameof(PartitionMaintenanceFailed)),
            "Daily partition maintenance failed; PostgreSQL SQLSTATE {SqlState}.");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await EnsureAllMonthlyPartitionsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                var sqlState = (exception as PostgresException)?.SqlState ?? "unknown";
                PartitionMaintenanceFailed(logger, sqlState, null);
            }

            try
            {
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    public async Task EnsureAllMonthlyPartitionsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await migrationDataSource.OpenConnectionAsync(cancellationToken);
        var currentMonth = GetCurrentMonthUtc();

        foreach (var definition in ManagedTables)
        {
            if (!await RelationExistsAsync(connection, SchemaName, definition.TableName, cancellationToken))
            {
                continue;
            }

            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            try
            {
                for (var monthOffset = -PreviousMonths; monthOffset <= FutureMonths; monthOffset++)
                {
                    var monthStart = currentMonth.AddMonths(monthOffset);
                    await EnsureMonthlyPartitionAsync(
                        connection,
                        transaction,
                        SchemaName,
                        definition.TableName,
                        definition.PartitionColumn,
                        monthStart,
                        cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
        }

        await using var retentionTransaction = await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            await Operations.AuditStore.DeleteExpiredAsync(
                connection,
                retentionTransaction,
                DateTimeOffset.UtcNow,
                cancellationToken);
            await retentionTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await retentionTransaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    public static async Task EnsureMonthlyPartitionAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string schemaName,
        string parentTableName,
        string partitionColumn,
        DateOnly monthStart,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(transaction);

        var schema = QuoteIdentifier(schemaName);
        var parentName = QuoteIdentifier(parentTableName);
        var columnName = QuoteIdentifier(partitionColumn);
        var qualifiedParent = $"{schema}.{parentName}";
        var month = new DateOnly(monthStart.Year, monthStart.Month, 1);
        var monthEnd = month.AddMonths(1);
        var partitionTableName = $"{parentTableName}_{month.ToString("yyyyMM", CultureInfo.InvariantCulture)}";
        var partitionName = QuoteIdentifier(partitionTableName);
        var qualifiedPartition = $"{schema}.{partitionName}";

        var partitionColumnType = await GetPartitionColumnTypeAsync(
            connection,
            transaction,
            schemaName,
            parentTableName,
            partitionColumn,
            cancellationToken);
        var startBound = FormatPartitionBound(month, partitionColumnType);
        var endBound = FormatPartitionBound(monthEnd, partitionColumnType);

        await ExecuteAsync(connection, transaction, $"LOCK TABLE {qualifiedParent} IN ACCESS EXCLUSIVE MODE", cancellationToken);
        var existingBound = await GetPartitionBoundAsync(
            connection,
            transaction,
            schemaName,
            parentTableName,
            partitionTableName,
            cancellationToken);

        if (existingBound is not null)
        {
            EnsureExpectedBound(existingBound, month, monthEnd, partitionColumnType, partitionTableName);
            await EnsureChildIndexesAsync(connection, transaction, qualifiedParent, qualifiedPartition, cancellationToken);
            return;
        }

        if (await RelationExistsAsync(connection, transaction, schemaName, partitionTableName, cancellationToken))
        {
            throw new InvalidOperationException($"Partition relation exists but is not attached: {partitionTableName}.");
        }

        var defaultPartition = await GetDefaultPartitionAsync(
            connection,
            transaction,
            schemaName,
            parentTableName,
            cancellationToken);
        var qualifiedDefault = $"{QuoteIdentifier(defaultPartition.SchemaName)}.{QuoteIdentifier(defaultPartition.TableName)}";
        var stageName = QuoteIdentifier($"gm_stage_{Guid.NewGuid():N}");
        var startDate = month.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var endDate = monthEnd.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        await ExecuteAsync(
            connection,
            transaction,
            $"CREATE TEMP TABLE {stageName} ON COMMIT DROP AS " +
            $"SELECT * FROM {qualifiedDefault} WHERE {columnName} >= {startBound} AND {columnName} < {endBound}",
            cancellationToken);
        await ExecuteAsync(
            connection,
            transaction,
            $"DELETE FROM {qualifiedDefault} WHERE {columnName} >= {startBound} AND {columnName} < {endBound}",
            cancellationToken);

        var writableColumns = await GetWritableColumnsAsync(
            connection,
            transaction,
            qualifiedParent,
            cancellationToken);
        var hasAlwaysIdentity = await HasAlwaysIdentityColumnAsync(
            connection,
            transaction,
            qualifiedParent,
            cancellationToken);
        var overrideIdentity = hasAlwaysIdentity ? " OVERRIDING SYSTEM VALUE" : string.Empty;
        var constraintName = QuoteIdentifier($"{partitionTableName}_range_check");

        await ExecuteAsync(
            connection,
            transaction,
            $"CREATE TABLE {qualifiedPartition} (LIKE {qualifiedParent} INCLUDING ALL)",
            cancellationToken);
        await ExecuteAsync(
            connection,
            transaction,
            $"ALTER TABLE {qualifiedPartition} ADD CONSTRAINT {constraintName} " +
            $"CHECK ({columnName} >= {startBound} AND {columnName} < {endBound})",
            cancellationToken);
        await ExecuteAsync(
            connection,
            transaction,
            $"INSERT INTO {qualifiedPartition} ({writableColumns}){overrideIdentity} " +
            $"SELECT {writableColumns} FROM {stageName}",
            cancellationToken);
        await ExecuteAsync(
            connection,
            transaction,
            $"ALTER TABLE {qualifiedParent} ATTACH PARTITION {qualifiedPartition} " +
            $"FOR VALUES FROM ({startBound}) TO ({endBound})",
            cancellationToken);

        await EnsureChildIndexesAsync(connection, transaction, qualifiedParent, qualifiedPartition, cancellationToken);
    }

    private static DateOnly GetCurrentMonthUtc()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new DateOnly(today.Year, today.Month, 1);
    }

    private static string QuoteIdentifier(string identifier)
    {
        if (identifier.Length is < 1 or > 63 ||
            !Regex.IsMatch(identifier, "^[a-z_][a-z0-9_]*$", RegexOptions.CultureInvariant))
        {
            throw new ArgumentException($"Database identifier is not an allowed lowercase identifier: {identifier}.", nameof(identifier));
        }

        return $"\"{identifier}\"";
    }

    private static string FormatPartitionBound(DateOnly month, string columnType)
    {
        var date = month.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return columnType switch
        {
            "date" => $"DATE '{date}'",
            "timestamp with time zone" => $"TIMESTAMPTZ '{date} 00:00:00+00'",
            _ => throw new InvalidOperationException($"Unsupported monthly partition column type: {columnType}."),
        };
    }

    private static async Task<string> GetPartitionColumnTypeAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string schemaName,
        string parentTableName,
        string partitionColumn,
        CancellationToken cancellationToken)
    {
        const string query = """
            SELECT part.partstrat::text, part.partnatts, attribute.attnum, part.partattrs[0],
                   format_type(attribute.atttypid, attribute.atttypmod)
            FROM pg_partitioned_table AS part
            JOIN pg_class AS parent ON parent.oid = part.partrelid
            JOIN pg_namespace AS namespace ON namespace.oid = parent.relnamespace
            JOIN pg_attribute AS attribute ON attribute.attrelid = parent.oid
            WHERE namespace.nspname = @schema_name
              AND parent.relname = @parent_table
              AND attribute.attname = @partition_column
              AND attribute.attnum > 0
              AND NOT attribute.attisdropped
            """;
        await using var command = new NpgsqlCommand(query, connection, transaction);
        command.Parameters.AddWithValue("schema_name", schemaName);
        command.Parameters.AddWithValue("parent_table", parentTableName);
        command.Parameters.AddWithValue("partition_column", partitionColumn);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken) ||
            reader.GetString(0) != "r" ||
            reader.GetInt16(1) != 1 ||
            reader.GetInt16(2) != reader.GetInt16(3))
        {
            throw new InvalidOperationException($"Table {schemaName}.{parentTableName} is not range-partitioned by {partitionColumn}.");
        }

        return reader.GetString(4);
    }

    private static async Task<string?> GetPartitionBoundAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string schemaName,
        string parentTableName,
        string partitionTableName,
        CancellationToken cancellationToken)
    {
        const string query = """
            SELECT pg_get_expr(child.relpartbound, child.oid)
            FROM pg_inherits AS inheritance
            JOIN pg_class AS parent ON parent.oid = inheritance.inhparent
            JOIN pg_namespace AS parent_namespace ON parent_namespace.oid = parent.relnamespace
            JOIN pg_class AS child ON child.oid = inheritance.inhrelid
            WHERE parent_namespace.nspname = @schema_name
              AND parent.relname = @parent_table
              AND child.relname = @partition_table
            """;
        await using var command = new NpgsqlCommand(query, connection, transaction);
        command.Parameters.AddWithValue("schema_name", schemaName);
        command.Parameters.AddWithValue("parent_table", parentTableName);
        command.Parameters.AddWithValue("partition_table", partitionTableName);
        return (string?)await command.ExecuteScalarAsync(cancellationToken);
    }

    private static void EnsureExpectedBound(
        string bound,
        DateOnly monthStart,
        DateOnly monthEnd,
        string columnType,
        string partitionName)
    {
        var boundValues = Regex.Matches(bound, "'([^']+)'");
        if (boundValues.Count != 2)
        {
            throw new InvalidOperationException($"Partition {partitionName} has a different range bound.");
        }

        var startValue = boundValues[0].Groups[1].Value;
        var endValue = boundValues[1].Groups[1].Value;
        var matchesExpected = columnType switch
        {
            "date" => DateOnly.TryParse(startValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var actualStart) &&
                      DateOnly.TryParse(endValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var actualEnd) &&
                      actualStart == monthStart && actualEnd == monthEnd,
            "timestamp with time zone" => DateTimeOffset.TryParse(
                                                  startValue,
                                                  CultureInfo.InvariantCulture,
                                                  DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                                                  out var actualStartUtc) &&
                                              DateTimeOffset.TryParse(
                                                  endValue,
                                                  CultureInfo.InvariantCulture,
                                                  DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                                                  out var actualEndUtc) &&
                                              actualStartUtc == new DateTimeOffset(monthStart.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)) &&
                                              actualEndUtc == new DateTimeOffset(monthEnd.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)),
            _ => false,
        };

        if (!matchesExpected)
        {
            throw new InvalidOperationException($"Partition {partitionName} has a different range bound.");
        }
    }

    private static async Task<(string SchemaName, string TableName)> GetDefaultPartitionAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string schemaName,
        string parentTableName,
        CancellationToken cancellationToken)
    {
        const string query = """
            SELECT child_namespace.nspname, child.relname
            FROM pg_inherits AS inheritance
            JOIN pg_class AS parent ON parent.oid = inheritance.inhparent
            JOIN pg_namespace AS parent_namespace ON parent_namespace.oid = parent.relnamespace
            JOIN pg_class AS child ON child.oid = inheritance.inhrelid
            JOIN pg_namespace AS child_namespace ON child_namespace.oid = child.relnamespace
            WHERE parent_namespace.nspname = @schema_name
              AND parent.relname = @parent_table
              AND pg_get_expr(child.relpartbound, child.oid) = 'DEFAULT'
            """;
        await using var command = new NpgsqlCommand(query, connection, transaction);
        command.Parameters.AddWithValue("schema_name", schemaName);
        command.Parameters.AddWithValue("parent_table", parentTableName);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException($"Default partition is missing for {schemaName}.{parentTableName}.");
        }

        return (reader.GetString(0), reader.GetString(1));
    }

    private static async Task<string> GetWritableColumnsAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string qualifiedParent,
        CancellationToken cancellationToken)
    {
        const string query = """
            SELECT string_agg(format('%I', attribute.attname), ', ' ORDER BY attribute.attnum)
            FROM pg_attribute AS attribute
            WHERE attribute.attrelid = to_regclass(@qualified_parent)
              AND attribute.attnum > 0
              AND NOT attribute.attisdropped
              AND attribute.attgenerated = ''
            """;
        await using var command = new NpgsqlCommand(query, connection, transaction);
        command.Parameters.AddWithValue("qualified_parent", qualifiedParent);
        var columns = (string?)await command.ExecuteScalarAsync(cancellationToken);
        return string.IsNullOrWhiteSpace(columns)
            ? throw new InvalidOperationException($"Partitioned table has no writable columns: {qualifiedParent}.")
            : columns;
    }

    private static async Task<bool> HasAlwaysIdentityColumnAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string qualifiedParent,
        CancellationToken cancellationToken)
    {
        const string query = """
            SELECT EXISTS (
                SELECT 1
                FROM pg_attribute AS attribute
                WHERE attribute.attrelid = to_regclass(@qualified_parent)
                  AND attribute.attnum > 0
                  AND NOT attribute.attisdropped
                  AND attribute.attidentity = 'a'
            )
            """;
        await using var command = new NpgsqlCommand(query, connection, transaction);
        command.Parameters.AddWithValue("qualified_parent", qualifiedParent);
        return (bool)(await command.ExecuteScalarAsync(cancellationToken))!;
    }

    private static async Task EnsureChildIndexesAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string qualifiedParent,
        string qualifiedChild,
        CancellationToken cancellationToken)
    {
        const string query = """
            SELECT
                (SELECT count(*) FROM pg_index WHERE indrelid = to_regclass(@parent) AND indisvalid),
                (SELECT count(*) FROM pg_index WHERE indrelid = to_regclass(@child) AND indisvalid)
            """;
        await using var command = new NpgsqlCommand(query, connection, transaction);
        command.Parameters.AddWithValue("parent", qualifiedParent);
        command.Parameters.AddWithValue("child", qualifiedChild);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken) || reader.GetInt64(0) != reader.GetInt64(1))
        {
            throw new InvalidOperationException($"Partition indexes do not match the parent for {qualifiedChild}.");
        }
    }

    private static async Task<bool> RelationExistsAsync(
        NpgsqlConnection connection,
        string schemaName,
        string tableName,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand("SELECT to_regclass(@qualified_name) IS NOT NULL", connection);
        command.Parameters.AddWithValue("qualified_name", $"{schemaName}.{tableName}");
        return (bool)(await command.ExecuteScalarAsync(cancellationToken))!;
    }

    private static async Task<bool> RelationExistsAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string schemaName,
        string tableName,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand("SELECT to_regclass(@qualified_name) IS NOT NULL", connection, transaction);
        command.Parameters.AddWithValue("qualified_name", $"{schemaName}.{tableName}");
        return (bool)(await command.ExecuteScalarAsync(cancellationToken))!;
    }

    private static async Task ExecuteAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string commandText,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(commandText, connection, transaction);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private sealed record PartitionDefinition(string TableName, string PartitionColumn);
}
