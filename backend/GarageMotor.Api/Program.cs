using GarageMotor.Api.Identity;
using GarageMotor.Api.Infrastructure;
using GarageMotor.Api.Operations;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Npgsql;

var logUnhandledRequest = LoggerMessage.Define<string, string>(
    LogLevel.Error,
    new EventId(5000, "UnhandledHttpRequest"),
    "Unhandled HTTP request failed: {ExceptionType}; trace {TraceId}.");
var builder = WebApplication.CreateBuilder(args);
var runtimeConnectionString = builder.Configuration.GetConnectionString("GarageMotor");
var migrationConnectionString = builder.Configuration.GetConnectionString("GarageMotorMigrations");
if (string.IsNullOrWhiteSpace(runtimeConnectionString) || string.IsNullOrWhiteSpace(migrationConnectionString))
{
    throw new InvalidOperationException("Runtime and migration database credentials must be configured externally.");
}

builder.Services.AddSingleton(new Database(runtimeConnectionString));
builder.Services.AddSingleton(IdentityStoreFactory.Create);
builder.Services.AddSingleton<RecoveryAttemptStore>();
builder.Services.AddSingleton<OperationStore>();
builder.Services.AddSingleton<AuditStore>();
builder.Services.AddGarageMotorSecurity(builder.Configuration);

var migrationDataSource = NpgsqlDataSource.Create(migrationConnectionString);
builder.Services.AddSingleton(migrationDataSource);
builder.Services.AddSingleton<PartitionManager>();
builder.Services.AddHostedService(serviceProvider => serviceProvider.GetRequiredService<PartitionManager>());

var app = builder.Build();
app.UseForwardedHeaders();
app.UseExceptionHandler(errorApplication => errorApplication.Run(async context =>
{
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    logUnhandledRequest(
        app.Logger,
        exception?.GetType().Name ?? "UnknownException",
        context.TraceIdentifier,
        null);
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    context.Response.ContentType = "application/problem+json";
    await context.Response.WriteAsJsonAsync(new
    {
        type = "about:blank",
        title = "Falha interna",
        status = StatusCodes.Status500InternalServerError,
        traceId = context.TraceIdentifier,
    });
}));
app.UseCors("GarageMotor");
app.UseGarageMotorSecurity();
app.MapInstanceEndpoints();
app.MapIdentityEndpoints();
app.Run();

internal static class IdentityStoreFactory
{
    public static IdentityStore Create(IServiceProvider serviceProvider)
    {
        return new IdentityStore(serviceProvider.GetRequiredService<Database>());
    }
}
