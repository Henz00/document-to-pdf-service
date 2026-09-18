using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using ODTDOCXtoPDFConverter.Api.Models;

namespace ODTDOCXtoPDFConverter.Api.Tests.Security;

// Start TEST-docker-compose.yml before running Category=Deployment.
[Trait("Category", "Deployment")]
public class UploadSizeDeploymentTests
{
    [Fact]
    public async Task Convert_WhenRequestExceedsNginxLimit_Returns413BeforeAuthentication()
    {
        // Arrange: use the real Nginx/API stack, without a test factory or login.
        using var handler = new HttpClientHandler { AllowAutoRedirect = false };
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://127.0.0.1:4201"),
            Timeout = TimeSpan.FromSeconds(10)
        };

        await Async_TestApiConnection(client);

        // Control: a small request reaches the API and requires authentication.
        using var smallForm = new MultipartFormDataContent();
        smallForm.Add(
            new ByteArrayContent([0]), "document", "small.docx"
        );
        smallForm.Add(
            new StringContent("{}"), "variables", "variables.json"
        );

        using var smallResponse = await client.PostAsync("/api/document", smallForm);

        Assert.Equal(HttpStatusCode.Unauthorized, smallResponse.StatusCode);

        // The file contents alone exceed 12 MiB; multipart overhead adds more.
        using var form = new MultipartFormDataContent();
        form.Add(
            new ByteArrayContent(new byte[checked((int)UploadLimits.MaxRequestBytes + 1)]),
            "document",
            "oversized.docx"
        );
        form.Add(new StringContent("{}"), "variables", "variables.json");
        Assert.True(form.Headers.ContentLength > UploadLimits.MaxRequestBytes);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/document")
        {
            Content = form
        };
        request.Headers.ExpectContinue = true;

        // Act
        using var response = await client.SendAsync(request);

