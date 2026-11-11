using Xunit;

namespace GarageMotor.Tests.Fixtures;

public static class TestCollectionNames
{
    public const string Integration = "PostgreSQL integration";
}

[CollectionDefinition(TestCollectionNames.Integration, DisableParallelization = true)]
public sealed class IntegrationTestCollectionDefinition
{
}
