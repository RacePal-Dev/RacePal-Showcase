using System.Text.Json;
using LapNotes.Api;
using Xunit;

namespace LapNotes.Tests;

public sealed class ValidationTests
{
    [Theory]
    [InlineData("Kart 12", 60_000)]
    [InlineData(" R42 ", 15_000)]
    [InlineData("Kart-1", 600_000)]
    [InlineData("Kart-40", 15_000)]
    public void Accepts_valid_fictional_lap_notes(string name, int duration) =>
        Assert.Null(LapNoteValidator.Validate(new CreateLapNoteRequest(name, duration)));

    [Fact]
    public void Accepts_exactly_forty_characters() =>
        Assert.Null(LapNoteValidator.Validate(new CreateLapNoteRequest(new string('x', 40), 60_000)));

    [Theory]
    [InlineData(null, 60_000)]
    [InlineData("  ", 60_000)]
    [InlineData("Kart 12", 0)]
    [InlineData("Kart 12", 14_999)]
    [InlineData("Kart 12", 600_001)]
    public void Rejects_invalid_lap_notes(string? name, int duration) =>
        Assert.NotNull(LapNoteValidator.Validate(new CreateLapNoteRequest(name, duration)));

    [Fact]
    public void Rejects_oversized_kart_label() =>
        Assert.NotNull(LapNoteValidator.Validate(
            new CreateLapNoteRequest(new string('x', 41), 60_000)));

    [Theory]
    [InlineData("team_a")]
    [InlineData("org-2026")]
    public void Accepts_valid_organization_claim(string organization) =>
        Assert.True(OrganizationScope.IsValid(organization));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData("team/../other")]
    [InlineData("tenant name")]
    public void Rejects_invalid_organization_claim(string? organization) =>
        Assert.False(OrganizationScope.IsValid(organization));

    [Fact]
    public void Response_omits_organization_claim()
    {
        var response = LapNoteResponse.From(new LapNote(
            Guid.NewGuid(), "private-tenant", "Kart 12", 60_000, DateTimeOffset.UtcNow));

        var serialized = JsonSerializer.Serialize(response);
        Assert.DoesNotContain("private-tenant", serialized);
        Assert.DoesNotContain("OrganizationId", serialized);
        Assert.DoesNotContain("organizationId", serialized);
    }
}
