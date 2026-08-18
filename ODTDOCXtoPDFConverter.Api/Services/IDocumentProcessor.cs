namespace ODTDOCXtoPDFConverter.Api.Services
{
    public interface IDocumentProcessor
    {
        bool CanProcess(string extension);

        Task<string> ProcessAsync(IFormFile document, Dictionary<string,string> variables, string directory, CancellationToken cancellationToken);
    }
}
