using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODTDOCXtoPDFConverter.Api.Models;
using ODTDOCXtoPDFConverter.Api.Services;

namespace ODTDOCXtoPDFConverter.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/document/")]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpPost]
        public async Task<ActionResult> ConvertDocument(IFormFile document, IFormFile variables, CancellationToken cancellationToken)
        {
            if (document.Length > UploadLimits.MaxDocumentBytes || variables.Length > UploadLimits.MaxVariableDocumentBytes)
                return StatusCode(StatusCodes.Status413PayloadTooLarge, "Document file limit is 10 MiB; variables file limit is 1 Mib.");

            byte[] pdf = await _documentService.ConvertAsync(
                document,
                variables,
                cancellationToken
            );

            return File(pdf,"application/pdf", "converted_file.pdf");
        }
    }
}
