using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Byteology.TypedHttpClients.Tests.Mocks
{
	internal static class MockHttpClientFactory
	{
		public static HttpClient Create(HttpStatusCode statusCode, HttpContent? content,
										Action<HttpRequestMessage>? onBeforeSend = null)
		{
			MessageHandler handler = new(statusCode, content, onBeforeSend);
			return new HttpClient(handler, true);
		}

		private class MessageHandler : HttpMessageHandler
		{
			private readonly HttpContent? _content;
			private readonly Action<HttpRequestMessage>? _onBeforeSend;
			private readonly HttpStatusCode _statusCode;

			public MessageHandler(HttpStatusCode statusCode, HttpContent? content,
								  Action<HttpRequestMessage>? onBeforeSend = null)
			{
				_statusCode = statusCode;
				_content = content;
				_onBeforeSend = onBeforeSend;
			}

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
																   CancellationToken cancellationToken)
			{
				_onBeforeSend?.Invoke(request);

				return Task.FromResult(new HttpResponseMessage
				{
					StatusCode = _statusCode,
					Content = _content
				});
			}
		}
	}
}
