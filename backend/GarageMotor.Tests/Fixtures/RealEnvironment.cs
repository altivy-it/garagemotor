using Npgsql;

namespace GarageMotor.Tests.Fixtures;

internal static class RealEnvironment
{
    public static Uri ProxyBaseUri
    {
        get
        {
            var value = Environment.GetEnvironmentVariable("GARAGEMOTOR_BASE_URL") ?? "http://nginx:8080";
            return new Uri(value, UriKind.Absolute);
        }
    }

    public static NpgsqlDataSource CreateDataSource()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__GarageMotor");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings__GarageMotor is required for real integration tests.");
        }

        return NpgsqlDataSource.Create(connectionString);
    }

    public static NpgsqlDataSource CreateMigrationDataSource()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__GarageMotorMigrations");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings__GarageMotorMigrations is required for migration integration tests.");
        }

        return NpgsqlDataSource.Create(connectionString);
    }

    public static TemporaryDirectory CreateTemporaryDirectory()
    {
        return new TemporaryDirectory();
    }
}

internal sealed class TemporaryDirectory : IDisposable
{
    public string Path { get; }

    public TemporaryDirectory()
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "garagemotor-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path);
    }

    public void Dispose()
    {
        if (Directory.Exists(Path))
        {
            Directory.Delete(Path, recursive: true);
        }
    }
}
