using Microsoft.AspNetCore.Http;
using ODTDOCXtoPDFConverter.Api.Services;

namespace ODTDOCXtoPDFConverter.Api.Tests.Infrastructure;

public sealed class FakeDocumentService : IDocumentService
{
    private int _callCount;

    public int CallCount => Volatile.Read(ref _callCount);

    public byte[] PdfBytes { get; } = [1, 2, 3];

    public Task<byte[]> ConvertAsync(
        IFormFile document,
        IFormFile variables,
        CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _callCount);
        return Task.FromResult(PdfBytes);
    }
}