using Microsoft.AspNetCore.Http;
using ODTDOCXtoPDFConverter.Api.Models;
using ODTDOCXtoPDFConverter.Api.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace ODTDOCXtoPDFConverter.Api.Tests.Infrastructure;

public sealed class FakeDocumentValidatorService : IDocumentValidatorService
{
    public DocumentValidationResult ValidateDocument(IFormFile? document)
    {
        return new DocumentValidationResult(true, "Fake service");
    }
}
