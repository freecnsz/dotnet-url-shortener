namespace UrlShortener.Application.Common.Results;

public enum ErrorType
{
    Validation,   // Input violates a rule; the caller can fix it
    Unauthorized, // Missing or invalid credentials
    NotFound,     // Requested resource does not exist
    Gone,         // Resource existed but is no longer available
    Conflict,     // Request conflicts with current state
    Failure       // Unexpected failure; not the caller's fault
}
