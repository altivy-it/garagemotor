using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using GarageMotor.Api.Identity;
using GarageMotor.Api.Infrastructure;
using GarageMotor.Tests.Fixtures;
using Npgsql;
using Xunit;

namespace GarageMotor.Tests.Foundation;

[Collection(TestCollectionNames.Integration)]
public sealed class SecurityTests
{
    private const string InitialPassword = "GarageMotor-Test-Only-Password-2026!";

    [Fact]
    [Trait("Category", "Integration")]
    public async Task RecoveryRequestsDoNotRevealWhetherAccountExists()
    {
        using var account = await CreateWebAccountAsync();
        using var knownResponse = await account.Client.PostAsJsonAsync(
            "/api/v1/recovery-requests",
            new { email = account.Email });
        using var unknownResponse = await account.Client.PostAsJsonAsync(
            "/api/v1/recovery-requests",
            new { email = $"missing-{Guid.NewGuid():N}@example.test" });

        Assert.Equal(HttpStatusCode.Accepted, knownResponse.StatusCode);
        Assert.Equal(knownResponse.StatusCode, unknownResponse.StatusCode);

        var knownBody = await knownResponse.Content.ReadAsStringAsync();
        var unknownBody = await unknownResponse.Content.ReadAsStringAsync();
        Assert.Equal(knownBody, unknownBody);
        Assert.False(knownBody.Contains(account.Email, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task WebSessionRequiresCsrfAndTrustedOriginAndCanBeRevoked()
    {
        using var account = await CreateWebAccountAsync();
        using var missingCsrfRequest = CreateJsonRequest(
            HttpMethod.Post,
            "/api/v1/recovery-codes",
            new { currentPassword = InitialPassword });
        using var missingCsrfResponse = await account.Client.SendAsync(missingCsrfRequest);
        Assert.True(
            missingCsrfResponse.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Forbidden,
            $"Mutation without an antiforgery token returned {(int)missingCsrfResponse.StatusCode}.");

        var csrfToken = await GetCsrfTokenAsync(account);
        using var foreignOriginRequest = CreateJsonRequest(
            HttpMethod.Post,
            "/api/v1/recovery-codes",
            new { currentPassword = InitialPassword });
        foreignOriginRequest.Headers.Add("Origin", "https://untrusted.invalid");
        foreignOriginRequest.Headers.Add("X-CSRF-Token", csrfToken);
        using var foreignOriginResponse = await account.Client.SendAsync(foreignOriginRequest);
        Assert.Equal(HttpStatusCode.Forbidden, foreignOriginResponse.StatusCode);

        var trustedOrigin = account.Client.BaseAddress!.GetLeftPart(UriPartial.Authority);
        using var revokeRequest = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/sessions");
        revokeRequest.Headers.Add("Origin", trustedOrigin);
        revokeRequest.Headers.Add("X-CSRF-Token", csrfToken);
        using var revokeResponse = await account.Client.SendAsync(revokeRequest);
        Assert.Equal(HttpStatusCode.NoContent, revokeResponse.StatusCode);

        using var garagesResponse = await account.Client.GetAsync("/api/v1/garages");
        Assert.Equal(HttpStatusCode.Unauthorized, garagesResponse.StatusCode);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task OfflineRecoveryBatchContainsTenUniqueCodesAndStoresOnlyHashes()
    {
        using var account = await CreateWebAccountAsync();
        var csrfToken = await GetCsrfTokenAsync(account);
        var issuedAt = DateTimeOffset.UtcNow;
        var trustedOrigin = account.Client.BaseAddress!.GetLeftPart(UriPartial.Authority);
        using var request = CreateJsonRequest(
            HttpMethod.Post,
            "/api/v1/recovery-codes",
            new { currentPassword = InitialPassword });
        request.Headers.Add("Origin", trustedOrigin);
        request.Headers.Add("X-CSRF-Token", csrfToken);

        using var response = await account.Client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.CacheControl?.NoStore == true);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var codes = document.RootElement
            .GetProperty("codes")
            .EnumerateArray()
            .Select(code => code.GetString()!)
            .ToArray();
        var expiresUtc = document.RootElement.GetProperty("expiresUtc").GetDateTimeOffset();

        Assert.Equal(10, codes.Length);
        Assert.Equal(10, codes.Distinct(StringComparer.Ordinal).Count());
        Assert.True(codes.All(code => code.Length == 22 && code.All(IsBase64UrlCharacter)));
        Assert.True(expiresUtc >= issuedAt.AddMonths(12).AddMinutes(-1));
        Assert.True(expiresUtc <= issuedAt.AddMonths(12).AddMinutes(1));

        await using var dataSource = RealEnvironment.CreateDataSource();
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT code_hash FROM recovery_codes WHERE user_id = @user_id AND kind::text = 'offline'",
            connection);
        command.Parameters.AddWithValue("user_id", account.UserId);
        await using var reader = await command.ExecuteReaderAsync();

        var persistedHashes = new HashSet<string>(StringComparer.Ordinal);
        while (await reader.ReadAsync())
        {
            persistedHashes.Add(Convert.ToHexString(reader.GetFieldValue<byte[]>(0)));
        }

        Assert.Equal(10, persistedHashes.Count);
        Assert.DoesNotContain(codes, code => persistedHashes.Contains(code));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FailedRecoveryCodesAreLimitedPerAccountAndConcurrentAttemptsAreAtomic()
    {
        using var client = CreateHttpClient();
        var email = $"attempts-{Guid.NewGuid():N}@example.test";
        var invalidRecovery = new
        {
            email,
            code = "AAAAAAAAAAAAAAAAAAAAAA",
            newPassword = "GarageMotor-New-Test-Password-2026!",
        };

        for (var attempt = 0; attempt < 5; attempt++)
        {
            using var response = await client.PostAsJsonAsync("/api/v1/recovery-confirmations", invalidRecovery);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        using var blockedResponse = await client.PostAsJsonAsync("/api/v1/recovery-confirmations", invalidRecovery);
        Assert.Equal(HttpStatusCode.TooManyRequests, blockedResponse.StatusCode);

        var concurrentEmail = $"concurrent-{Guid.NewGuid():N}@example.test";
        var requests = Enumerable.Range(0, 8)
            .Select(_ => client.PostAsJsonAsync(
                "/api/v1/recovery-confirmations",
                invalidRecovery with { email = concurrentEmail }))
            .ToArray();
        var responses = await Task.WhenAll(requests);
        try
        {
            Assert.True(
                responses.Any(response => response.StatusCode == HttpStatusCode.TooManyRequests),
                "Concurrent failures must not exceed the per-account limit.");
            Assert.True(responses.All(response => response.StatusCode is
                HttpStatusCode.UnprocessableEntity or HttpStatusCode.TooManyRequests));
        }
        finally
        {
            foreach (var response in responses)
            {
                response.Dispose();
            }
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SmtpRecoveryCodeExpiresWithinFifteenMinutesAndPersistsOnlyHash()
    {
        var connectionString = GetConnectionString();
        await using var database = new Database(connectionString);
        var store = new IdentityStore(database);
        var email = $"smtp-recovery-{Guid.NewGuid():N}@example.test";
        var userId = await store.RegisterAccountAsync(email, "SMTP recovery test", InitialPassword);

        try
        {
            var code = await store.CreateSmtpRecoveryCodeAsync(userId);
            Assert.NotNull(code);
            Assert.Equal(22, code!.Length);

            await using var dataSource = NpgsqlDataSource.Create(connectionString);
            await using var connection = await dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(
                "SELECT code_hash, EXTRACT(EPOCH FROM (expires_utc - created_utc)) " +
                "FROM garagemotor.recovery_codes WHERE user_id = @user_id AND kind = 'smtp'",
                connection);
            command.Parameters.AddWithValue("user_id", userId);
            await using var reader = await command.ExecuteReaderAsync();
            Assert.True(await reader.ReadAsync());
            Assert.Equal(
                System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(code)),
                reader.GetFieldValue<byte[]>(0));
            var lifetimeSeconds = reader.GetDouble(1);
            Assert.True(lifetimeSeconds > 0 && lifetimeSeconds <= TimeSpan.FromMinutes(15).TotalSeconds);
            Assert.False(await reader.ReadAsync());
        }
        finally
        {
            await DeleteTestUserAsync(connectionString, userId);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task RegistrationStoresPasswordHashAndSessionStoresOnlyTokenHash()
    {
        var connectionString = GetConnectionString();
        await using var database = new Database(connectionString);
        var store = new IdentityStore(database);
        var email = $"identity-{Guid.NewGuid():N}@example.test";
        var userId = await store.RegisterAccountAsync(email, "Identity test", InitialPassword);

        try
        {
            var account = await store.FindAccountByEmailAsync(email.ToUpperInvariant());
            Assert.NotNull(account);
            Assert.Equal(email.ToLowerInvariant(), account.EmailNormalized);
            Assert.False(string.Equals(InitialPassword, account.PasswordHash, StringComparison.Ordinal));
            Assert.True(store.VerifyPassword(account, InitialPassword));
            Assert.False(store.VerifyPassword(account, "Wrong-Password-For-Test-2026!"));

            var session = await store.CreateSessionAsync(userId);
            Assert.Equal(43, session.Token.Length);
            Assert.True(session.ExpiresUtc >= DateTimeOffset.UtcNow.AddHours(12).AddMinutes(-1));
            Assert.True(session.ExpiresUtc <= DateTimeOffset.UtcNow.AddHours(12).AddMinutes(1));
            Assert.True(await store.RevokeSessionAsync(userId, session.Token));

            await using var dataSource = NpgsqlDataSource.Create(connectionString);
            await using var connection = await dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(
                "SELECT count(*) FROM garagemotor.sessions WHERE user_id = @user_id AND token_hash = @token_hash",
                connection);
            command.Parameters.AddWithValue("user_id", userId);
            command.Parameters.AddWithValue("token_hash", System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(session.Token)));
            Assert.Equal(1L, await command.ExecuteScalarAsync());
        }
        finally
        {
            await DeleteTestUserAsync(connectionString, userId);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task OfflineRecoveryUsesUniqueHashedCodesAndSuccessRevokesSessions()
    {
        var connectionString = GetConnectionString();
        await using var database = new Database(connectionString);
        var store = new IdentityStore(database);
        var email = $"recovery-{Guid.NewGuid():N}@example.test";
        var userId = await store.RegisterAccountAsync(email, "Recovery test", InitialPassword);

        try
        {
            var session = await store.CreateSessionAsync(userId);
            var smtpCode = await store.CreateSmtpRecoveryCodeAsync(userId);
            Assert.NotNull(smtpCode);
            Assert.Equal(22, smtpCode!.Length);
            var batch = await store.RenewOfflineRecoveryCodesAsync(userId, InitialPassword);

            Assert.Equal(10, batch.Codes.Count);
            Assert.Equal(10, batch.Codes.Distinct(StringComparer.Ordinal).Count());
            Assert.True(batch.Codes.All(code => code.Length == 22 && code.All(IsBase64UrlCharacter)));
            Assert.True(batch.ExpiresUtc >= DateTimeOffset.UtcNow.AddMonths(12).AddMinutes(-1));
            Assert.True(batch.ExpiresUtc <= DateTimeOffset.UtcNow.AddMonths(12).AddMinutes(1));

            await using var dataSource = NpgsqlDataSource.Create(connectionString);
            await using (var connection = await dataSource.OpenConnectionAsync())
            await using (var command = new NpgsqlCommand(
                "SELECT code_hash FROM garagemotor.recovery_codes WHERE user_id = @user_id AND kind = 'offline'",
                connection))
            {
                command.Parameters.AddWithValue("user_id", userId);
                await using var reader = await command.ExecuteReaderAsync();
                var storedHashes = new HashSet<string>(StringComparer.Ordinal);
                while (await reader.ReadAsync())
                {
                    storedHashes.Add(Convert.ToHexString(reader.GetFieldValue<byte[]>(0)));
                }

                Assert.Equal(10, storedHashes.Count);
                Assert.True(batch.Codes.All(code => storedHashes.Contains(
                    Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(code))))));
                Assert.DoesNotContain(batch.Codes, code => storedHashes.Contains(code));
            }

            const string replacementPassword = "GarageMotor-Recovered-Password-2026!";
            Assert.True(await store.ConfirmRecoveryAsync(email, batch.Codes[0], replacementPassword));
            var updatedAccount = await store.FindAccountByEmailAsync(email);
            Assert.NotNull(updatedAccount);
            Assert.True(store.VerifyPassword(updatedAccount, replacementPassword));
            Assert.False(store.VerifyPassword(updatedAccount, InitialPassword));
            Assert.False(await store.RevokeSessionAsync(userId, session.Token));
        }
        finally
        {
            await DeleteTestUserAsync(connectionString, userId);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task RecoveryAttemptStoreEnforcesAccountAndOriginLimitsUnderConcurrency()
    {
        var connectionString = GetConnectionString();
        await using var database = new Database(connectionString);
        var store = new RecoveryAttemptStore(database);
        var email = $"rate-limit-{Guid.NewGuid():N}@example.test";
        var serialOrigin = $"test-origin-serial-{Guid.NewGuid():N}";
        var concurrentEmail = $"parallel-{Guid.NewGuid():N}@example.test";
        var concurrentOrigin = $"test-origin-concurrent-{Guid.NewGuid():N}";
        var dedicatedOrigin = $"test-origin-dedicated-{Guid.NewGuid():N}";
        var originResults = new List<RecoveryAttemptDecision>();
        var accountKeys = new List<string> { email, concurrentEmail };
        try
        {
            var serialResults = new List<RecoveryAttemptDecision>();
            for (var attempt = 0; attempt < 6; attempt++)
            {
                serialResults.Add(await store.RecordFailureAsync(email, serialOrigin));
            }

            Assert.Equal(5, serialResults.Count(result => result.Allowed));
            Assert.False(serialResults[^1].Allowed);

            var concurrent = await Task.WhenAll(Enumerable.Range(0, 8)
                .Select(_ => store.RecordFailureAsync(concurrentEmail, concurrentOrigin)));
            Assert.Equal(5, concurrent.Count(result => result.Allowed));
            Assert.True(concurrent.All(result => result.AccountFailures <= 5 && result.OriginFailures <= 100));

            for (var attempt = 0; attempt < 100; attempt++)
            {
                var uniqueEmail = $"origin-{attempt}-{Guid.NewGuid():N}@example.test";
                accountKeys.Add(uniqueEmail);
                originResults.Add(await store.RecordFailureAsync(uniqueEmail, dedicatedOrigin));
            }
            var blockedOrigin = await store.RecordFailureAsync($"origin-blocked-{Guid.NewGuid():N}@example.test", dedicatedOrigin);
            Assert.Equal(100, originResults.Count(result => result.Allowed));
            Assert.False(blockedOrigin.Allowed);
            Assert.Equal(100, blockedOrigin.OriginFailures);
        }
        finally
        {
            await DeleteTestRecoveryAttemptsAsync(
                connectionString,
                accountKeys,
                [serialOrigin, concurrentOrigin, dedicatedOrigin]);
        }
    }

    private static async Task<WebAccount> CreateWebAccountAsync()
    {
        var email = $"security-{Guid.NewGuid():N}@example.test";
        var handler = new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false, UseProxy = false };
        var client = new HttpClient(handler) { BaseAddress = RealEnvironment.ProxyBaseUri };

        using var registrationResponse = await client.PostAsJsonAsync(
            "/api/v1/accounts",
            new { email, name = "Security integration test", password = InitialPassword });
        Assert.Equal(HttpStatusCode.Accepted, registrationResponse.StatusCode);

        using var sessionResponse = await client.PostAsJsonAsync(
            "/api/v1/sessions",
            new { email, password = InitialPassword, channel = "web" });
        Assert.Equal(HttpStatusCode.OK, sessionResponse.StatusCode);
        Assert.True(sessionResponse.Headers.TryGetValues("Set-Cookie", out var cookieHeaders));
        var sessionCookie = string.Join(";", cookieHeaders!);
        var normalizedCookie = sessionCookie.ToLowerInvariant();
        Assert.Contains("__host-gm-session=", normalizedCookie);
        Assert.Contains("secure", normalizedCookie);
        Assert.Contains("httponly", normalizedCookie);
        Assert.Contains("path=/", normalizedCookie);
        CaptureCookiesForLocalProxy(client, sessionResponse);

        using var session = JsonDocument.Parse(await sessionResponse.Content.ReadAsStringAsync());
        Assert.False(session.RootElement.TryGetProperty("token", out _));
        var userId = session.RootElement.GetProperty("userId").GetGuid();
        return new WebAccount(client, email, userId);
    }

    private static HttpClient CreateHttpClient()
    {
        return new HttpClient(new HttpClientHandler { UseProxy = false })
        {
            BaseAddress = RealEnvironment.ProxyBaseUri,
        };
    }

    private static string GetConnectionString()
    {
        return Environment.GetEnvironmentVariable("ConnectionStrings__GarageMotor")
            ?? throw new InvalidOperationException("ConnectionStrings__GarageMotor is required.");
    }

    private static async Task DeleteTestUserAsync(string connectionString, Guid userId)
    {
        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(
            "DELETE FROM garagemotor.users WHERE user_id = @user_id",
            connection);
        command.Parameters.AddWithValue("user_id", userId);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task DeleteTestRecoveryAttemptsAsync(
        string connectionString,
        IEnumerable<string> accountKeys,
        IEnumerable<string> origins)
    {
        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        await using var connection = await dataSource.OpenConnectionAsync();
        foreach (var accountKey in accountKeys)
        {
            await DeleteRecoveryAttemptsByHashAsync(connection, HashRateLimitKey("account", IdentityStore.NormalizeEmail(accountKey)));
        }
        foreach (var origin in origins)
        {
            await DeleteRecoveryAttemptsByHashAsync(connection, HashRateLimitKey("origin", origin));
        }
    }

    private static async Task DeleteRecoveryAttemptsByHashAsync(NpgsqlConnection connection, byte[] keyHash)
    {
        await using var command = new NpgsqlCommand(
            "DELETE FROM garagemotor.recovery_attempts WHERE account_key_hash = @key_hash OR origin_key_hash = @key_hash",
            connection);
        command.Parameters.AddWithValue("key_hash", keyHash);
        await command.ExecuteNonQueryAsync();
    }

    private static byte[] HashRateLimitKey(string kind, string value)
    {
        var input = System.Text.Encoding.UTF8.GetBytes($"GarageMotor.RecoveryAttempt.v1:{kind}:{value}");
        return System.Security.Cryptography.SHA256.HashData(input);
    }

    private static async Task<string> GetCsrfTokenAsync(WebAccount account)
    {
        using var response = await account.Client.GetAsync("/api/v1/csrf");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        CaptureCookiesForLocalProxy(account.Client, response);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("requestToken").GetString()!;
    }

    private static void CaptureCookiesForLocalProxy(HttpClient client, HttpResponseMessage response)
    {
        var cookies = new Dictionary<string, string>(StringComparer.Ordinal);
        if (client.DefaultRequestHeaders.TryGetValues("Cookie", out var existingHeaders))
        {
            foreach (var cookie in existingHeaders.SelectMany(header => header.Split(';', StringSplitOptions.RemoveEmptyEntries)))
            {
                var separator = cookie.IndexOf('=');
                if (separator > 0)
                {
                    cookies[cookie[..separator].Trim()] = cookie.Trim();
                }
            }
        }

        if (response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
        {
            foreach (var setCookie in setCookieHeaders)
            {
                var cookie = setCookie.Split(';', 2)[0].Trim();
                var separator = cookie.IndexOf('=');
                if (separator > 0)
                {
                    cookies[cookie[..separator]] = cookie;
                }
            }
        }

        client.DefaultRequestHeaders.Remove("Cookie");
        if (cookies.Count > 0)
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", string.Join("; ", cookies.Values));
        }
    }

    private static HttpRequestMessage CreateJsonRequest(HttpMethod method, string path, object payload)
    {
        return new HttpRequestMessage(method, path)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
        };
    }

    private static bool IsBase64UrlCharacter(char character)
    {
        return character is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '-' or '_';
    }

    private sealed class WebAccount : IDisposable
    {
        public HttpClient Client { get; }
        public string Email { get; }
        public Guid UserId { get; }

        public WebAccount(HttpClient client, string email, Guid userId)
        {
            Client = client;
            Email = email;
            UserId = userId;
        }

        public void Dispose()
        {
            Client.Dispose();
        }
    }
}
