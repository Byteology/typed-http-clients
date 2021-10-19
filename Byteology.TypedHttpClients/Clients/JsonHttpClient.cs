using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Byteology.TypedHttpClients
{
    /// <summary>
    /// An HTTP clients that implement a contract of a service with a JSON content type.
    /// </summary>
    /// <typeparam name="TServiceContract"><inheritdoc/></typeparam>
    public class JsonHttpClient<TServiceContract> : TypedHttpClient<TServiceContract>
        where TServiceContract : class
    {
        /// <summary>
        /// Gets the JSON serialization options used to create and parse the HTTP requests and responses.
        /// </summary>
        public JsonSerializerOptions JsonSerializerOptions { get; } = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonHttpClient{TServiceContract}"/> class using an <see cref="HttpClient"/>
        /// that will send the HTTP requests.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        public JsonHttpClient(HttpClient httpClient) : base(httpClient) { }

        /// <summary>
        /// Builds an HTTP request with a JSON content type.
        /// </summary>
        /// <param name="verb"><inheritdoc path="/param[@name='verb']"/></param>
        /// <param name="uri"><inheritdoc path="/param[@name='uri']"/></param>
        /// <param name="body"><inheritdoc path="/param[@name='body']"/></param>
        /// <param name="tags"><inheritdoc path="/param[@name='tags']"/></param>
        protected override Task<HttpRequestMessage> BuildRequestAsync(string verb, string uri, object body, string[] tags)
        {
            HttpRequestMessage httpRequest = new(new HttpMethod(verb), uri);
            httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (body != null)
                httpRequest.Content = new StringContent(JsonSerializer.Serialize(body, JsonSerializerOptions),
                                                        Encoding.UTF8,
                                                        "application/json");

            return Task.FromResult(httpRequest);
        }

        /// <summary>
        /// Throws an exception if the <see cref="HttpResponseMessage.IsSuccessStatusCode"/>
        /// property for the HTTP response is false.
        /// </summary>
        /// <param name="response"><inheritdoc path="/param[@name='response']"/></param>
        /// <param name="tags"><inheritdoc path="/param[@name='tags']"/></param>
        /// <exception cref="HttpRequestException"/>
        protected override Task ProcessResponse(HttpResponseMessage response, string[] tags)
        {
            response.EnsureSuccessStatusCode();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Throws an exception if the <see cref="HttpResponseMessage.IsSuccessStatusCode"/>
        /// property for the HTTP response is false. Otherwise converts the response content
        /// to a <typeparamref name="TResult"/> object and returns it.
        /// </summary>
        /// <param name="response"><inheritdoc path="/param[@name='response']"/></param>
        /// <param name="tags"><inheritdoc path="/param[@name='tags']"/></param>
        /// <exception cref="HttpRequestException"/>
        protected override async Task<TResult> ProcessResponse<TResult>(HttpResponseMessage response, string[] tags)
        {
            response.EnsureSuccessStatusCode();

            string bodyString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            TResult result = JsonSerializer.Deserialize<TResult>(bodyString, JsonSerializerOptions);

            return result;
        }
    }
}
