using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODTDOCXtoPDFConverter.Api.Services;

namespace ODTDOCXtoPDFConverter.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/document/extract")]
    public class DocumentVariableExtractionController : ControllerBase
    {
        private readonly DocumentVariableExtractorService _documentVariableExtractorService;

        public DocumentVariableExtractionController(DocumentVariableExtractorService documentVariableExtractorService)
        {
            _documentVariableExtractorService = documentVariableExtractorService;
        }

        [HttpPost]
        public async Task<List<string>> GetExtractedVariables(IFormFile document, CancellationToken cancellationToken)
        {

            List<string> extractedVariables = await _documentVariableExtractorService.ExtractVariables(document);

            return extractedVariables;
        }
    }
}
