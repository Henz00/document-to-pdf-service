using System.Net;
using System.Text.Json;
using ODTDOCXtoPDFConverter.Api.Models;
using ODTDOCXtoPDFConverter.Api.Tests.Infrastructure;

namespace ODTDOCXtoPDFConverter.Api.Tests.Security;

public class UploadSizeHttpTests
{
    [Fact]
    public async Task Convert_SmallDocumentAndVariablesFile_Returns200OK()
    {
        // Arrange
        using var factory = new ApiTestFactory();
        factory.ClientOptions.AllowAutoRedirect = false;
        using var client = factory.CreateClient();

        using var form = new MultipartFormDataContent();

        form.Add(
            new ByteArrayContent(
                new byte[checked((int)1)]
            ),
            "document",
            "oversized.docx"
        );

        form.Add(
            new StringContent("{}"),
            "variables",
            "variables.json"
        );

        // Act
        using var response = await client.PostAsync("/api/document", form);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, factory.DocumentService.CallCount);
    }

    [Fact]
    public async Task Convert_WhenVariableDocumentExceedsLimit_Returns413()
    {
        // Arrange
        using var factory = new ApiTestFactory();
        factory.ClientOptions.AllowAutoRedirect = false;
        using var client = factory.CreateClient();

        using var form = new MultipartFormDataContent();

        form.Add(
            new ByteArrayContent(
                new byte[checked((int)1)]
            ),
            "document",
            "oversized.docx"
        );

        form.Add(
            new ByteArrayContent(
                new byte[checked((int)UploadLimits.MaxVariableDocumentBytes + 1)]
            ),
            "variables",
            "variables.json"
        );

        // Act
        using var response = await client.PostAsync("/api/document", form);

        // Assert
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Equal(0, factory.DocumentService.CallCount);
    }

    [Fact]
    public async Task Convert_WhenDocumentExceedsLimit_Returns413()
    {
        // Arrange
        using var factory = new ApiTestFactory();
        factory.ClientOptions.AllowAutoRedirect = false;
        using var client = factory.CreateClient();

        using var form = new MultipartFormDataContent();

        form.Add(
            new ByteArrayContent(
                new byte[checked((int)UploadLimits.MaxDocumentBytes + 1)]
            ),
            "document",
            "oversized.docx"
        );

        form.Add(
            new StringContent("{}"),
            "variables",
            "variables.json"
        );

        // Act
        using var response = await client.PostAsync("/api/document", form);

        // Assert
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Equal(0, factory.DocumentService.CallCount);
    }

    [Fact]
    public async Task Extract_WhenDocumentExceedsLimit_Returns413()
    {
        // Arrange
        using var factory = new ApiTestFactory();
        factory.ClientOptions.AllowAutoRedirect = false;
        using var client = factory.CreateClient();

        using var form = new MultipartFormDataContent();

        form.Add(
            new ByteArrayContent(
                new byte[checked((int)UploadLimits.MaxDocumentBytes + 1)]
            ),
            "document",
            "oversized.docx"
        );

        form.Add(
            new StringContent("{}"),
            "variables",
            "variables.json"
        );

        // Act
        using var response = await client.PostAsync("/api/document/extract", form);

        // Assert
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Equal(0, factory.VariableExtractorService.CallCount);
    }

    [Fact]
    public async Task Convert_WhenDocumentAtMaxLimit_Returns200()
    {
        // Arrange
        using var factory = new ApiTestFactory();
        factory.ClientOptions.AllowAutoRedirect = false;
        using var client = factory.CreateClient();

        using var form = new MultipartFormDataContent();

        form.Add(
            new ByteArrayContent(
                new byte[checked((int)UploadLimits.MaxDocumentBytes)]
            ),
            "document",
            "oversized.docx"
        );

        form.Add(
            new StringContent("{}"),
            "variables",
            "variables.json"
        );

        // Act
        using var response = await client.PostAsync("/api/document", form);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, factory.DocumentService.CallCount);
    }

    [Fact]
    public async Task Convert_WhenVariableDocumentAtMaxLimit_Returns200()
    {
        // Arrange
        using var factory = new ApiTestFactory();
        factory.ClientOptions.AllowAutoRedirect = false;
        using var client = factory.CreateClient();

        using var form = new MultipartFormDataContent();

        form.Add(
            new ByteArrayContent(
                new byte[checked((int)1)]
            ),
            "document",
            "oversized.docx"
        );

        form.Add(
            new ByteArrayContent(
                new byte[checked((int)UploadLimits.MaxVariableDocumentBytes)]
            ),
            "variables",
            "variables.json"
        );

        // Act
        using var response = await client.PostAsync("/api/document", form);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, factory.DocumentService.CallCount);
    }

    [Fact]
    public async Task Extract_WhenDocumentAtMaxLimit_Returns200()
    {
        // Arrange
        using var factory = new ApiTestFactory();
        factory.ClientOptions.AllowAutoRedirect = false;
        using var client = factory.CreateClient();

        using var form = new MultipartFormDataContent();

        form.Add(
            new ByteArrayContent(
                new byte[checked((int)UploadLimits.MaxDocumentBytes)]
            ),
            "document",
            "oversized.docx"
        );

        form.Add(
            new StringContent("{}"),
            "variables",
            "variables.json"
        );

        // Act
        using var response = await client.PostAsync("/api/document/extract", form);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, factory.VariableExtractorService.CallCount);
    }

    [Fact]
    public async Task Convert_WhenHttpBodyExceedsLimit_Returns400WithoutConversion()
    {
        // Arrange
        using var factory = new ApiTestFactory();
        factory.ClientOptions.AllowAutoRedirect = false;
        using var client = factory.CreateClient();

        using var form = new MultipartFormDataContent();

        form.Add(
            new ByteArrayContent(
                new byte[checked((int)UploadLimits.MaxDocumentBytes)]
            ),
            "document",
            "document.docx"
        );

        form.Add(
            new StringContent("{}"),
            "variables",
            "variables.json"
        );

        // Document + padding alone exceed the whole-request limit.
        var paddingBytes = checked((int)(
            UploadLimits.MaxRequestBytes - UploadLimits.MaxDocumentBytes + 1)
        );

        form.Add(
            new ByteArrayContent(new byte[paddingBytes]),
            "padding",
            "padding.bin"
        );

        Assert.True(form.Headers.ContentLength > UploadLimits.MaxRequestBytes);

        // Act
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/document")
        {
            Content = form
        };

        request.Headers.ExpectContinue = true;

        using var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, factory.DocumentService.CallCount);

        var responseBody = await response.Content.ReadAsStringAsync();
        using var problem = JsonDocument.Parse(responseBody);

        var errors = problem.RootElement
            .GetProperty("errors")
            .GetProperty("")
            .EnumerateArray()
            .Select(error => error.GetString() ?? "")
            .ToArray();

        Assert.Contains(errors, error =>
            error.Contains("Request body too large.") &&
            error.Contains(UploadLimits.MaxRequestBytes.ToString()));
    }

    //[Fact]
    //public async Task TemplateTest()
    //{
    //    // Arrange
    //    using var factory = new ApiTestFactory();
    //    factory.ClientOptions.AllowAutoRedirect = false;
    //    using var client = factory.CreateClient();

    //    using var form = new MultipartFormDataContent();

    //    form.Add(
    //        new ByteArrayContent(
    //            new byte[checked((int)1)]
    //        ),
    //        "document",
    //        "oversized.docx"
    //    );

    //    form.Add(
    //        new StringContent("{}"),
    //        "variables",
    //        "variables.json"
    //    );

    //    // Act
    //    using var response = await client.PostAsync("/api/document", form);

    //    // Assert
    //    Assert.Equal();
    //}

}