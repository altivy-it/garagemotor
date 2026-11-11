using System.Text.Json;
using GarageMotor.Api.Infrastructure;
using GarageMotor.Api.Operations;
using GarageMotor.Tests.Fixtures;
using Npgsql;
using Xunit;

namespace GarageMotor.Tests.Operations;

[Collection(TestCollectionNames.Integration)]
public sealed class OperationStoreTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void RequestHashIsIndependentOfJsonPropertyOrder()
    {
        using var first = JsonDocument.Parse("{\"name\":\"garage\",\"timezone\":\"UTC\"}");
        using var second = JsonDocument.Parse("{\"timezone\":\"UTC\",\"name\":\"garage\"}");

        Assert.Equal(
            OperationStore.ComputeRequestHash("createGarage", first.RootElement),
            OperationStore.ComputeRequestHash("createGarage", second.RootElement));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task RepeatedCreationReplaysOnlyAfterActorScopedHashAndAuthorizationChecks()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__GarageMotor");
        Assert.False(string.IsNullOrWhiteSpace(connectionString), "A real PostgreSQL connection is required.");

        await using var dataSource = NpgsqlDataSource.Create(connectionString!);
        var actorId = Guid.NewGuid();
        var otherActorId = Guid.NewGuid();
        var email = $"operation-{Guid.NewGuid():N}@example.test";
        await CreateTestActorAsync(dataSource, actorId, email);
        await CreateTestActorAsync(dataSource, otherActorId, $"other-{Guid.NewGuid():N}@example.test");

        try
        {
            await using var database = new Database(connectionString!);
            var store = new OperationStore(database);
            var operationKey = Guid.NewGuid();
            var garageId = Guid.NewGuid();
            var request = JsonDocument.Parse("{\"name\":\"Personal garage\",\"timezone\":\"UTC\"}");
            using (request)
            {
                var createCount = 0;
                var authorizeCount = 0;
                var createAsync = (NpgsqlConnection _, NpgsqlTransaction _, CancellationToken _) =>
                {
                    createCount++;
                    return Task.FromResult(new OperationResponse(
                        201,
                        garageId,
                        1,
                        "\"1\"",
                        $$"""{"id":"{{garageId}}","kind":"garage"}""",
                        garageId));
                };
                var authorizeAsync = (NpgsqlConnection _, NpgsqlTransaction _, OperationResponse _, CancellationToken _) =>
                {
                    authorizeCount++;
                    return Task.FromResult(true);
                };

                var initial = await store.ExecuteAsync(
                    actorId,
                    operationKey,
                    "createGarage",
                    new OperationScope(OperationScopeKind.Actor, actorId, null),
                    request.RootElement,
                    createAsync,
                    authorizeAsync);
                var replay = await store.ExecuteAsync(
                    actorId,
                    operationKey,
                    "createGarage",
                    new OperationScope(OperationScopeKind.Actor, actorId, null),
                    request.RootElement,
                    createAsync,
                    authorizeAsync);

                Assert.Equal(201, initial.Response.StatusCode);
                Assert.False(initial.IsReplay);
                Assert.Equal(1, initial.Response.Version);
                Assert.True(replay.IsReplay);
                Assert.Equal(initial.Response.ResourceId, replay.Response.ResourceId);
                Assert.Equal(initial.Response.Version, replay.Response.Version);
                Assert.Equal(initial.Response.ETag, replay.Response.ETag);
                Assert.Equal(1, createCount);
                Assert.Equal(1, authorizeCount);

                using var changedRequest = JsonDocument.Parse("{\"name\":\"Different garage\",\"timezone\":\"UTC\"}");
                var conflict = await store.ExecuteAsync(
                    actorId,
                    operationKey,
                    "createGarage",
                    new OperationScope(OperationScopeKind.Actor, actorId, null),
                    changedRequest.RootElement,
                    createAsync,
                    authorizeAsync);
                Assert.Equal(409, conflict.Response.StatusCode);
                Assert.True(conflict.IsReplay);
                Assert.Equal(1, createCount);

                var operationConflict = await store.ExecuteAsync(
                    actorId,
                    operationKey,
                    "createVehicle",
                    new OperationScope(OperationScopeKind.Actor, actorId, null),
                    request.RootElement,
                    createAsync,
                    authorizeAsync);
                Assert.Equal(409, operationConflict.Response.StatusCode);
                Assert.Equal(1, createCount);

                var deniedReplay = await store.ExecuteAsync(
                    actorId,
                    operationKey,
                    "createGarage",
                    new OperationScope(OperationScopeKind.Actor, actorId, null),
                    request.RootElement,
                    createAsync,
                    (_, _, _, _) => Task.FromResult(false));
                Assert.Equal(404, deniedReplay.Response.StatusCode);
                Assert.True(deniedReplay.IsReplay);

                var anotherActorCreation = await store.ExecuteAsync(
                    otherActorId,
                    operationKey,
                    "createGarage",
                    new OperationScope(OperationScopeKind.Actor, otherActorId, null),
                    request.RootElement,
                    createAsync,
                    authorizeAsync);
                Assert.Equal(201, anotherActorCreation.Response.StatusCode);
                Assert.False(anotherActorCreation.IsReplay);
                Assert.Equal(2, createCount);
            }
        }
        finally
        {
            await DeleteTestActorAsync(dataSource, actorId);
            await DeleteTestActorAsync(dataSource, otherActorId);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task AllDomainCreationOperationNamesReplayWithinTheirActorOrGarageScope()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__GarageMotor");
        Assert.False(string.IsNullOrWhiteSpace(connectionString), "A real PostgreSQL connection is required.");

        await using var dataSource = NpgsqlDataSource.Create(connectionString!);
        var actorId = Guid.NewGuid();
        await CreateTestActorAsync(dataSource, actorId, $"all-operations-{Guid.NewGuid():N}@example.test");

        try
        {
            await using var database = new Database(connectionString!);
            var store = new OperationStore(database);
            var garageId = Guid.NewGuid();
            var operations = new[]
            {
                "createGarage",
                "createVehicle",
                "createInvite",
                "createExpense",
                "createFuelRecord",
                "createElectricCharge",
                "createElectricityTariff",
                "createMaintenance",
                "createPlan",
                "createCommitment",
                "createAttachment",
                "createExport",
            };

            foreach (var operationName in operations)
            {
                var operationKey = Guid.NewGuid();
                var resourceId = Guid.NewGuid();
                var isGarageCreation = operationName == "createGarage";
                var scope = isGarageCreation
                    ? new OperationScope(OperationScopeKind.Actor, actorId, null)
                    : new OperationScope(OperationScopeKind.Garage, garageId, garageId);
                using var request = JsonDocument.Parse("{\"request\":\"same\"}");
                var createCount = 0;
                var createAsync = (NpgsqlConnection _, NpgsqlTransaction _, CancellationToken _) =>
                {
                    createCount++;
                    return Task.FromResult(new OperationResponse(
                        201,
                        resourceId,
                        1,
                        "\"1\"",
                        "{}",
                        isGarageCreation ? garageId : null));
                };
                var reauthorize = (NpgsqlConnection _, NpgsqlTransaction _, OperationResponse _, CancellationToken _) =>
                    Task.FromResult(true);

                var created = await store.ExecuteAsync(
                    actorId,
                    operationKey,
                    operationName,
                    scope,
                    request.RootElement,
                    createAsync,
                    reauthorize);
                var replay = await store.ExecuteAsync(
                    actorId,
                    operationKey,
                    operationName,
                    scope,
                    request.RootElement,
                    createAsync,
                    reauthorize);

                Assert.Equal(201, created.Response.StatusCode);
                Assert.False(created.IsReplay, operationName);
                Assert.Equal(201, replay.Response.StatusCode);
                Assert.True(replay.IsReplay, operationName);
                Assert.Equal(resourceId, replay.Response.ResourceId);
                Assert.Equal(1, createCount);
            }
        }
        finally
        {
            await DeleteTestActorAsync(dataSource, actorId);
        }
    }

    private static async Task CreateTestActorAsync(NpgsqlDataSource dataSource, Guid userId, string email)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(
            "INSERT INTO garagemotor.users (user_id, email_normalized, display_name, password_hash) " +
            "VALUES (@user_id, @email, 'Operation test', 'not-a-credential')",
            connection);
        command.Parameters.AddWithValue("user_id", userId);
        command.Parameters.AddWithValue("email", email);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task DeleteTestActorAsync(NpgsqlDataSource dataSource, Guid userId)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(
            "DELETE FROM garagemotor.users WHERE user_id = @user_id",
            connection);
        command.Parameters.AddWithValue("user_id", userId);
        await command.ExecuteNonQueryAsync();
    }
}
