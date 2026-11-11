using System.Security.Claims;
using GarageMotor.Api.Infrastructure;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace GarageMotor.Api.Identity;

public interface IRecoveryMessageSender
{
    Task SendRecoveryCodeAsync(string email, string code, DateTimeOffset expiresUtc, CancellationToken cancellationToken);
}

public static class IdentityEndpoints
{
    public const string SessionCookieName = "__Host-gm-session";

    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1");
        group.MapPost("/accounts", RegisterAccountAsync).WithName("registerAccount");
        group.MapPost("/sessions", CreateSessionAsync).WithName("createSession");
        group.MapDelete("/sessions", RevokeSessionAsync).WithName("revokeSession");
        group.MapGet("/csrf", GetCsrfToken).WithName("getCsrfToken");
        group.MapPost("/recovery-requests", RequestRecoveryAsync).WithName("requestRecovery");
        group.MapPost("/recovery-confirmations", ConfirmRecoveryAsync).WithName("confirmRecovery");
        group.MapPost("/recovery-codes", RenewOfflineCodesAsync).WithName("renewOfflineRecoveryCodes");
        return endpoints;
    }

    private static async Task<IResult> RegisterAccountAsync(
        RegistrationRequest request,
        IdentityStore store,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (request.Email is null || request.Name is null || request.Password is null ||
            request.Email.Length > 254 || request.Name.Length is < 1 or > 100 || request.Password.Length is < 12 or > 128)
        {
            return ProblemResponses.Create(context, StatusCodes.Status422UnprocessableEntity);
        }

        try
        {
            await store.RegisterAccountAsync(request.Email, request.Name, request.Password, cancellationToken);
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return Results.Accepted();
        }
        catch (ArgumentException)
        {
            return ProblemResponses.Create(context, StatusCodes.Status422UnprocessableEntity);
        }

        return Results.Accepted();
    }

    private static async Task<IResult> CreateSessionAsync(
        LoginRequest request,
        IdentityStore store,
        RecoveryAttemptStore attempts,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (request.Email is null || request.Password is null || request.Channel is not ("web" or "android") || request.Email.Length > 254 || request.Password.Length > 128)
        {
            return ProblemResponses.Create(context, StatusCodes.Status422UnprocessableEntity);
        }

        AccountRecord? account;
        try
        {
            account = await store.FindAccountByEmailAsync(request.Email, cancellationToken);
        }
        catch (ArgumentException)
        {
            account = null;
        }

        if (account is null || !store.VerifyPassword(account, request.Password))
        {
            var decision = await attempts.RecordFailureAsync(request.Email, GetOriginKey(context), cancellationToken);
            return ProblemResponses.Create(
                context,
                decision.Allowed ? StatusCodes.Status422UnprocessableEntity : StatusCodes.Status429TooManyRequests);
        }

        var session = await store.CreateSessionAsync(account.UserId, cancellationToken);
        context.Response.Headers.CacheControl = "no-store";
        if (request.Channel == "web")
        {
            context.Response.Cookies.Append(
                SessionCookieName,
                session.Token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    Path = "/",
                    SameSite = SameSiteMode.Strict,
                    Expires = session.ExpiresUtc,
                    IsEssential = true,
                });
            return Results.Json(
                new { userId = account.UserId, expiresUtc = session.ExpiresUtc },
                statusCode: StatusCodes.Status200OK);
        }

        return Results.Json(
            new AndroidSessionResponse(account.UserId, session.ExpiresUtc, session.Token),
            statusCode: StatusCodes.Status200OK);
    }

    private static async Task<IResult> RevokeSessionAsync(
        IdentityStore store,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedUserId(context, out var userId) ||
            !context.Items.TryGetValue(SessionTokenItemKey, out var tokenValue) || tokenValue is not string token)
        {
            return ProblemResponses.Create(context, StatusCodes.Status401Unauthorized);
        }

        await store.RevokeSessionAsync(userId, token, cancellationToken);
        context.Response.Cookies.Delete(SessionCookieName, new CookieOptions { Path = "/", Secure = true, HttpOnly = true, SameSite = SameSiteMode.Strict });
        return Results.NoContent();
    }

    private static IResult GetCsrfToken(HttpContext context, IAntiforgery antiforgery)
    {
        var tokens = antiforgery.GetAndStoreTokens(context);
        context.Response.Headers.CacheControl = "no-store";
        return Results.Json(new CsrfTokenResponse(tokens.RequestToken ?? string.Empty));
    }

    private static async Task<IResult> RequestRecoveryAsync(
        RecoveryRequest request,
        IdentityStore store,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || request.Email.Length > 254)
        {
            return Results.Accepted();
        }

        AccountRecord? account;
        try
        {
            account = await store.FindAccountByEmailAsync(request.Email, cancellationToken);
        }
        catch (ArgumentException)
        {
            account = null;
        }

        var sender = context.RequestServices.GetService<IRecoveryMessageSender>();
        if (account is not null && sender is not null)
        {
            var code = await store.CreateSmtpRecoveryCodeAsync(account.UserId, cancellationToken);
            if (code is not null)
            {
                await sender.SendRecoveryCodeAsync(
                    account.EmailNormalized,
                    code,
                    DateTimeOffset.UtcNow.AddMinutes(15),
                    cancellationToken);
            }
        }

        return Results.Accepted();
    }

    private static async Task<IResult> ConfirmRecoveryAsync(
        RecoveryRequestConfirmation request,
        IdentityStore store,
        RecoveryAttemptStore attempts,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var validShape = request.Email is not null && request.Email.Length <= 254 &&
                         request.Code is not null && IdentityStore.IsValidRecoveryCode(request.Code) &&
                         request.NewPassword is not null && request.NewPassword.Length is >= 12 and <= 128;
        if (validShape)
        {
            try
            {
                if (await store.ConfirmRecoveryAsync(request.Email!, request.Code!, request.NewPassword!, cancellationToken))
                {
                    return Results.NoContent();
                }
            }
            catch (ArgumentException)
            {
            }
        }

        var accountKey = request.Email is { Length: <= 254 } ? request.Email : "invalid-account";
        var decision = await attempts.RecordFailureAsync(accountKey, GetOriginKey(context), cancellationToken);
        return ProblemResponses.Create(
            context,
            decision.Allowed ? StatusCodes.Status422UnprocessableEntity : StatusCodes.Status429TooManyRequests);
    }

    private static async Task<IResult> RenewOfflineCodesAsync(
        RenewRecoveryCodesRequest request,
        IdentityStore store,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedUserId(context, out var userId) || request.CurrentPassword is null)
        {
            return ProblemResponses.Create(context, StatusCodes.Status401Unauthorized);
        }

        try
        {
            var batch = await store.RenewOfflineRecoveryCodesAsync(userId, request.CurrentPassword, cancellationToken);
            context.Response.Headers.CacheControl = "no-store";
            return Results.Json(batch);
        }
        catch (InvalidOperationException)
        {
            return ProblemResponses.Create(context, StatusCodes.Status422UnprocessableEntity);
        }
        catch (ArgumentException)
        {
            return ProblemResponses.Create(context, StatusCodes.Status422UnprocessableEntity);
        }
    }

    private static bool TryGetAuthenticatedUserId(HttpContext context, out Guid userId)
    {
        var value = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out userId) && userId != Guid.Empty;
    }

    internal static string GetOriginKey(HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown-origin";
    }

    internal const string SessionTokenItemKey = "GarageMotor.SessionToken";

    private sealed record RegistrationRequest(string? Email, string? Name, string? Password);
    private sealed record LoginRequest(string? Email, string? Password, string? Channel);
    private sealed record RecoveryRequest(string? Email);
    private sealed record RecoveryRequestConfirmation(string? Email, string? Code, string? NewPassword);
    private sealed record RenewRecoveryCodesRequest(string? CurrentPassword);
    private sealed record AndroidSessionResponse(Guid UserId, DateTimeOffset ExpiresUtc, string Token);
    private sealed record CsrfTokenResponse(string RequestToken);
}
