namespace Portfolio.Domain.Entities;

public sealed record AboutInfo(IReadOnlyList<string> Paragraphs, string Philosophy);
