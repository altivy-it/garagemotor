using System.Security.Claims;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using GarageMotor.Api.Identity;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.DataProtection;

namespace GarageMotor.Api.Infrastructure;

public static class SecurityConfiguration
{
    public static IServiceCollection AddGarageMotorSecurity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var proxyAddress = configuration["Security:TrustedProxy"];
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
            options.ForwardLimit = 1;
            if (!string.IsNullOrWhiteSpace(proxyAddress))
            {
                if (!IPAddress.TryParse(proxyAddress, out var address))
                {
                    throw new InvalidOperationException("Trusted proxy address is invalid.");
                }

                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
                options.KnownProxies.Add(address);
            }
        });

        var keysPath = configuration["Security:DataProtection:KeysPath"];
        if (!string.IsNullOrWhiteSpace(keysPath))
        {
            var normalizedPath = Path.GetFullPath(keysPath);
            if (!string.Equals(normalizedPath, "/var/lib/garagemotor/keys", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Data protection path is outside the approved private volume.");
            }

            var certificateValue = configuration["Security:DataProtection:CertificateBase64"];
            var certificatePassword = configuration["Security:DataProtection:CertificatePassword"];
            if (string.IsNullOrWhiteSpace(certificateValue) || string.IsNullOrWhiteSpace(certificatePassword))
            {
                throw new InvalidOperationException("Data protection certificate must be configured externally.");
            }

            var certificate = X509CertificateLoader.LoadPkcs12(
                Convert.FromBase64String(certificateValue), certificatePassword, X509KeyStorageFlags.EphemeralKeySet);
            services.AddSingleton(certificate);
            services.AddDataProtection()
                .SetApplicationName("GarageMotor")
                .PersistKeysToFileSystem(new DirectoryInfo(normalizedPath))
                .ProtectKeysWithCertificate(certificate);
        }

        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-Token";
            options.Cookie.Name = "__Host-gm-antiforgery";
            options.Cookie.Path = "/";
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });
        services.AddCors(options => options.AddPolicy("GarageMotor", policy =>
        {
            var origins = configuration.GetSection("Security:TrustedOrigins").Get<string[]>() ?? [];
            if (origins.Length > 0)
            {
                policy.WithOrigins(origins)
                    .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                    .WithHeaders("Content-Type", "Authorization", "If-Match", "Idempotency-Key", "X-CSRF-Token")
                    .AllowCredentials();
            }
        }));

        return services;
    }

    public static IApplicationBuilder UseGarageMotorSecurity(this IApplicationBuilder application)
    {
        ArgumentNullException.ThrowIfNull(application);
        return application.UseMiddleware<SessionSecurityMiddleware>();
    }
}

public sealed class SessionSecurityMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IdentityStore identityStore, IConfiguration configuration, IAntiforgery antiforgery)
    {
        if (!context.Request.Path.StartsWithSegments("/api/v1"))
        {
            await next(context);
            return;
        }

        var path = context.Request.Path.Value;
        if (context.Request.Method == HttpMethods.Get && path is "/api/v1/csrf" or "/api/v1/instance")
        {
            await next(context);
            return;
        }

        var isPublicAuthenticationEndpoint = context.Request.Method == HttpMethods.Post &&
            path is "/api/v1/accounts" or "/api/v1/sessions" or "/api/v1/recovery-requests" or "/api/v1/recovery-confirmations";
        if (isPublicAuthenticationEndpoint)
        {
            await next(context);
            return;
        }

        var cookieToken = context.Request.Cookies[IdentityEndpoints.SessionCookieName];
        var isCookieAuthenticated = !string.IsNullOrWhiteSpace(cookieToken);
        var token = cookieToken;
        if (!isCookieAuthenticated &&
            context.Request.Headers.Authorization.FirstOrDefault() is { } authorization &&
            authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = authorization[7..].Trim();
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            await ProblemResponses.Create(context, StatusCodes.Status401Unauthorized).ExecuteAsync(context);
            return;
        }

        if (isCookieAuthenticated && IsUnsafeMethod(context.Request.Method))
        {
            if (!IsTrustedOrigin(context, configuration))
            {
                await ProblemResponses.Create(context, StatusCodes.Status403Forbidden).ExecuteAsync(context);
                return;
            }

            try
            {
                await antiforgery.ValidateRequestAsync(context);
            }
            catch (AntiforgeryValidationException)
            {
                await ProblemResponses.Create(context, StatusCodes.Status400BadRequest).ExecuteAsync(context);
                return;
            }
        }

        var session = await identityStore.FindActiveSessionAsync(token, context.RequestAborted);
        if (session is null)
        {
            await ProblemResponses.Create(context, StatusCodes.Status401Unauthorized).ExecuteAsync(context);
            return;
        }

        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, session.UserId.ToString("D"))],
            authenticationType: "GarageMotorSession");
        context.User = new ClaimsPrincipal(identity);
        context.Items[IdentityEndpoints.SessionTokenItemKey] = token;
        await next(context);
    }

    private static bool IsUnsafeMethod(string method)
    {
        return !string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(method, "HEAD", StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(method, "OPTIONS", StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(method, "TRACE", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTrustedOrigin(HttpContext context, IConfiguration configuration)
    {
        if (!context.Request.Headers.TryGetValue("Origin", out var originValues) ||
            !Uri.TryCreate(originValues.ToString(), UriKind.Absolute, out var origin))
        {
            return false;
        }

        var configuredOrigins = configuration.GetSection("Security:TrustedOrigins").Get<string[]>() ?? [];
        if (configuredOrigins.Length > 0)
        {
            return configuredOrigins.Any(configured => Uri.TryCreate(configured, UriKind.Absolute, out var allowed) &&
                string.Equals(origin.GetLeftPart(UriPartial.Authority), allowed.GetLeftPart(UriPartial.Authority), StringComparison.OrdinalIgnoreCase));
        }

        var requestOrigin = $"{context.Request.Scheme}://{context.Request.Host}";
        return string.Equals(origin.GetLeftPart(UriPartial.Authority), requestOrigin, StringComparison.OrdinalIgnoreCase);
    }
}
