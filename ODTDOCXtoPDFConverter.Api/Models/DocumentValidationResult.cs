namespace ODTDOCXtoPDFConverter.Api.Models;

public record DocumentValidationResult(
    bool IsValid,
    string? Reason
);
