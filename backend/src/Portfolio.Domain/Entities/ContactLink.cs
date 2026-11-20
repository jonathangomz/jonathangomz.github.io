namespace Portfolio.Domain.Entities;

public sealed record ContactLink(
    string Key,
    string Display,
    string Href,
    string Icon);
