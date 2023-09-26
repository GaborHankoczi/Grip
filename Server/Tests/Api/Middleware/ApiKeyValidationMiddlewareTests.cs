using System;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Threading.Tasks;
using Grip.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Tests.Grip.Api.Middleware
{
    /*
    [TestFixture]
    public class ApiKeyValidationMiddlewareTests
    {
        public class PipeWriterWrapper : PipeWriter
        {
            public override void Advance(int bytes)
            {

            }

            public override void CancelPendingFlush()
            {

            }

            public override void Complete(Exception? exception = null)
            {

            }

            public override ValueTask<FlushResult> FlushAsync(CancellationToken cancellationToken = default)
            {
                return new ValueTask<FlushResult>();
            }

            public override Memory<byte> GetMemory(int sizeHint = 0)
            {
                return new Memory<byte>();
            }

            public override Span<byte> GetSpan(int sizeHint = 0)
            {
                return new Span<byte>();
            }
        }
        public class HttpResponseWrapper : HttpResponse
        {
            public virtual Task WriteAsync(string text, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public override PipeWriter BodyWriter => new PipeWriterWrapper();

            public override Task StartAsync(CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public override HttpContext HttpContext =>
            null;

            private int _statusCode;
            public override int StatusCode { get => _statusCode; set => _statusCode = value; }

            public override IHeaderDictionary Headers =>
            new HeaderDictionary();

            private Stream _body;
            public override Stream Body { get => _body; set => _body = value; }

            private long? _contentLength;
            public override long? ContentLength
            {
                get => _contentLength;
                set => _contentLength = value;
            }

            private string? _contentType;
            public override string? ContentType { get => _contentType; set => _contentType = value; }

            public override IResponseCookies Cookies =>
            null;

            public override bool HasStarted => false;

            public override void OnCompleted(Func<object, Task> callback, object state)
            {

            }

            public override void OnStarting(Func<object, Task> callback, object state)
            {

            }

            public override void Redirect(string location, bool permanent)
            {

            }
        }

        private Mock<RequestDelegate> _nextDelegateMock;
        private Mock<ILogger<DeChunkingMiddleware>> _loggerMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<IFeatureCollection> _featureCollectionMock;
        private Mock<HttpContext> _httpContextMock;
        private Mock<HttpResponseWrapper> _httpResponseMock;
        private ApiKeyValidationMiddleware _middleware;

        [SetUp]
        public void Setup()
        {
            _nextDelegateMock = new Mock<RequestDelegate>();
            _loggerMock = new Mock<ILogger<DeChunkingMiddleware>>();
            _configurationMock = new Mock<IConfiguration>();
            _featureCollectionMock = new Mock<IFeatureCollection>();
            _httpContextMock = new Mock<HttpContext>();
            // Response.WriteAsync cannot be mocked, as we cannot modify either the 
            _httpContextMock.Setup(c => c.Response).Returns(new HttpResponseWrapper());
            _httpContextMock.Setup(c => c.Features).Returns(_featureCollectionMock.Object);
            _featureCollectionMock.Setup(f => f.Get<IEndpointFeature>().Endpoint).Returns(
            new Endpoint(null, new EndpointMetadataCollection(new List<object>() { new ValidateApiKey() }), "test"));

            _middleware = new ApiKeyValidationMiddleware(
                _nextDelegateMock.Object,
                _loggerMock.Object,
                _configurationMock.Object);
        }

        [Test]
        public async Task InvokeAsync_ValidApiKey_DoesNotModifyResponse()
        {
            // Arrange
            var apiKey = "valid-api-key";
            var configurationApiKey = "valid-api-key";
            _httpContextMock.Setup(c => c.Request.Headers["ApiKey"]).Returns(apiKey);
            _configurationMock.Setup(c => c["Station:ApiKey"]).Returns(configurationApiKey);

            // Act
            await _middleware.InvokeAsync(_httpContextMock.Object);

            // Assert
            Assert.AreEqual(StatusCodes.Status200OK, _httpContextMock.Object.Response.StatusCode);
            Assert.IsNull(_httpContextMock.Object.Response.ContentType);
            _nextDelegateMock.Verify(next => next.Invoke(_httpContextMock.Object), Times.Once);
        }

        [Test]
        public async Task InvokeAsync_InvalidApiKey_ReturnsUnauthorizedResponse()
        {
            // Arrange
            var apiKey = "invalid-api-key";
            var configurationApiKey = "valid-api-key";
            _httpContextMock.Setup(c => c.Request.Headers["ApiKey"]).Returns(apiKey);
            _configurationMock.Setup(c => c["Station:ApiKey"]).Returns(configurationApiKey);

            // Act
            await _middleware.InvokeAsync(_httpContextMock.Object);

            // Assert
            Assert.AreEqual(StatusCodes.Status401Unauthorized, _httpContextMock.Object.Response.StatusCode);
            Assert.AreEqual("text/plain", _httpContextMock.Object.Response.ContentType);
            var responseContent = await GetResponseContent(_httpContextMock.Object.Response);
            Assert.AreEqual("Please provide a valid api key", responseContent);
            _nextDelegateMock.Verify(next => next.Invoke(It.IsAny<HttpContext>()), Times.Never);
        }

        [Test]
        public async Task InvokeAsync_NoApiKey_ReturnsUnauthorizedResponse()
        {
            // Arrange
            var configurationApiKey = "valid-api-key";
            var context = CreateHttpContextWithoutApiKey();
            _configurationMock.Setup(c => c["Station:ApiKey"]).Returns(configurationApiKey);

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            Assert.AreEqual(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
            Assert.AreEqual("text/plain", context.Response.ContentType);
            var responseContent = await GetResponseContent(context.Response);
            Assert.AreEqual("Please provide a valid api key", responseContent);
            _nextDelegateMock.Verify(next => next.Invoke(It.IsAny<HttpContext>()), Times.Never);
        }

        private HttpContext CreateHttpContextWithApiKey(string apiKey)
        {
            var request = new Mock<HttpRequest>();
            request.Setup(r => r.Headers["ApiKey"]).Returns(apiKey);
            _httpContextMock.Setup(c => c.Request).Returns(request.Object);
            return _httpContextMock.Object;
        }

        private HttpContext CreateHttpContextWithoutApiKey()
        {
            var request = new Mock<HttpRequest>();
            _httpContextMock.Setup(c => c.Request).Returns(request.Object);
            return _httpContextMock.Object;
        }

        private async Task<string> GetResponseContent(HttpResponse response)
        {
            var responseStream = new MemoryStream();
            response.Body = responseStream;
            await _middleware.InvokeAsync(_httpContextMock.Object);
            responseStream.Seek(0, SeekOrigin.Begin);
            return await new StreamReader(responseStream).ReadToEndAsync();
        }
    }
    */
}
