using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODTDOCXtoPDFConverter.Api.Models;
using ODTDOCXtoPDFConverter.Api.Services;


namespace ODTDOCXtoPDFConverter.Api.Controllers
{
    [ApiController]
    [Route("api/document/extract")]
    public class DocumentVariableExtractionController : ControllerBase
    {
        private readonly IDocumentVariableExtractorService _documentVariableExtractorService;

        public DocumentVariableExtractionController(IDocumentVariableExtractorService documentVariableExtractorService)
        {
            _documentVariableExtractorService = documentVariableExtractorService;
        }

        [HttpPost]
        public async Task<ActionResult<List<string>>> GetExtractedVariables(IFormFile document, CancellationToken cancellationToken)
        {
            if (document.Length > UploadLimits.MaxDocumentBytes)
                return StatusCode(StatusCodes.Status413PayloadTooLarge, "Document file limit is 10 MiB; variables file limit is 1 Mib.");

            List<string> extractedVariables = await _documentVariableExtractorService.ExtractVariables(document);

            return extractedVariables;
        }
    }
}
