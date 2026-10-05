using ODTDOCXtoPDFConverter.Api.Models;
using System.IO.Compression;

namespace ODTDOCXtoPDFConverter.Api.Services
{
    public class DocumentValidatorService : IDocumentValidatorService
    {
        public DocumentValidationResult ValidateDocument(IFormFile? document)
        {
            if (document == null)
                return new DocumentValidationResult(false, "The document is missing.");

            if (document.Length == 0)
                return new DocumentValidationResult(false, "The document is empty.");

            string extension = Path.GetExtension(document.FileName).ToLowerInvariant();

            if (extension != ".odt" && extension != ".docx")
                return new DocumentValidationResult(false, "Only DOCX and ODT files are supported.");


            try
            {
                using Stream documentStream = document.OpenReadStream();
                using ZipArchive archive = new(documentStream, ZipArchiveMode.Read);

                if (extension == ".odt")
                {
                    if (archive.GetEntry("content.xml") is null || archive.GetEntry("mimetype") is null)
                        return new DocumentValidationResult(false, "The file is not a supported ODT document.");
                }                    
                else if (extension == ".docx")
                {
                    if (archive.GetEntry("word/document.xml") is null || archive.GetEntry("[Content_Types].xml") is null)
                        return new DocumentValidationResult(false, "The file is not a supported DOCX document.");
                }
            }
            catch (InvalidDataException)
            {
                return new(false, "The document is not a readable ZIP-based document.");
            }
            
            return new DocumentValidationResult(true, null);
        }
    }
}
