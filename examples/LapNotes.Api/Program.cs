using LapNotes.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var authority = builder.Configuration["Auth:Authority"];
var audience = builder.Configuration["Auth:Audience"];
var connectionString = builder.Configuration.GetConnectionString("DemoDb");

if (!Uri.TryCreate(authority, UriKind.Absolute, out var issuer)
    || issuer.Scheme != Uri.UriSchemeHttps
    || string.IsNullOrWhiteSpace(audience)
    || string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Configure Auth:Authority (HTTPS issuer), Auth:Audience and ConnectionStrings:DemoDb.");
}

builder.Services.AddSingleton(NpgsqlDataSource.Create(connectionString));
builder.Services.AddScoped<ILapNoteStore, PostgresLapNoteStore>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.Audience = audience;
        options.RequireHttpsMetadata = true;
    });
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

// Liveness checks do not touch the database and disclose no configuration.
app.MapGet("/health/live", () => Results.Ok(new { status = "ok" }))
    .AllowAnonymous();

var api = app.MapGroup("/api/demo/lap-notes").RequireAuthorization();
api.MapPost("/", async Task<IResult> (
    CreateLapNoteRequest request, HttpContext context, ILapNoteStore store,
    CancellationToken cancellationToken) =>
{
    var organizationId = context.User.FindFirst("org_id")?.Value;
    if (!OrganizationScope.IsValid(organizationId))
        return Results.Forbid();

    var validationError = LapNoteValidator.Validate(request);
    if (validationError is not null)
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["lapNote"] = [validationError]
        });

    var note = new LapNote(
        Guid.NewGuid(),
        organizationId!,
        request.KartLabel!.Trim(),
        request.LapDurationMs,
        DateTimeOffset.UtcNow);

    await store.InsertAsync(note, cancellationToken);
    return Results.Created($"/api/demo/lap-notes/{note.Id}", LapNoteResponse.From(note));
});

api.MapGet("/{id:guid}", async Task<IResult> (
    Guid id, HttpContext context, ILapNoteStore store,
    CancellationToken cancellationToken) =>
{
    var organizationId = context.User.FindFirst("org_id")?.Value;
    if (!OrganizationScope.IsValid(organizationId))
        return Results.Forbid();

    // The tenant scope is supplied by the validated token and enforced in the SQL predicate.
    var note = await store.FindAsync(id, organizationId!, cancellationToken);
    return note is null ? Results.NotFound() : Results.Ok(LapNoteResponse.From(note));
});

app.Run();
