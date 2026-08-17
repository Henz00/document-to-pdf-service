using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODTDOCXtoPDFConverter.Api.Services;

namespace ODTDOCXtoPDFConverter.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/document/")]
    public class DocumentController : ControllerBase
    {
        private readonly DocumentService _documentService;

        public DocumentController(DocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpPost]
        public async Task<ActionResult> ConvertDocument(IFormFile document, IFormFile variables, CancellationToken cancellationToken)
        {
            byte[] pdf = await _documentService.ConvertAsync(
            document,
            variables,
            cancellationToken);
            return File(pdf,"application/pdf", "converted_file.pdf");
        }
    }
}
