using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ContactService.Tests.Middleware
{
    public class ExceptionHandlingMiddlewareTests
    {
        [Fact]
        public async Task InvokeAsync_ShouldHandleException()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var next = new RequestDelegate(_ => throw new Exception("Test exception"));
            var middleware = new ExceptionHandlingMiddleware(next);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_ShouldCallNext_WhenNoException()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var wasCalled = false;
            var next = new RequestDelegate(_ => { wasCalled = true; return Task.CompletedTask; });
            var middleware = new ExceptionHandlingMiddleware(next);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.True(wasCalled);
            Assert.NotEqual(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        }
    }
}