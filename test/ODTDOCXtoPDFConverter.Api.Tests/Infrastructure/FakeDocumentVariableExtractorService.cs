using Microsoft.AspNetCore.Http;
using ODTDOCXtoPDFConverter.Api.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace ODTDOCXtoPDFConverter.Api.Tests.Infrastructure;

public sealed class FakeDocumentVariableExtractorService : IDocumentVariableExtractorService
{
    public int CallCount { set; get; }
    public List<string> Variables { get; } = new List<string>();
    public Task<List<string>> ExtractVariables(IFormFile document)
    {
        CallCount++;
        return Task.FromResult(new List<string>());
    }
}
