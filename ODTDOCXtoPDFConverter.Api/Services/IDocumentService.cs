namespace ODTDOCXtoPDFConverter.Api.Services;

public interface IDocumentService
{
    Task<byte[]> ConvertAsync(
        IFormFile document,
        IFormFile variables,
        CancellationToken cancellationToken
    );
}

