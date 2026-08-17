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
        public DocumentService(ILogger<DocumentService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<byte[]> ConvertAsync(
        IFormFile document,
        IFormFile variables,
        CancellationToken cancellationToken)
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDirectory);

            try
            {
                using Stream documentStream = document.OpenReadStream();
                using ZipArchive archive = new(documentStream, ZipArchiveMode.Read);

                ZipArchiveEntry? contentEntry = archive.GetEntry("content.xml");
                ZipArchiveEntry? stylesEntry = archive.GetEntry("styles.xml");

                if (contentEntry is null || stylesEntry is null)
                    throw new Exception("xml file not found!");

                using Stream stream = contentEntry.Open();
                using Stream stylesStream = stylesEntry.Open();
                XDocument xmlDocument = XDocument.Load(stream);
                XDocument stylesDocument = XDocument.Load(stylesStream);
                
                XNamespace office = "urn:oasis:names:tc:opendocument:xmlns:office:1.0";
                XNamespace style = "urn:oasis:names:tc:opendocument:xmlns:style:1.0";

                

                XElement? body = xmlDocument
                    .Element(office + "document-content")?
                    .Element(office + "body")?
                    .Element(office + "text");

                XNamespace text = "urn:oasis:names:tc:opendocument:xmlns:text:1.0";

                IEnumerable<XElement> headersTextElements = stylesDocument.Descendants(style + "header");
                IEnumerable<XElement> footersTextElements = stylesDocument.Descendants(style + "footer");
                IEnumerable<XElement> bodyTextElements = body!.Descendants(text + "p");
                IEnumerable<XElement> combinedElements = bodyTextElements.Concat(footersTextElements).Concat(headersTextElements);

                string combinedText = "";
                foreach (XElement element in combinedElements)
                    combinedText += element.Value + "\n";

                List<string> regex_variables = Regex
                    .Matches(combinedText, @"\{([^{}\r\n]+)\}")
                    .Select(match => match.Groups[1].Value)
                    .Distinct()
                    .ToList();

                using Stream variablesStream = variables.OpenReadStream();

                Dictionary<string, string>? jsonVariables = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(variablesStream, cancellationToken: cancellationToken);

                if (jsonVariables is null)
                    throw new Exception("Could not deserialize variables.json");

                foreach (XElement element in combinedElements)
                {
                    foreach (XText textNode in element.DescendantNodes().OfType<XText>())
                    {
                        string textToTranslate = textNode.Value;

                        foreach (string variableName in regex_variables)
                        {
                            if (!jsonVariables.TryGetValue(variableName, out string? value))
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



                string inputFile = Path.Combine(tempDirectory, "input.odt");
                await using (FileStream fileStream = File.Create(inputFile))
                {
                    await document.CopyToAsync(fileStream, cancellationToken);
                }


                using ZipArchive inputArchive = ZipFile.OpenRead(inputFile);
                string outputFile = Path.Combine(tempDirectory, "output.odt");
                using (FileStream outputStream = File.Create(outputFile))
                using (ZipArchive outputArchive = new(outputStream, ZipArchiveMode.Create))
                {
                    foreach (ZipArchiveEntry entry in inputArchive.Entries)
                    {
                        ZipArchiveEntry newEntry = outputArchive.CreateEntry(entry.FullName);

                        using Stream source = entry.Open();
                        using Stream destination = newEntry.Open();

                        if (entry.FullName == "content.xml")
                            xmlDocument.Save(destination);
                        else if (entry.FullName == "styles.xml")
                            stylesDocument.Save(destination);
                        else
                            source.CopyTo(destination);
                    }
                }


                string libreOfficePath = _configuration["LibreOffice:Path"];
                if (string.IsNullOrWhiteSpace(libreOfficePath))
                    throw new InvalidOperationException("LibreOffice path is not configured.");

                ProcessStartInfo startInfo = new()
                {
                    FileName = libreOfficePath,
                    Arguments = $"--headless --convert-to pdf --outdir \"{tempDirectory}\" \"{outputFile}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using Process process = Process.Start(startInfo)!;

                string standardOutput = process.StandardOutput.ReadToEnd();
                string standardError = process.StandardError.ReadToEnd();

                process.WaitForExit();
                string pdfFile = Path.Combine(tempDirectory, "output.pdf");

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
