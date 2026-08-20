using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ODTDOCXtoPDFConverter.Api.Services
{
    public class DocumentVariableExtractorService
    {
        public async Task<List<string>> ExtractVariables(IFormFile document)
        {
            string extension = Path.GetExtension(document.FileName).ToLowerInvariant();
            List<string> regex_variables = new List<string>();

            if (extension == ".odt")
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


                regex_variables = Regex
                    .Matches(combinedText, @"\{([^{}\r\n]+)\}")
                    .Select(match => match.Groups[1].Value)
                    .Distinct()
                    .ToList();

                return regex_variables;
            }
            else if (extension == ".docx")
            {
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
                    using var stream = entry.Open();

                    XDocument openedDocument = XDocument.Load(stream);
                    XNamespace w =
                        "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

                    foreach (XElement textElement in openedDocument.Descendants(w + "t"))
                    {
                        MatchCollection matches = Regex.Matches(textElement.Value, @"\{([^{}\r\n]+)\}");

                        foreach (Match match in matches)
                        {
                            regex_variables.Add(match.Groups[1].Value);
                        }
                    }
                }

                return regex_variables.Distinct().ToList();
            } else
            {
                throw new InvalidOperationException("File type not supported");
            }
        }
    }
}
