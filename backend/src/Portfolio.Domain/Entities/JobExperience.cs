namespace Portfolio.Domain.Entities;

public sealed record JobExperience(
    string Company,
    string Role,
    string Period,
    string Location,
    IReadOnlyList<string> Highlights);
