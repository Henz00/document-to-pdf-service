using ODTDOCXtoPDFConverter.Api.Models;

namespace ODTDOCXtoPDFConverter.Api.Services
{
    public interface IDocumentValidatorService
    {
        DocumentValidationResult ValidateDocument(IFormFile? document);
    }
}
