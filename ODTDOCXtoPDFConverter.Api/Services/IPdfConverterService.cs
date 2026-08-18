namespace ODTDOCXtoPDFConverter.Api.Services
{
    public interface IPdfConverterService
    {
        Task<string> ConvertAsync(string file, string outputDirectory);
    }
}
