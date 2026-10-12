using LapNotes.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Validate configuration on application start (not during composition), allowing
// WebApplicationFactory to provide isolated configuration and a fake store in tests.
builder.Services.AddOptions<DemoRuntimeOptions>()
    .Configure<IConfiguration>((options, config) =>
    {
        options.Authority = config["Auth:Authority"];
        options.Audience = config["Auth:Audience"];
        options.ConnectionString = config.GetConnectionString("DemoDb");
    })
    .Validate(options => options.IsConfigured(),
        "Provide an HTTPS Auth:Authority, Auth:Audience and ConnectionStrings:DemoDb.")
    .ValidateOnStart();

builder.Services.AddSingleton(provider =>
    NpgsqlDataSource.Create(provider.GetRequiredService<IOptions<DemoRuntimeOptions>>()
        .Value.ConnectionString!));
builder.Services.AddScoped<ILapNoteStore, PostgresLapNoteStore>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.Audience = builder.Configuration["Auth:Audience"];
        options.RequireHttpsMetadata = true;
        options.MapInboundClaims = false;
        options.TokenValidationParameters.ValidAlgorithms = [SecurityAlgorithms.RsaSha256];
        options.TokenValidationParameters.ClockSkew = TimeSpan.FromSeconds(30);
    });

// The tenant check applies to the entire route group, not individual handlers.
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("TenantAccess", policy => policy
        .RequireAuthenticatedUser()
        .AddRequirements(new OrganizationRequirement()));
builder.Services.AddSingleton<IAuthorizationHandler, OrganizationAuthorizationHandler>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentOrganization, CurrentOrganization>();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health/live", () => Results.Ok(new { status = "ok" }))
    .AllowAnonymous();

var api = app.MapGroup("/api/demo/lap-notes").RequireAuthorization("TenantAccess");
api.MapPost("/", async Task<IResult> (
    CreateLapNoteRequest request, ICurrentOrganization organization, ILapNoteStore store,
    CancellationToken cancellationToken) =>
{
    var validationError = LapNoteValidator.Validate(request);
    if (validationError is not null)
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["lapNote"] = [validationError]
        });

    var note = new LapNote(
        Guid.NewGuid(),
        organization.Id,
        request.KartLabel!.Trim(),
        request.LapDurationMs,
        DateTimeOffset.UtcNow);

    await store.InsertAsync(note, cancellationToken);
    return Results.Created($"/api/demo/lap-notes/{note.Id}", LapNoteResponse.From(note));
});

api.MapGet("/{id:guid}", async Task<IResult> (
    Guid id, ICurrentOrganization organization, ILapNoteStore store,
    CancellationToken cancellationToken) =>
{
    var note = await store.FindAsync(id, organization.Id, cancellationToken);
    return note is null ? Results.NotFound() : Results.Ok(LapNoteResponse.From(note));
});

app.Run();

// Required by WebApplicationFactory<Program> in the separate HTTP test assembly.
public partial class Program;
