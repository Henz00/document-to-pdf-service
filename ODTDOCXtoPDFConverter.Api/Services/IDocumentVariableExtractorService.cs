namespace ODTDOCXtoPDFConverter.Api.Services;

public interface IDocumentVariableExtractorService
{
    Task<List<string>> ExtractVariables(IFormFile document);
}
