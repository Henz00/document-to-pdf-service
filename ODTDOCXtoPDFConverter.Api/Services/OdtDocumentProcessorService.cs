using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ODTDOCXtoPDFConverter.Api.Services
{
    public class OdtDocumentProcessorService: IDocumentProcessor
    {
        public bool CanProcess(string extension)
        {
            return extension.Equals(
               ".odt",
               StringComparison.OrdinalIgnoreCase);
        }

        public async Task<string> ProcessAsync(IFormFile document, Dictionary<string, string> variables, string directory, CancellationToken cancellationToken)
        {
            using Stream documentStream = document.OpenReadStream();
            using ZipArchive archive = new(documentStream, ZipArchiveMode.Read);

            ZipArchiveEntry? contentEntry = archive.GetEntry("content.xml");
            ZipArchiveEntry? stylesEntry = archive.GetEntry("styles.xml");

            if (contentEntry is null || stylesEntry is null)
                throw new Exception("xml file not found!");

            //preparing content.xml for extraction
            using Stream stream = contentEntry.Open();
            XDocument contentDocument = XDocument.Load(stream);

            //preparing styles.xml for extraction
            using Stream stylesStream = stylesEntry.Open();
            XDocument stylesDocument = XDocument.Load(stylesStream);
            
            //variable extraction from content.xml
            XNamespace office = "urn:oasis:names:tc:opendocument:xmlns:office:1.0";
            XElement? body = contentDocument
                .Element(office + "document-content")?
                .Element(office + "body")?
                .Element(office + "text");
            XNamespace text = "urn:oasis:names:tc:opendocument:xmlns:text:1.0";
            IEnumerable<XElement> bodyTextElements = body!.Descendants(text + "p");

            //variable extraction from styles.xml
            XNamespace style = "urn:oasis:names:tc:opendocument:xmlns:style:1.0";
            IEnumerable<XElement> headersTextElements = stylesDocument.Descendants(style + "header");
            IEnumerable<XElement> footersTextElements = stylesDocument.Descendants(style + "footer");

            //combining elements for single-operation regex search
            IEnumerable<XElement> combinedElements = bodyTextElements.Concat(footersTextElements).Concat(headersTextElements);
            string combinedText = string.Join('\n', combinedElements.Select(e => e.Value));


            List<string> regex_variables = Regex
                .Matches(combinedText, @"\{([^{}\r\n]+)\}")
                .Select(match => match.Groups[1].Value)
                .Distinct()
                .ToList();

            foreach (XElement element in combinedElements)
            {
                foreach (XText textNode in element.DescendantNodes().OfType<XText>())
                {
                    string textToTranslate = textNode.Value;

                    foreach (string variableName in regex_variables)
                    {
                        if (!variables.TryGetValue(variableName, out string? value))
                            continue;

                        textToTranslate = textToTranslate.Replace(
                            $"{{{variableName}}}",
                            value);

                        textToTranslate = textToTranslate.Replace(
                            variableName,
                            value);

                        textToTranslate = textToTranslate.Replace(
                            "{",
                            "");

                        textToTranslate = textToTranslate.Replace(
                            "}",
                            "");
                    }

                    textNode.Value = textToTranslate;
                }
            }



            string inputFile = Path.Combine(directory, "input.odt");
            await using (FileStream fileStream = File.Create(inputFile))
            {
                await document.CopyToAsync(fileStream, cancellationToken);
            }


            using ZipArchive inputArchive = ZipFile.OpenRead(inputFile);
            string outputFile = Path.Combine(directory, "output.odt");
            using (FileStream outputStream = File.Create(outputFile))
            using (ZipArchive outputArchive = new(outputStream, ZipArchiveMode.Create))
            {
                foreach (ZipArchiveEntry entry in inputArchive.Entries)
                {
                    ZipArchiveEntry newEntry = outputArchive.CreateEntry(entry.FullName);

                    using Stream source = entry.Open();
                    using Stream destination = newEntry.Open();

                    if (entry.FullName == "content.xml")
                        contentDocument.Save(destination);
                    else if (entry.FullName == "styles.xml")
                        stylesDocument.Save(destination);
                    else
                        source.CopyTo(destination);
                }
            }

            return outputFile;
        }
    }
}
