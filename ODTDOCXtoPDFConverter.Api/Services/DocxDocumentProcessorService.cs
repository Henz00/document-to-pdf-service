using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;

namespace ODTDOCXtoPDFConverter.Api.Services
{
    public class DocxDocumentProcessorService: IDocumentProcessor
    {
        public bool CanProcess(string extension)
        {
            return extension.Equals(
               ".docx",
               StringComparison.OrdinalIgnoreCase);
        }

        public async Task<string> ProcessAsync(IFormFile document, Dictionary<string, string> variables, string directory, CancellationToken cancellationToken)
        {
            Dictionary<string, XDocument> modifiedDocuments = [];
            using Stream documentStream = document.OpenReadStream();
            using ZipArchive archive = new(documentStream, ZipArchiveMode.Read);

            ZipArchiveEntry? entryBody = archive.GetEntry("word/document.xml");
            ZipArchiveEntry? entryHeader = archive.GetEntry("word/header1.xml");
            ZipArchiveEntry? entryFooter = archive.GetEntry("word/footer1.xml");

            if (entryBody is null)
                throw new InvalidOperationException("word/document.xml not found.");

            List<ZipArchiveEntry> entries = [entryBody];

            if (entryHeader is not null)
                entries.Add(entryHeader);

            if (entryFooter is not null)
                entries.Add(entryFooter);

            foreach (ZipArchiveEntry entry in entries)
            {
                using var stream = entry!.Open();

                var openedDocument = XDocument.Load(stream);
                XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

                foreach (var textElement in openedDocument.Descendants(w + "t"))
                {
                    foreach (var variable in variables)
                    {
                        textElement.Value = textElement.Value.Replace(
                            $"{{{variable.Key}}}",
                            variable.Value);
                    }
                }

                modifiedDocuments[entry.FullName] = openedDocument;
            }

            

            string inputFile = Path.Combine(directory, "input.docx");
            await using (FileStream fileStream = File.Create(inputFile))
            {
                await document.CopyToAsync(fileStream, cancellationToken);
            }


            using ZipArchive inputArchive = ZipFile.OpenRead(inputFile);
            string outputFile = Path.Combine(directory, "output.docx");
            using (FileStream outputStream = File.Create(outputFile))
            using (ZipArchive outputArchive = new(outputStream, ZipArchiveMode.Create))
            {
                foreach (ZipArchiveEntry entry in inputArchive.Entries)
                {
                    ZipArchiveEntry newEntry =
                        outputArchive.CreateEntry(entry.FullName);

                    using Stream source = entry.Open();
                    using Stream destination = newEntry.Open();

                    if (modifiedDocuments.TryGetValue(
                        entry.FullName,
                        out XDocument? modifiedDocument))
                    {
                        modifiedDocument.Save(destination);
                    }
                    else
                    {
                        source.CopyTo(destination);
                    }
                }
            }

            return outputFile;
        }
    }
}
