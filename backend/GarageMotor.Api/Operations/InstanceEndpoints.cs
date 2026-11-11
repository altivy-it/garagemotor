using Microsoft.AspNetCore.Http;

namespace GarageMotor.Api.Operations;

public static class InstanceEndpoints
{
    public static IEndpointRouteBuilder MapInstanceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/instance", GetInstance).AllowAnonymous().WithName("getInstance");
        return endpoints;
    }

    private static IResult GetInstance(IConfiguration configuration)
    {
        var name = configuration["Instance:Name"];
        var minimumClientMajor = configuration.GetValue<int?>("Instance:MinimumClientMajor") ?? 1;
        if (minimumClientMajor < 1)
        {
            minimumClientMajor = 1;
        }

        return Results.Json(new InstanceResponse(
            string.IsNullOrWhiteSpace(name) ? "GarageMotor" : name.Trim(),
            ApiMajor: 1,
            MinimumClientMajor: minimumClientMajor));
    }

    private sealed record InstanceResponse(string Name, int ApiMajor, int MinimumClientMajor);
}
