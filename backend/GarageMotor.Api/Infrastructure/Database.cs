using Npgsql;

namespace GarageMotor.Api.Infrastructure;

public sealed class Database : IAsyncDisposable
{
    private readonly NpgsqlDataSource dataSource;

    public Database(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        dataSource = NpgsqlDataSource.Create(connectionString);
    }

    public static Database FromConfiguration(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var connectionString = configuration.GetConnectionString("GarageMotor");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:GarageMotor is required.");
        }

        return new Database(connectionString);
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Guid actorId,
        Guid? garageId,
        Func<NpgsqlConnection, NpgsqlTransaction, CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        if (actorId == Guid.Empty)
        {
            throw new ArgumentException("Actor ID must be a non-empty UUID.", nameof(actorId));
        }

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            await SetTransactionContextAsync(connection, transaction, actorId, garageId, cancellationToken);
            var result = await operation(connection, transaction, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    public ValueTask DisposeAsync()
    {
        return dataSource.DisposeAsync();
    }

    private static async Task SetTransactionContextAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Guid actorId,
        Guid? garageId,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(
            "SELECT set_config('app.actor_id', @actor_id, true), " +
            "set_config('app.garage_id', @garage_id, true)",
            connection,
            transaction);
        command.Parameters.AddWithValue("actor_id", actorId.ToString());
        command.Parameters.AddWithValue("garage_id", garageId?.ToString() ?? string.Empty);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
