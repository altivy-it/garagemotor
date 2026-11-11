using GarageMotor.Api.Infrastructure;
using GarageMotor.Api.Operations;
using GarageMotor.Tests.Fixtures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql;
using Xunit;

namespace GarageMotor.Tests.Foundation;

[Collection(TestCollectionNames.Integration)]
public sealed class AuditTests
{
    private static readonly string[] ExpectedChangedFields = ["amountMinor", "spentOn"];
    private static readonly string[] ExpectedAuditColumns =
    [
        "event_id",
        "garage_id",
        "occurred_utc",
        "actor_id",
        "action",
        "target_id",
        "changed_fields",
        "trace_id",
    ];

    [Fact]
    [Trait("Category", "Integration")]
    public async Task AuditEventsAreLimitedToTheCurrentGarageAndStoreFieldNamesOnly()
    {
        await using var database = new Database(
            Environment.GetEnvironmentVariable("ConnectionStrings__GarageMotor")
            ?? throw new InvalidOperationException("A real PostgreSQL connection is required."));
        var store = new AuditStore();
        var actorId = Guid.NewGuid();
        var garageId = Guid.NewGuid();
        var inserted = await database.ExecuteInTransactionAsync(
            actorId,
            garageId,
            async (connection, transaction, cancellationToken) =>
            {
                await store.AppendAsync(
                    connection,
                    transaction,
                    garageId,
                    actorId,
                    "expense.updated",
                    $"expense:{Guid.NewGuid():N}",
                    ["amountMinor", "spentOn"],
                    "trace-test",
                    cancellationToken);

                await using var command = new NpgsqlCommand(
                    "SELECT event_id, changed_fields FROM garagemotor.audit_events " +
                    "WHERE garage_id = @garage_id AND action = 'expense.updated'",
                    connection,
                    transaction);
                command.Parameters.AddWithValue("garage_id", garageId);
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                Assert.True(await reader.ReadAsync(cancellationToken));
                var result = (EventId: reader.GetGuid(0), ChangedFields: reader.GetFieldValue<string[]>(1));
                Assert.False(await reader.ReadAsync(cancellationToken));
                return result;
            }, TestContext.Current.CancellationToken);

        Assert.NotEqual(Guid.Empty, inserted.EventId);
        Assert.Equal(ExpectedChangedFields.Order(), inserted.ChangedFields.Order());

        var otherGarageCount = await database.ExecuteInTransactionAsync(
            actorId,
            Guid.NewGuid(),
            async (connection, transaction, cancellationToken) =>
            {
                await using var command = new NpgsqlCommand(
                    "SELECT count(*) FROM garagemotor.audit_events WHERE garage_id = @garage_id",
                    connection,
                    transaction);
                command.Parameters.AddWithValue("garage_id", garageId);
                return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken), System.Globalization.CultureInfo.InvariantCulture);
            }, TestContext.Current.CancellationToken);
        Assert.Equal(0, otherGarageCount);

        await using var schemaDataSource = NpgsqlDataSource.Create(
            Environment.GetEnvironmentVariable("ConnectionStrings__GarageMotor")!);
        await using var schemaConnection = await schemaDataSource.OpenConnectionAsync(TestContext.Current.CancellationToken);
        await using var columnsCommand = new NpgsqlCommand(
            "SELECT column_name FROM information_schema.columns " +
            "WHERE table_schema = 'garagemotor' AND table_name = 'audit_events' ORDER BY ordinal_position",
            schemaConnection);
        await using var columnsReader = await columnsCommand.ExecuteReaderAsync(TestContext.Current.CancellationToken);
        var columns = new List<string>();
        while (await columnsReader.ReadAsync(TestContext.Current.CancellationToken))
        {
            columns.Add(columnsReader.GetString(0));
        }

        Assert.Equal(ExpectedAuditColumns.Order(), columns.Order());
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ProblemResponsesUseContractStatusAndProblemJsonWithoutExceptionDetails()
    {
        var context = new DefaultHttpContext();
        context.TraceIdentifier = "trace-test";

        var result = ProblemResponses.Create(context, StatusCodes.Status409Conflict);
        var problem = result as ProblemHttpResult;

        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status409Conflict, problem.StatusCode);
        Assert.Equal("application/problem+json", problem.ContentType);
        Assert.Equal("trace-test", problem.ProblemDetails.Extensions["traceId"]);
        Assert.False(problem.ProblemDetails.Detail?.Contains("Exception", StringComparison.OrdinalIgnoreCase) ?? false);

        var rejectedStatus = false;
        try
        {
            _ = ProblemResponses.Create(context, StatusCodes.Status500InternalServerError);
        }
        catch (ArgumentOutOfRangeException)
        {
            rejectedStatus = true;
        }

        Assert.True(rejectedStatus);
    }
}
