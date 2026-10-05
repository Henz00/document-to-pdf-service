using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ODTDOCXtoPDFConverter.Api.Controllers;
using ODTDOCXtoPDFConverter.Api.Models;
using ODTDOCXtoPDFConverter.Api.Services;
using ODTDOCXtoPDFConverter.Api.Tests.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace ODTDOCXtoPDFConverter.Api.Tests.Security;

public class DocumentValidatorTests
{
    [Fact]
    public async Task ValidateDocument_UploadEmptyDocument_ReturnsFalse()
    {
        // arrange
        var validator = new DocumentValidatorService();

        var documentStream = new MemoryStream(
            []
        );

        var document = new FormFile(
            documentStream,
            0,
            documentStream.Length,
            "document",
            "empty.docx"
        );

        // act
        var result = validator.ValidateDocument(document);

        // assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ValidateDocument_UploadNullDocument_ReturnsFalse()
    {
        // arrange
        var validator = new DocumentValidatorService();

        // act
        var result = validator.ValidateDocument(null);

        // assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ValidateDocument_UploadWrongDocumentType_ReturnsFalse()
    {
        // arrange
        var validator = new DocumentValidatorService();

        var filePath = Path.Combine(AppContext.BaseDirectory, "TestDocuments", "wrongType.rar");

        await using var stream = File.OpenRead(filePath);
        IFormFile document = new FormFile(stream, 0, stream.Length, "document", Path.GetFileName(filePath));

        // act
        var result = validator.ValidateDocument(document);

        // assert
        Assert.False(result.IsValid);

    }

    [Fact]
    public async Task ValidateDocument_UploadCorruptDocxDocumentType_ReturnsFalse()
    {
        // arrange
        var validator = new DocumentValidatorService();

        var filePath = Path.Combine(AppContext.BaseDirectory, "TestDocuments", "corrupt.docx");

        await using var stream = File.OpenRead(filePath);
        IFormFile document = new FormFile(stream, 0, stream.Length, "document", Path.GetFileName(filePath));

        // act
        var result = validator.ValidateDocument(document);

        // assert
        Assert.False(result.IsValid);

    }

    [Fact]
    public async Task ValidateDocument_UploadCorruptOdtDocumentType_ReturnsFalse()
    {
        // arrange
        var validator = new DocumentValidatorService();

        var filePath = Path.Combine(AppContext.BaseDirectory, "TestDocuments", "corrupt.odt");

        await using var stream = File.OpenRead(filePath);
        IFormFile document = new FormFile(stream, 0, stream.Length, "document", Path.GetFileName(filePath));

        // act
        var result = validator.ValidateDocument(document);

        // assert
        Assert.False(result.IsValid);

    }

    [Fact]
    public async Task ValidateDocument_UploadAcceptedDocxDocumentType_ReturnsTrue()
    {
        // arrange
        var validator = new DocumentValidatorService();

        var filePath = Path.Combine(AppContext.BaseDirectory, "TestDocuments", "test.docx");

        await using var stream = File.OpenRead(filePath);
        IFormFile document = new FormFile(stream, 0, stream.Length, "document", Path.GetFileName(filePath));

        // act
        var result = validator.ValidateDocument(document);

        // assert
        Assert.True(result.IsValid);

    }

    [Fact]
    public async Task ValidateDocument_UploadAcceptedOdtDocumentType_ReturnsTrue()
    {
        // arrange
        var validator = new DocumentValidatorService();

        var filePath = Path.Combine(AppContext.BaseDirectory, "TestDocuments", "test.odt");

        await using var stream = File.OpenRead(filePath);
        IFormFile document = new FormFile(stream, 0, stream.Length, "document", Path.GetFileName(filePath));

        // act
        var result = validator.ValidateDocument(document);

        // assert
        Assert.True(result.IsValid);

    }
}