using Byteology.TypedHttpClients.Tests.TestServices;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Byteology.TypedHttpClients.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTypedHttpClient()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            services.AddTypedHttpClient<ITestService, JsonHttpClient<ITestService>>();
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            // Act
            ITestService service = serviceProvider.GetRequiredService<ITestService>();

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void AddTypedHttpClient_AbstractClient()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => services.AddTypedHttpClient<ITestService, TypedHttpClient<ITestService>>());
        }
    }
}
