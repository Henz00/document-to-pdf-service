using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ODTDOCXtoPDFConverter.Api.Services
{
    public class DocumentService
    {
        private readonly ILogger<DocumentService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IPdfConverterService _pdfConverter;
        private readonly OdtDocumentProcessorService _odtProcessor;
        private readonly DocxDocumentProcessorService _docxProcessor;

        public DocumentService(ILogger<DocumentService> logger, IConfiguration configuration, IPdfConverterService pdfConverter, OdtDocumentProcessorService odtProcessor, DocxDocumentProcessorService docxProcessor)
        {
            _logger = logger;
            _configuration = configuration;
            _pdfConverter = pdfConverter;
            _odtProcessor = odtProcessor;
            _docxProcessor = docxProcessor;
        }


        public async Task<byte[]> ConvertAsync(IFormFile document, IFormFile variables, CancellationToken cancellationToken)
        {
            // preparing working temp directory
            string tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDirectory);
            
            // preparing json variables from user input
            using Stream variablesStream = variables.OpenReadStream();
            Dictionary<string, string>? jsonVariables = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(variablesStream, cancellationToken: cancellationToken);

            // document preparation
            string extension = Path.GetExtension(document.FileName).ToLowerInvariant();
            string outputFile = "";

            try
            {

                if (extension == ".odt")
                {
                    outputFile = await _odtProcessor.ProcessAsync(document, jsonVariables, tempDirectory, cancellationToken);
                }
                else if (extension == ".docx")
                {
                    outputFile = await _docxProcessor.ProcessAsync(document, jsonVariables, tempDirectory, cancellationToken);
                }

                string pdfFile = await _pdfConverter.ConvertAsync(outputFile, tempDirectory);

                byte[] pdfBytes = await File.ReadAllBytesAsync(pdfFile, cancellationToken);

                return pdfBytes;
            }
            finally
            {
                if(Directory.Exists(tempDirectory))
                    Directory.Delete(tempDirectory, true);
            }
        }
    }
}