        // Assert: the proxy rejects this before the API's authentication check.
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(
            response.StatusCode == HttpStatusCode.RequestEntityTooLarge,
            $"Expected 413, got {(int)response.StatusCode}. Body: {body}");
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("nginx", body.ToLowerInvariant());
    }

    [Fact]
    public async Task Convert_WhenRequestExceedsNginxLimit_Returns413AfterAuthentication()
    {
        // Arrange
        using var handler = new HttpClientHandler { AllowAutoRedirect = false };
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://127.0.0.1:4201"),
            Timeout = TimeSpan.FromSeconds(10)
        };
        using var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                Username = "deployment-test",
                Password = "!123Test"
            });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        using var meResponse = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        await Async_TestApiConnection(client);

        // The file contents alone exceed 12 MiB; multipart overhead adds more.
        using var form = new MultipartFormDataContent();
        form.Add(
            new ByteArrayContent(new byte[checked((int)UploadLimits.MaxRequestBytes + 1)]),
            "document",
            "oversized.docx"
        );
        form.Add(new StringContent("{}"), "variables", "variables.json");
        Assert.True(form.Headers.ContentLength > UploadLimits.MaxRequestBytes);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/document")
        {
            Content = form
        };
        request.Headers.ExpectContinue = true;

        // Act
        using var response = await client.SendAsync(request);

        // Assert: the proxy rejects this before the API's authentication check.
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(
            response.StatusCode == HttpStatusCode.RequestEntityTooLarge,
            $"Expected 413, got {(int)response.StatusCode}. Body: {body}");
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("nginx", body.ToLowerInvariant());
    }

    [Fact]
    public async Task Convert_WhenRequestValidDocument_Returns200OK()
    {
        // Arrange
        using var handler = new HttpClientHandler { AllowAutoRedirect = false };
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://127.0.0.1:4201"),
            Timeout = TimeSpan.FromSeconds(10)
        };

        await Async_TestApiConnection(client);

        using var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new {
                    Username = "deployment-test",
                    Password = "!123Test"
            }
        );

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // Ensure authentication state
        using var meResponse = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        // Valid file upload
        var documentPath = Path.Combine(
            AppContext.BaseDirectory,
            "TestDocuments",
            "test.docx");

        using var form = new MultipartFormDataContent();
        form.Add(
            new StreamContent(File.OpenRead(documentPath)),
            "document",
            "valid.docx"
        );
        form.Add(
            new StringContent("{}"),
            "variables",
            "variables.json"
        );

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/document")
        {
            Content = form
        };
        request.Headers.ExpectContinue = true;

        // Act
        using var response = await client.SendAsync(request);

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            Assert.Fail($"Expected 200, got {(int)response.StatusCode}. Body: {errorBody}");
        }

        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);

        var pdfBytes = await response.Content.ReadAsByteArrayAsync();

        Assert.NotEmpty(pdfBytes);
        Assert.True(pdfBytes.AsSpan().StartsWith("%PDF-"u8), "Expected output beginning with the PDF signature.");
    }

    [Fact]
    public async Task Convert_WhenHttpRequestExceedsNginxLimit_Returns413()
    {
        // arrange
        using var handler = new HttpClientHandler { AllowAutoRedirect = false };
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://127.0.0.1:4201"),
            Timeout = TimeSpan.FromSeconds(10)
        };

        await Async_TestApiConnection(client);

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

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/document")
        {
            Content = form
        };
        request.Headers.ExpectContinue = true;

        Assert.True(form.Headers.ContentLength > UploadLimits.MaxRequestBytes);

        // Act
        using var response = await client.SendAsync(request);

        // assert
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.RequestEntityTooLarge, $"Expected 413, got {(int)response.StatusCode}");

        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);

        Assert.Contains("nginx", body.ToLowerInvariant());
    }

    [Fact]
    public async Task Convert_WhenDocumentExceedsApiLimitButTotalRequestDoesNot_Returns413()
    {
        // Arrange
        using var handler = new HttpClientHandler { AllowAutoRedirect = false };
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://127.0.0.1:4201"),
            Timeout = TimeSpan.FromSeconds(10)
        };

        await Async_TestApiConnection(client);

        using var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                Username = "deployment-test",
                Password = "!123Test"
            }
        );

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // Ensure authentication state
        using var meResponse = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        using var form = new MultipartFormDataContent();

        form.Add(
            new ByteArrayContent(
                new byte[checked((int)UploadLimits.MaxDocumentBytes + 1)]
            ),
            "document",
            "document.docx"
        );

        form.Add(
            new StringContent("{}"),
            "variables",
            "variables.json"
        );

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/document")
        {
            Content = form
        };
        request.Headers.ExpectContinue = true;

        Assert.True(form.Headers.ContentLength < UploadLimits.MaxRequestBytes);

        // Act
        using var response = await client.SendAsync(request);

        // Assert
        var body = await response.Content.ReadAsStringAsync();

        Assert.True(
            response.StatusCode == HttpStatusCode.RequestEntityTooLarge,
            $"Upload: expected 413, got {(int)response.StatusCode}. Body: {body}"
        );

        Assert.Contains("Document file limit is 10 MiB", body);
    }

    [Fact]
    public async Task Convert_WhenVariablesDocumentExceedsLimitButTotalRequestDoesNot_Return413()
    {
        // Arrange
        using var handler = new HttpClientHandler { AllowAutoRedirect = false };
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://127.0.0.1:4201"),
            Timeout = TimeSpan.FromSeconds(10)
        };

        await Async_TestApiConnection(client);

        using var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                Username = "deployment-test",
                Password = "!123Test"
            }
        );

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // Ensure authentication state
        using var meResponse = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        using var form = new MultipartFormDataContent();

        form.Add(
            new ByteArrayContent(
                new byte[checked(1)]
            ),
            "document",
            "document.docx"
        );

        form.Add(
            new ByteArrayContent(new byte[checked((int)UploadLimits.MaxVariableDocumentBytes + 1)]),
            "variables",
            "variables.json"
        );

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/document")
        {
            Content = form
        };
        request.Headers.ExpectContinue = true;

        Assert.True(form.Headers.ContentLength < UploadLimits.MaxRequestBytes);

        // Act
        using var response = await client.SendAsync(request);

        // Assert
        var body = await response.Content.ReadAsStringAsync();

        Assert.True(
            response.StatusCode == HttpStatusCode.RequestEntityTooLarge,
            $"Upload: expected 413, got {(int)response.StatusCode}. Body: {body}"
        );

        Assert.Contains("Document file limit is 10 MiB", body);
    }


    private static async Task Async_TestApiConnection(HttpClient client)
    {
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        string lastResult = "No response received.";

        try
        {
            while (true)
            {
                try
                {
                    // A 401 proves that Nginx can reach the protected API route.
                    using var response = await client.GetAsync("/api/auth/me", deadline.Token);
                    if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.OK)
                        return;

                    lastResult = $"HTTP {(int)response.StatusCode}";
                }
                catch (HttpRequestException ex)
                {
                    lastResult = ex.Message;
                }
                catch (OperationCanceledException) when (!deadline.IsCancellationRequested)
                {
                    lastResult = "Request timed out.";
                }

                await Task.Delay(500, deadline.Token);
            }
        }
        catch (OperationCanceledException) when (deadline.IsCancellationRequested)
        {
            throw new TimeoutException(
                "Test stack was not ready at http://127.0.0.1:4201 within 30 seconds. " +
                "Start it with: docker compose -f TEST-docker-compose.yml up --build -d. " +
                $"Last result: {lastResult}");
        }
    }
}
