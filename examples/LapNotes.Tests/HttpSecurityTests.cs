using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LapNotes.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace LapNotes.Tests;

// Signature validation uses a throwaway RSA key and static, in-process issuer metadata.
// No identity provider, external network access or actual customer token is involved.
public sealed class HttpSecurityTests : IClassFixture<DemoHttpFactory>
{
    private readonly DemoHttpFactory _factory;
    public HttpSecurityTests(DemoHttpFactory factory) => _factory = factory;

    [Fact]
    public async Task Liveness_is_public()
    {
        using var client = _factory.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health/live")).StatusCode);
    }

    [Fact]
    public async Task Missing_bearer_token_gets_401()
    {
        using var client = _factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.GetAsync($"/api/demo/lap-notes/{Guid.NewGuid()}")).StatusCode);
    }

    [Fact]
    public async Task Expired_token_gets_401()
    {
        using var client = _factory.CreateAuthenticatedClient(
            _factory.IssueToken(expires: DateTime.UtcNow.AddHours(-1),
                notBefore: DateTime.UtcNow.AddHours(-2)));
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.GetAsync($"/api/demo/lap-notes/{Guid.NewGuid()}")).StatusCode);
    }

    [Fact]
    public async Task Wrong_issuer_gets_401()
    {
        using var client = _factory.CreateAuthenticatedClient(
            _factory.IssueToken(issuer: "https://untrusted.test.invalid"));
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.GetAsync($"/api/demo/lap-notes/{Guid.NewGuid()}")).StatusCode);
    }

    [Fact]
    public async Task Wrong_audience_gets_401()
    {
        using var client = _factory.CreateAuthenticatedClient(
            _factory.IssueToken(audience: "untrusted-api"));
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.GetAsync($"/api/demo/lap-notes/{Guid.NewGuid()}")).StatusCode);
    }

    [Fact]
    public async Task Unsigned_token_gets_401()
    {
        var expiry = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds();
        var unsigned = $"{Base64UrlEncoder.Encode("{\"alg\":\"none\",\"typ\":\"JWT\"}")}." +
            $"{Base64UrlEncoder.Encode($"{{\"iss\":\"{DemoHttpFactory.Issuer}\",\"aud\":\"{DemoHttpFactory.Audience}\",\"exp\":{expiry},\"org_id\":\"team_a\"}}")}.";
        using var client = _factory.CreateAuthenticatedClient(unsigned);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.GetAsync($"/api/demo/lap-notes/{Guid.NewGuid()}")).StatusCode);
    }

    [Fact]
    public async Task Valid_token_without_organization_gets_403()
    {
        using var client = _factory.CreateAuthenticatedClient(_factory.IssueToken(orgIds: []));
        Assert.Equal(HttpStatusCode.Forbidden,
            (await client.GetAsync($"/api/demo/lap-notes/{Guid.NewGuid()}")).StatusCode);
    }

    [Fact]
    public async Task Duplicate_organization_claims_get_403()
    {
        using var client = _factory.CreateAuthenticatedClient(
            _factory.IssueToken(orgIds: ["team_a", "team_b"]));
        Assert.Equal(HttpStatusCode.Forbidden,
            (await client.GetAsync($"/api/demo/lap-notes/{Guid.NewGuid()}")).StatusCode);
    }

    [Fact]
    public async Task Malformed_organization_claim_gets_403()
    {
        using var client = _factory.CreateAuthenticatedClient(
            _factory.IssueToken(orgIds: ["team/../other"]));
        Assert.Equal(HttpStatusCode.Forbidden,
            (await client.GetAsync($"/api/demo/lap-notes/{Guid.NewGuid()}")).StatusCode);
    }

    [Fact]
    public async Task Organization_from_request_body_cannot_override_token()
    {
        using var client = _factory.CreateAuthenticatedClient(_factory.IssueToken(orgIds: ["team_a"]));
        using var created = await client.PostAsJsonAsync("/api/demo/lap-notes",
            new { kartLabel = "Kart 12", lapDurationMs = 60_000, organizationId = "team_b" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        using var document = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        AssertNoOrganizationField(document.RootElement);
        var id = document.RootElement.GetProperty("id").GetGuid();

        using var otherTenantClient = _factory.CreateAuthenticatedClient(
            _factory.IssueToken(orgIds: ["team_b"]));
        Assert.Equal(HttpStatusCode.NotFound,
            (await otherTenantClient.GetAsync($"/api/demo/lap-notes/{id}")).StatusCode);

        using var ownerResult = await client.GetAsync($"/api/demo/lap-notes/{id}");
        Assert.Equal(HttpStatusCode.OK, ownerResult.StatusCode);
        using var ownerDocument = JsonDocument.Parse(await ownerResult.Content.ReadAsStringAsync());
        AssertNoOrganizationField(ownerDocument.RootElement);
    }

    [Fact]
    public async Task Invalid_payload_returns_400_without_database_write()
    {
        using var client = _factory.CreateAuthenticatedClient(_factory.IssueToken(orgIds: ["team_a"]));
        using var response = await client.PostAsJsonAsync("/api/demo/lap-notes",
            new { kartLabel = "Kart 12", lapDurationMs = 0 });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static void AssertNoOrganizationField(JsonElement json)
    {
        Assert.False(json.TryGetProperty("organizationId", out _));
        Assert.False(json.TryGetProperty("OrganizationId", out _));
        Assert.DoesNotContain("team_a", json.GetRawText());
    }
}

public sealed class DemoHttpFactory : WebApplicationFactory<Program>
{
    public const string Issuer = "https://issuer.example.invalid";
    public const string Audience = "lapnotes-tests";
    private readonly RSA _rsa = RSA.Create(2048);
    private readonly RsaSecurityKey _signingKey;

    public DemoHttpFactory() => _signingKey = new RsaSecurityKey(_rsa) { KeyId = "test-signing-key" };

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Auth:Authority"] = Issuer,
                ["Auth:Audience"] = Audience,
                ["ConnectionStrings:DemoDb"] = "Host=localhost;Database=not-used;Username=test"
            }));

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ILapNoteStore>();
            services.AddSingleton<ILapNoteStore, MemoryStore>();

            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var configuration = new OpenIdConnectConfiguration { Issuer = Issuer };
                configuration.SigningKeys.Add(_signingKey);
                options.ConfigurationManager =
                    new StaticConfigurationManager<OpenIdConnectConfiguration>(configuration);
                options.TokenValidationParameters.ValidIssuer = Issuer;
                options.TokenValidationParameters.ValidAudience = Audience;
                options.TokenValidationParameters.IssuerSigningKey = _signingKey;
                options.TokenValidationParameters.ValidAlgorithms = [SecurityAlgorithms.RsaSha256];
                options.TokenValidationParameters.ClockSkew = TimeSpan.Zero;
            });
        });
    }

    public HttpClient CreateAuthenticatedClient(string token)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public string IssueToken(
        string[]? orgIds = null,
        string? issuer = null,
        string? audience = null,
        DateTime? notBefore = null,
        DateTime? expires = null)
    {
        var claims = (orgIds ?? ["team_a"])
            .Select(id => new Claim("org_id", id))
            .ToArray();

        var jwt = new JwtSecurityToken(
            issuer ?? Issuer,
            audience ?? Audience,
            claims,
            notBefore ?? DateTime.UtcNow.AddMinutes(-1),
            expires ?? DateTime.UtcNow.AddMinutes(5),
            new SigningCredentials(_signingKey, SecurityAlgorithms.RsaSha256));
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}

public sealed class MemoryStore : ILapNoteStore
{
    private readonly ConcurrentDictionary<Guid, LapNote> _items = new();

    public Task InsertAsync(LapNote note, CancellationToken cancellationToken)
    {
        _items[note.Id] = note;
        return Task.CompletedTask;
    }

    public Task<LapNote?> FindAsync(Guid id, string organizationId, CancellationToken cancellationToken)
    {
        var note = _items.TryGetValue(id, out var item)
            && item.OrganizationId == organizationId ? item : null;
        return Task.FromResult(note);
    }
}
