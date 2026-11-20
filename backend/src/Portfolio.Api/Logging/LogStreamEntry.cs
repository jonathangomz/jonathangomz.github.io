namespace Portfolio.Api.Logging;

public sealed record LogStreamEntry(
    DateTimeOffset Timestamp,
    string Level,
    string Source,
    string Message);
