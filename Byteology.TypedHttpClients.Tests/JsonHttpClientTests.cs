﻿using Byteology.TypedHttpClients.Tests.Mocks;
using Byteology.TypedHttpClients.Tests.TestServices;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Byteology.TypedHttpClients.Tests
{
    public class JsonHttpClientTests
    {
        [Fact]
        public void Success()
        {
            // Arrange
            using HttpClient httpClient = MockHttpClientFactory.Create(HttpStatusCode.OK, null);
            ITestService service = getService(httpClient);

            // Act
            Task result = service.NoResultActionAsync();

            // Assert
            Assert.True(result.IsCompletedSuccessfully);
        }

        [Fact]
        public void Success_Generic()
        {
            // Arrange
            TestServiceResult response = new ();
            using HttpClient httpClient = MockHttpClientFactory.Create(HttpStatusCode.OK, new StringContent(response.ToString()));
            ITestService service = getService(httpClient);

            // Act
            TestServiceResult result = service.ActionAsync().Result;

            // Assert
            Assert.Equal(response, result);
        }

        [Fact]
        public void Error()
        {
            // Arrange
            using HttpClient httpClient = MockHttpClientFactory.Create(HttpStatusCode.Forbidden, null);
            ITestService service = getService(httpClient);

            // Act
            Task result = service.NoResultActionAsync();

            // Assert
            Assert.ThrowsAny<Exception>(() => result.Wait());
        }

        [Fact]
        public void Error_Generic()
        {
            // Arrange
            using HttpClient httpClient = MockHttpClientFactory.Create(HttpStatusCode.Forbidden, null);
            ITestService service = getService(httpClient);

            // Act
            Task<TestServiceResult> result = service.ActionAsync();

            // Assert
            Assert.ThrowsAny<Exception>(() => result.Wait());
        }

        [Fact]
        public void Uri_Simple()
        {
            // Arrange
            string expectedUrl = "https://example.com/simpleuri";
            void assertUri(HttpRequestMessage m) => Assert.Equal(expectedUrl, m.RequestUri?.ToString());

            using HttpClient httpClient = MockHttpClientFactory.Create(HttpStatusCode.OK, null, assertUri);
            ITestService service = getService(httpClient);

            // Act
            Task result = service.SimpleUriAsync();
            result.Wait();

            // Assert
            Assert.True(result.IsCompletedSuccessfully);
        }

        [Fact]
        public void Uri_Simple_NoDash()
        {
            // Arrange
            static void assertUri(HttpRequestMessage m) => Assert.Equal("https://example.com/simpleuri", m.RequestUri?.ToString());

            using HttpClient httpClient = MockHttpClientFactory.Create(HttpStatusCode.OK, null, assertUri);
            ITestService service = getService(httpClient);

            // Act
            Task result = service.SimpleUriNoDashAsync();
            result.Wait();

            // Assert
            Assert.True(result.IsCompletedSuccessfully);
        }

        [Theory]
        [InlineData("test")]
        [InlineData(5)]
        [InlineData(5.8f)]
        [InlineData(true)]
        [InlineData(null)]
        public void Uri_Param(object param)
        {
            // Arrange
            string expectedUrl = $"https://example.com/paramuri/{param}";
            void assertUri(HttpRequestMessage m) => Assert.Equal(expectedUrl, m.RequestUri?.ToString());

            using HttpClient httpClient = MockHttpClientFactory.Create(HttpStatusCode.OK, null, assertUri);
            ITestService service = getService(httpClient);

            // Act
            Task result = service.ParamUriAsync(param);
            result.Wait();

            // Assert
            Assert.True(result.IsCompletedSuccessfully);
        }

        [Fact]
        public void Uri_Query()
        {
            // Arrange
            string expectedUrl = "https://example.com/query?i=5&s=asdf&b=True&f=5.4&n=&a=1&a=2&a=3&a=";
            void assertUri(HttpRequestMessage m) => Assert.Equal(expectedUrl, m.RequestUri?.ToString());

            using HttpClient httpClient = MockHttpClientFactory.Create(HttpStatusCode.OK, null, assertUri);
            ITestService service = getService(httpClient);

            // Act
            Task result = service.QueryAsync(5, "asdf", true, 5.4f, null, new int?[] { 1, 2, 3, null });
            result.Wait();

            // Assert
            Assert.True(result.IsCompletedSuccessfully);
        }

        [Fact]
        public void Body()
        {
            // Arrange
            string expectedUrl = "https://example.com/actionbody";
            TestServiceResult body = new();
            void assertUri(HttpRequestMessage m)
            {
                Assert.Equal(expectedUrl, m.RequestUri?.ToString());
                Assert.Equal(body.ToString(), m.Content.ReadAsStringAsync().Result);
            }

            using HttpClient httpClient = MockHttpClientFactory.Create(HttpStatusCode.OK, null, assertUri);
            ITestService service = getService(httpClient);

            // Act
            Task result = service.BodyActionAsync(body);
            result.Wait();

            // Assert
            Assert.True(result.IsCompletedSuccessfully);
        }

        [Fact]
        public void Verb()
        {
            // Arrange
            static void assertUri(HttpRequestMessage m) => Assert.Equal("VERB", m.Method.Method);

            using HttpClient httpClient = MockHttpClientFactory.Create(HttpStatusCode.OK, null, assertUri);
            ITestService service = getService(httpClient);

            // Act
            Task result = service.VerbAsync();
            result.Wait();

            // Assert
            Assert.True(result.IsCompletedSuccessfully);
        }

        [Fact]
        public void Invalid_OutParam()
        {
            // Arrange
            using HttpClient httpClient = MockHttpClientFactory.Create(HttpStatusCode.OK, null);
            ITestService service = getService(httpClient);

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => service.OutParamAsync(out int p).Wait());
        }

        [Fact]
        public void Invalid_MultipleBody()
        {
            // Arrange
            using HttpClient httpClient = MockHttpClientFactory.Create(HttpStatusCode.OK, null);
            ITestService service = getService(httpClient);

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => service.MultipleBodyAsync(1, 2).Wait());
        }

        [Fact]
        public void Invalid_NotDecorated()
        {
            // Arrange
            using HttpClient httpClient = MockHttpClientFactory.Create(HttpStatusCode.OK, null);
            ITestService service = getService(httpClient);

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => service.NotDecoratedAsync().Wait());
        }

        [Fact]
        public void Invalid_NotAsync()
        {
            // Arrange
            using HttpClient httpClient = MockHttpClientFactory.Create(HttpStatusCode.OK, null);
            ITestService service = getService(httpClient);

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => service.NotAsync());
        }

        private static ITestService getService(HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri("https://example.com");
            JsonHttpClient<ITestService> client = new(httpClient);
            ITestService service = client.Endpoints;
            return service;
        }
    }
}
