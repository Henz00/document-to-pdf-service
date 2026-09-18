using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ODTDOCXtoPDFConverter.Api.Controllers;
using ODTDOCXtoPDFConverter.Api.Models;
using ODTDOCXtoPDFConverter.Api.Tests.Infrastructure;

namespace ODTDOCXtoPDFConverter.Api.Tests.Security;

public class UploadSizeTests
{

    [Fact]
    public async Task Convert_WhenDocumentExceedsLimit_Returns413()
    {
        // Arrange: generate a file one byte above the allowed size.
        using var documentStream = new MemoryStream(
            new byte[
                checked((int)UploadLimits.MaxDocumentBytes + 1)
            ]
        );

        using var variablesStream = new MemoryStream(
            System.Text.Encoding.UTF8.GetBytes("{}")
        );

        var document = new FormFile(
            documentStream,
            0,
            documentStream.Length,
            "document",
            "oversized.docx"
        );

        var variables = new FormFile(
            variablesStream,
            0,
            variablesStream.Length,
            "variables",
            "variables.json"
        );

        // This rejection path should return before using the service.
        var controller = new DocumentController(null!);

        // Act
        var result = await controller.ConvertDocument(
            document,
            variables,
            CancellationToken.None
        );

        // Assert
        var response = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status413PayloadTooLarge, response.StatusCode);
    }

    [Fact]
    public async Task Convert_WhenVariablesDocumentExceedsLimit_Returns413()
    {
        // Arrange: generate a file one byte above the allowed size.
        using var documentStream = new MemoryStream(
            new byte[
                checked(1024)
            ]
        );

        using var variablesStream = new MemoryStream(
            new byte[
                checked((int)UploadLimits.MaxVariableDocumentBytes + 1)
            ]
        );

        var document = new FormFile(
            documentStream,
            0,
            documentStream.Length,
            "document",
            "oversized.docx"
        );

        var variables = new FormFile(
            variablesStream,
            0,
            variablesStream.Length,
            "variables",
            "variables.json"
        );

        // This rejection path should return before using the service.
        var controller = new DocumentController(null!);

        // Act
        var result = await controller.ConvertDocument(
            document,
            variables,
            CancellationToken.None
        );

        // Assert
        var response = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status413PayloadTooLarge, response.StatusCode);
    }

    [Fact]
    public async Task Extract_WhenDocumentExceedsLimit_Returns413()
    {
        // Arrange: generate a file one byte above the allowed size.
        using var documentStream = new MemoryStream(
            new byte[
                checked((int)UploadLimits.MaxDocumentBytes + 1)
            ]
        );

        var document = new FormFile(
            documentStream,
            0,
            documentStream.Length,
            "document",
            "oversized.docx"
        );

        // This rejection path should return before using the service.
        var controller = new DocumentVariableExtractionController(null!);

        // Act
        var result = await controller.GetExtractedVariables(
            document,
            CancellationToken.None
        );

        // Assert
        var response = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status413PayloadTooLarge, response.StatusCode);
    }

    [Fact]
    public async Task Convert_WhenDocumentAtMaxLimit_CallsServiceAndReturnsFile()
    {
        // Arrange: generate a file exactly at the allowed size.
        using var documentStream = new MemoryStream(
            new byte[
                checked((int)UploadLimits.MaxDocumentBytes)
            ]
        );

        using var variablesStream = new MemoryStream(
            System.Text.Encoding.UTF8.GetBytes("{}")
        );

        var document = new FormFile(
            documentStream,
            0,
            documentStream.Length,
            "document",
            "oversized.docx"
        );

        var variables = new FormFile(
            variablesStream,
            0,
            variablesStream.Length,
            "variables",
            "variables.json"
        );

        var service = new FakeDocumentService();
        var controller = new DocumentController(service);

        // Act
        var result = await controller.ConvertDocument(
            document,
            variables,
            CancellationToken.None
        );

        // Assert
        var response = Assert.IsType<FileContentResult>(result);

        Assert.Equal(1, service.CallCount);
        Assert.Equal("application/pdf", response.ContentType);
        Assert.Equal(service.PdfBytes, response.FileContents);
    }

    [Fact]
    public async Task Convert_WhenVariablesDocumentAtMaxLimit_CallsServiceAndReturnsFile()
    {
        // Arrange: generate a file exactly at the allowed size.
        using var documentStream = new MemoryStream(
            new byte[
                checked((int)1024)
            ]
        );

        using var variablesStream = new MemoryStream(
            new byte[
                checked((int)UploadLimits.MaxVariableDocumentBytes)
            ]
        );

        var document = new FormFile(
            documentStream,
            0,
            documentStream.Length,
            "document",
            "oversized.docx"
        );

        var variables = new FormFile(
            variablesStream,
            0,
            variablesStream.Length,
            "variables",
            "variables.json"
        );

        var service = new FakeDocumentService();
        var controller = new DocumentController(service);

        // Act
        var result = await controller.ConvertDocument(
            document,
            variables,
            CancellationToken.None
        );

        // Assert
        var response = Assert.IsType<FileContentResult>(result);

        Assert.Equal(1, service.CallCount);
        Assert.Equal("application/pdf", response.ContentType);
        Assert.Equal(service.PdfBytes, response.FileContents);
    }

    [Fact]
    public async Task Extract_WhenDocumentAtMaxLimit_CallsServiceAndReturnsVariables()
    {
        // Arrange: generate a file at exactly allowed size.
        using var documentStream = new MemoryStream(
            new byte[
                checked((int)UploadLimits.MaxVariableDocumentBytes)
            ]
        );

        var document = new FormFile(
            documentStream,
            0,
            documentStream.Length,
            "document",
            "oversized.docx"
        );

        var service = new FakeDocumentVariableExtractorService();
        var controller = new DocumentVariableExtractionController(service);

        // Act
        var result = await controller.GetExtractedVariables(
            document,
            CancellationToken.None
        );

        // Assert
        var extractedVariables = Assert.IsType<List<string>>(result.Value);

        Assert.Equal(1, service.CallCount);
        Assert.Equal(new List<string>(), extractedVariables);
    }

}