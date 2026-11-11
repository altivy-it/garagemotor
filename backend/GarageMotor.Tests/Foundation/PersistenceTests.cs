using System.Globalization;
using GarageMotor.Api.Infrastructure;
using GarageMotor.Api.Operations;
using GarageMotor.Tests.Fixtures;
using Npgsql;
using Xunit;

namespace GarageMotor.Tests.Foundation;

[Collection(TestCollectionNames.Integration)]
public sealed class PersistenceTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task RuntimeRoleCannotBypassRowLevelSecurity()
    {
        await using var dataSource = RealEnvironment.CreateDataSource();
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT rolsuper OR rolbypassrls FROM pg_roles WHERE rolname = current_user",
            connection);

        var bypassesPolicies = (bool?)await command.ExecuteScalarAsync();

        Assert.NotNull(bypassesPolicies);
        Assert.False(bypassesPolicies.Value, "The application role must not bypass RLS.");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GarageContextIsRlsScopedAndDoesNotLeakAcrossPooledConnections()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__GarageMotor")!;
        await using var database = new Database(connectionString);
        var auditStore = new AuditStore();
        var actorId = Guid.NewGuid();
        var firstGarageId = Guid.NewGuid();
        var secondGarageId = Guid.NewGuid();
        var firstTargetId = $"foundation:first:{Guid.NewGuid():N}";
        var secondTargetId = $"foundation:second:{Guid.NewGuid():N}";

        try
        {
            await AppendTestAuditAsync(database, auditStore, actorId, firstGarageId, firstTargetId);
            await AppendTestAuditAsync(database, auditStore, actorId, secondGarageId, secondTargetId);

            Assert.Equal(1, await CountAuditEventsAsync(database, actorId, firstGarageId, firstTargetId));
            Assert.Equal(1, await CountAuditEventsAsync(database, actorId, secondGarageId, secondTargetId));
            Assert.Equal(0, await CountAuditEventsAsync(database, actorId, null, null));

            var observedGarageContext = await database.ExecuteInTransactionAsync(
                actorId,
                firstGarageId,
                async (connection, transaction, cancellationToken) =>
                {
                    await using var command = new NpgsqlCommand(
                        "SELECT current_setting('app.garage_id', true)",
                        connection,
                        transaction);
                    return (string?)await command.ExecuteScalarAsync(cancellationToken);
                });
            Assert.Equal(firstGarageId.ToString(), observedGarageContext);

            var clearedGarageContext = await database.ExecuteInTransactionAsync(
                actorId,
                null,
                async (connection, transaction, cancellationToken) =>
                {
                    await using var command = new NpgsqlCommand(
                        "SELECT current_setting('app.garage_id', true)",
                        connection,
                        transaction);
                    return (string?)await command.ExecuteScalarAsync(cancellationToken);
                });
            Assert.Equal(string.Empty, clearedGarageContext);

            await using var freshDataSource = NpgsqlDataSource.Create(connectionString);
            await using var freshConnection = await freshDataSource.OpenConnectionAsync();
            await AssertRuntimeRoleIsRestrictedAsync(freshConnection);
            Assert.True(string.IsNullOrEmpty(await ReadGarageContextAsync(freshConnection)));
        }
        finally
        {
            await DeleteTestAuditAsync(database, actorId, firstGarageId, firstTargetId);
            await DeleteTestAuditAsync(database, actorId, secondGarageId, secondTargetId);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task InitialGarageReceiptIsActorScopedBeforeGarageIdExists()
    {
        await using var dataSource = RealEnvironment.CreateDataSource();
        await using var connection = await dataSource.OpenConnectionAsync();
        await ExecuteAsync(
            connection,
            "CREATE TEMP TABLE gm_test_receipts (" +
            "actor_id uuid NOT NULL, operation_key uuid NOT NULL, scope_kind text NOT NULL, " +
            "scope_id uuid NOT NULL, garage_id uuid NULL, request_hash text NOT NULL, " +
            "resource_id uuid NOT NULL, PRIMARY KEY (actor_id, operation_key));");

        var actorId = Guid.NewGuid();
        var anotherActorId = Guid.NewGuid();
        var operationKey = Guid.NewGuid();
        var garageId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();

        await InsertReceiptAsync(connection, actorId, operationKey, actorId, null, "initial", resourceId);
        await InsertReceiptAsync(connection, anotherActorId, operationKey, anotherActorId, null, "initial", Guid.NewGuid());

        PostgresException? duplicate = null;
        try
        {
            await InsertReceiptAsync(connection, actorId, operationKey, actorId, garageId, "initial", resourceId);
        }
        catch (PostgresException exception)
        {
            duplicate = exception;
        }

        Assert.NotNull(duplicate);
        Assert.Equal(PostgresErrorCodes.UniqueViolation, duplicate.SqlState);

        await using var update = new NpgsqlCommand(
            "UPDATE gm_test_receipts SET garage_id = @garage_id WHERE actor_id = @actor_id AND operation_key = @operation_key AND garage_id IS NULL",
            connection);
        update.Parameters.AddWithValue("garage_id", garageId);
        update.Parameters.AddWithValue("actor_id", actorId);
        update.Parameters.AddWithValue("operation_key", operationKey);
        Assert.Equal(1, await update.ExecuteNonQueryAsync());

        await using var verify = new NpgsqlCommand(
            "SELECT garage_id FROM gm_test_receipts WHERE actor_id = @actor_id AND operation_key = @operation_key",
            connection);
        verify.Parameters.AddWithValue("actor_id", actorId);
        verify.Parameters.AddWithValue("operation_key", operationKey);
        Assert.Equal(garageId, await verify.ExecuteScalarAsync());
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task MonthlyRolloverKeepsOutOfWindowRowsInDefaultAndIndexesChildren()
    {
        await using var migrationDataSource = RealEnvironment.CreateMigrationDataSource();
        await using var connection = await migrationDataSource.OpenConnectionAsync();

        var monthStart = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1);
        var monthEnd = monthStart.AddMonths(1);
        var outsideWindow = monthEnd.AddMonths(2);
        var monthPartition = $"recovery_attempts_{monthStart:yyyyMM}";
        var inWindowAttemptId = Guid.NewGuid();
        var outOfWindowAttemptId = Guid.NewGuid();
        await InsertRecoveryAttemptAsync(connection, inWindowAttemptId, monthStart);
        await InsertRecoveryAttemptAsync(connection, outOfWindowAttemptId, outsideWindow);

        try
        {
            await using (var transaction = await connection.BeginTransactionAsync())
            {
                await PartitionManager.EnsureMonthlyPartitionAsync(
                    connection,
                    transaction,
                    "garagemotor",
                    "recovery_attempts",
                    "occurred_utc",
                    monthStart);
                await transaction.CommitAsync();
            }

            await using var routedRow = new NpgsqlCommand(
                "SELECT tableoid::regclass::text FROM garagemotor.recovery_attempts WHERE attempt_id = @attempt_id",
                connection);
            routedRow.Parameters.AddWithValue("attempt_id", inWindowAttemptId);
            Assert.Contains(monthPartition, (string)(await routedRow.ExecuteScalarAsync())!);

            await using var defaultCount = new NpgsqlCommand(
                "SELECT count(*) FROM garagemotor.recovery_attempts_default WHERE attempt_id = @attempt_id",
                connection);
            defaultCount.Parameters.AddWithValue("attempt_id", outOfWindowAttemptId);
            Assert.Equal(1L, await defaultCount.ExecuteScalarAsync());

            await using var childIndex = new NpgsqlCommand(
                "SELECT count(*) FROM pg_indexes WHERE schemaname = 'garagemotor' AND tablename = @table_name AND indexdef LIKE '%account_key_hash, occurred_utc%'",
                connection);
            childIndex.Parameters.AddWithValue("table_name", monthPartition);
            Assert.True((long)(await childIndex.ExecuteScalarAsync())! > 0);
        }
        finally
        {
            await using var cleanup = new NpgsqlCommand(
                "DELETE FROM garagemotor.recovery_attempts WHERE attempt_id = ANY(@attempt_ids)",
                connection);
            cleanup.Parameters.AddWithValue("attempt_ids", new[] { inWindowAttemptId, outOfWindowAttemptId });
            await cleanup.ExecuteNonQueryAsync();
        }
    }

    private static async Task AssertRuntimeRoleIsRestrictedAsync(NpgsqlConnection connection)
    {
        await using var command = new NpgsqlCommand(
            "SELECT rolsuper OR rolbypassrls FROM pg_roles WHERE rolname = current_user",
            connection);
        var bypassesPolicies = (bool?)await command.ExecuteScalarAsync();
        Assert.NotNull(bypassesPolicies);
        Assert.False(bypassesPolicies.Value, "The RLS test requires the application runtime role.");
    }

    private static async Task AppendTestAuditAsync(
        Database database,
        AuditStore auditStore,
        Guid actorId,
        Guid garageId,
        string targetId)
    {
        await database.ExecuteInTransactionAsync(
            actorId,
            garageId,
            async (connection, transaction, cancellationToken) =>
            {
                await auditStore.AppendAsync(
                    connection,
                    transaction,
                    garageId,
                    actorId,
                    "test.context",
                    targetId,
                    [],
                    null,
                    cancellationToken);
                return true;
            });
    }

    private static async Task<int> CountAuditEventsAsync(Database database, Guid actorId, Guid? garageId, string? targetId)
    {
        return await database.ExecuteInTransactionAsync(
            actorId,
            garageId,
            async (connection, transaction, cancellationToken) =>
            {
                await using var command = new NpgsqlCommand(
                    "SELECT count(*) FROM garagemotor.audit_events WHERE action = 'test.context' " +
                    (targetId is null ? string.Empty : "AND target_id = @target_id"),
                    connection,
                    transaction);
                if (targetId is not null)
                {
                    command.Parameters.AddWithValue("target_id", targetId);
                }
                return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
            });
    }

    private static async Task<string?> ReadGarageContextAsync(NpgsqlConnection connection)
    {
        await using var command = new NpgsqlCommand("SELECT current_setting('app.garage_id', true)", connection);
        var value = await command.ExecuteScalarAsync();
        return value is DBNull ? null : (string?)value;
    }

    private static async Task DeleteTestAuditAsync(Database database, Guid actorId, Guid garageId, string targetId)
    {
        await database.ExecuteInTransactionAsync(
            actorId,
            garageId,
            async (connection, transaction, cancellationToken) =>
            {
                await using var command = new NpgsqlCommand(
                    "DELETE FROM garagemotor.audit_events WHERE action = 'test.context' AND target_id = @target_id",
                    connection,
                    transaction);
                command.Parameters.AddWithValue("target_id", targetId);
                return await command.ExecuteNonQueryAsync(cancellationToken);
            });
    }

    private static async Task InsertReceiptAsync(
        NpgsqlConnection connection,
        Guid actorId,
        Guid operationKey,
        Guid scopeId,
        Guid? garageId,
        string requestHash,
        Guid resourceId)
    {
        await using var command = new NpgsqlCommand(
            "INSERT INTO gm_test_receipts (actor_id, operation_key, scope_kind, scope_id, garage_id, request_hash, resource_id) " +
            "VALUES (@actor_id, @operation_key, 'actor', @scope_id, @garage_id, @request_hash, @resource_id)",
            connection);
        command.Parameters.AddWithValue("actor_id", actorId);
        command.Parameters.AddWithValue("operation_key", operationKey);
        command.Parameters.AddWithValue("scope_id", scopeId);
        command.Parameters.AddWithValue("garage_id", garageId is null ? DBNull.Value : garageId.Value);
        command.Parameters.AddWithValue("request_hash", requestHash);
        command.Parameters.AddWithValue("resource_id", resourceId);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task InsertRecoveryAttemptAsync(NpgsqlConnection connection, Guid attemptId, DateOnly occurredOn)
    {
        await using var command = new NpgsqlCommand(
            "INSERT INTO garagemotor.recovery_attempts (attempt_id, account_key_hash, origin_key_hash, occurred_utc) " +
            "VALUES (@attempt_id, @account_key_hash, @origin_key_hash, @occurred_utc)",
            connection);
        command.Parameters.AddWithValue("attempt_id", attemptId);
        command.Parameters.AddWithValue("account_key_hash", new byte[32]);
        command.Parameters.AddWithValue("origin_key_hash", new byte[32]);
        command.Parameters.AddWithValue("occurred_utc", new DateTimeOffset(occurredOn.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero));
        await command.ExecuteNonQueryAsync();
    }

    private static async Task ExecuteAsync(
        NpgsqlConnection connection,
        string commandText,
        params NpgsqlParameter[] parameters)
    {
        await using var command = new NpgsqlCommand(commandText, connection);
        command.Parameters.AddRange(parameters);
        await command.ExecuteNonQueryAsync();
    }
}
