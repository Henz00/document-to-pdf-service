using System.Diagnostics;

namespace ODTDOCXtoPDFConverter.Api.Services
{
    public class PdfConverterService : IPdfConverterService
    {
        private readonly IConfiguration _configuration;

        public PdfConverterService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> ConvertAsync(string file, string outputDirectory)
        {
            string libreOfficePath = _configuration["LibreOffice:Path"];
            if (string.IsNullOrWhiteSpace(libreOfficePath))
                throw new InvalidOperationException("LibreOffice path is not configured.");

            ProcessStartInfo startInfo = new()
            {
                FileName = libreOfficePath,
                Arguments = $"--headless --convert-to pdf --outdir \"{outputDirectory}\" \"{file}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using Process process = Process.Start(startInfo)!;

            string standardOutput = process.StandardOutput.ReadToEnd();
            string standardError = process.StandardError.ReadToEnd();

            await process.WaitForExitAsync();
            string pdfFile = Path.Combine(outputDirectory, "output.pdf");
            
            return pdfFile;
        }
    }
}
