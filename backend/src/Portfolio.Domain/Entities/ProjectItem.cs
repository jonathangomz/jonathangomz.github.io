namespace Portfolio.Domain.Entities;

public sealed record ProjectItem(
    string Title,
    string Details,
    string? Href,
    string? HrefLabel,
    string Repo);
