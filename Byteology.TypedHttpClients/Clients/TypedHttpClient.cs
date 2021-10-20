using Byteology.GuardClauses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Byteology.TypedHttpClients
{
    /// <summary>
    /// Provides a base class for HTTP clients that implement a service contract.
    /// </summary>
    /// <typeparam name="TServiceContract">The service contract. It should be an interface containing only
    /// async methods decorated with <see cref="HttpEndpointAttribute"/> and having no output parameters.
    /// A single method parameter is allowed to be decorated with the <see cref="HttpBodyAttribute"/>
    /// in order to be used as the request's content. All other parameters will be used either as route or query parameters
    /// depending on whether their name has a match in the <see cref="HttpEndpointAttribute.RouteTemplate"/> or not.</typeparam>
    #pragma warning disable CS0618 // Type or member is obsolete
    public abstract class TypedHttpClient<TServiceContract> : IDispatchHandler
    #pragma warning restore CS0618 // Type or member is obsolete
        where TServiceContract : class
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Gets the endpoints of the service.
        /// </summary>
        public TServiceContract Endpoints { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypedHttpClient{TServiceContract}"/> 
        /// class using an <see cref="HttpClient"/>
        /// that will send the HTTP requests.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        protected TypedHttpClient(HttpClient httpClient)
        {
            Guard.Argument(httpClient, nameof(httpClient)).NotNull();

            _httpClient = httpClient;
            #pragma warning disable CS0618 // Type or member is obsolete
            Endpoints = DispatchProxyDelegator.Create<TServiceContract>(this);
            #pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// Builds a query string.
        /// </summary>
        /// <param name="queryParameters">The query parameters.</param>
        /// <param name="tags">The tags of the request. These are provided by the <see cref="HttpEndpointAttribute"/> 
        /// in order for this method to be able to recognize requests that require special treatment.</param>
        protected virtual string BuildQueryString(IEnumerable<HttpUriParameter> queryParameters, string[] tags)
        {
            StringBuilder builder = new();

            if (queryParameters != null)
                foreach (HttpUriParameter parameter in queryParameters)
                {
                    if (parameter.Value is ICollection collection)
                        foreach (object item in collection)
                            builder.Append($"{HttpUtility.UrlEncode(parameter.Name)}={HttpUtility.UrlEncode(item?.ToString())}&");
                    else
                        builder.Append($"{HttpUtility.UrlEncode(parameter.Name)}={HttpUtility.UrlEncode(parameter.Value?.ToString())}&");
                }

            if (builder.Length > 0)
                builder.Remove(builder.Length - 1, 1);

            return builder.ToString();
        }

        /// <summary>
        /// Builds an HTTP request.
        /// </summary>
        /// <param name="verb">The HTTP verb of the request.</param>
        /// <param name="uri">The URI of the request.</param>
        /// <param name="body">The content body of the request or <see langword="null"/> if no body should be provided.</param>
        /// <param name="tags">The tags of the request. These are provided by the <see cref="HttpEndpointAttribute"/> 
        /// in order for this method to be able to recognize requests that require special treatment.</param>
        protected abstract Task<HttpRequestMessage> BuildRequestAsync(string verb, string uri, object body, string[] tags);

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
        /// </summary>
        /// <param name="httpClient">The HTTP client to use.</param>
        /// <param name="request">The request to send.</param>
        /// <param name="tags">The tags of the request. These are specified by the <see cref="HttpEndpointAttribute"/> 
        /// in order for this method to be able to recognize requests that require special treatment.</param>
        protected virtual async Task<HttpResponseMessage> SendRequestAsync(HttpClient httpClient, HttpRequestMessage request, string[] tags)
        {
            Guard.Argument(httpClient, nameof(httpClient)).NotNull();
            Guard.Argument(request, nameof(request)).NotNull();

            HttpResponseMessage response = await httpClient.SendAsync(request).ConfigureAwait(false);
            response.RequestMessage = request;
            return response;
        }

        /// <summary>
        /// Processes the response of an HTTP request.
        /// </summary>
        /// <param name="response">The HTTP response.</param>
        /// <param name="tags">The tags of the request. These are specified by the <see cref="HttpEndpointAttribute"/> 
        /// in order for this method to be able to recognize requests that require special treatment.</param>
        protected abstract Task ProcessResponse(HttpResponseMessage response, string[] tags);
        /// <summary>
        /// Processes the response of an HTTP request and converts its body to <typeparamref name="TResult"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the object the response message should be converted to.</typeparam>
        /// <param name="response">The HTTP response.</param>
        /// <param name="tags">The tags of the request. These are specified by the <see cref="HttpEndpointAttribute"/> 
        /// in order for this method to be able to recognize requests that require special treatment.</param>
        protected abstract Task<TResult> ProcessResponse<TResult>(HttpResponseMessage response, string[] tags);

        object IDispatchHandler.Dispatch(MethodInfo targetMethod, object[] args)
        {
            if (targetMethod.GetCustomAttribute<HttpEndpointAttribute>(true) == null)
                throw new InvalidOperationException($"The method should be decorated with the {typeof(HttpEndpointAttribute)}.");

            ParameterInfo[] parameters = targetMethod.GetParameters();

            if (parameters.Any(p => p.IsOut))
                throw new InvalidOperationException("Output parameters are not allowed.");

            if (parameters.Count(p => p.GetCustomAttribute<HttpBodyAttribute>() != null) > 1)
                throw new InvalidOperationException("Maximum of one body parameter is allowed per method.");

            if (returnsTask(targetMethod))
                return sendRequestAndParseResponseAsync(targetMethod, args);
            else if (returnsGenericTask(targetMethod))
            {
                MethodInfo sendRequestMethod = typeof(TypedHttpClient<TServiceContract>)
                    .GetMethod(nameof(sendRequestAndParseGenericResponseAsync),BindingFlags.NonPublic | BindingFlags.Instance);
                sendRequestMethod = sendRequestMethod.MakeGenericMethod(targetMethod.ReturnType.GenericTypeArguments.Single());
                return sendRequestMethod.Invoke(this, new object[] { targetMethod, args });
            }
            else
                throw new InvalidOperationException("Method should return either Task or Task<>.");
        }

        private async Task sendRequestAndParseResponseAsync(MethodInfo targetMethod, object[] args)
        {
            HttpEndpointAttribute endpointAttribute = targetMethod.GetCustomAttribute<HttpEndpointAttribute>(true);
            string[] tags = endpointAttribute.Tags;

            HttpRequestMessage request = await createRequestAsync(targetMethod, args).ConfigureAwait(false);
            HttpResponseMessage response = await SendRequestAsync(_httpClient, request, tags).ConfigureAwait(false);

            await ProcessResponse(response, tags).ConfigureAwait(false);
        }
        private async Task<TResult> sendRequestAndParseGenericResponseAsync<TResult>(MethodInfo targetMethod, object[] args)
        {
            HttpEndpointAttribute endpointAttribute = targetMethod.GetCustomAttribute<HttpEndpointAttribute>(true);
            string[] tags = endpointAttribute.Tags;
            HttpRequestMessage request = await createRequestAsync(targetMethod, args).ConfigureAwait(false);
            HttpResponseMessage response = await SendRequestAsync(_httpClient, request, tags).ConfigureAwait(false);

            return await ProcessResponse<TResult>(response, tags).ConfigureAwait(false);
        }

        private async Task<HttpRequestMessage> createRequestAsync(MethodInfo targetMethod, object[] args)
        {
            HttpEndpointAttribute endpointAttribute = targetMethod.GetCustomAttribute<HttpEndpointAttribute>(true);

            string verb = endpointAttribute.Verb;
            string routeTemplate = endpointAttribute.RouteTemplate;
            string[] tags = endpointAttribute.Tags;

            List<HttpUriParameter> uriParameters = getParameters(targetMethod, args, out object body);
            string uri = getUri(tags, routeTemplate, ref uriParameters);

            HttpRequestMessage request = await BuildRequestAsync(verb, uri, body, tags).ConfigureAwait(false);
            return request;
        }
        private static List<HttpUriParameter> getParameters(MethodInfo targetMethod, object[] args, out object body)
        {
            List<HttpUriParameter> result = new();
            body = null;

            ParameterInfo[] parameters = targetMethod.GetParameters();

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter = parameters[i];
                object value = args[i];

                if (isBodyParameter(parameter))
                    body = value;
                else
                {
                    HttpUriParameter uriParameter = new(parameter.Name, value);
                    result.Add(uriParameter);
                }
            }

            return result;

            static bool isBodyParameter(ParameterInfo p)
            {
                HttpBodyAttribute bodyAttribute = p.GetCustomAttribute<HttpBodyAttribute>();
                return bodyAttribute != null;
            }
        }
        private string getUri(string[] tags, string routeTemplate, ref List<HttpUriParameter> uriParameters)
        {

            if (routeTemplate.StartsWith("/"))
                routeTemplate = routeTemplate[1..];

            for (int i = 0; i < uriParameters.Count; i++)
            {
                HttpUriParameter parameter = uriParameters[i];

                if (routeTemplate.Contains($"{{{parameter.Name}}}"))
                {
                    routeTemplate = routeTemplate.Replace(
                        $"{{{parameter.Name}}}", 
                        HttpUtility.UrlEncode(parameter.Value?.ToString() ?? string.Empty));

                    uriParameters.RemoveAt(i);
                    i--;
                }
            }

            routeTemplate = addQueryString(uriParameters, routeTemplate, tags);

            return routeTemplate;

            string addQueryString(List<HttpUriParameter> uriParameters, string uri, string[] tags)
            {
                string queryString = BuildQueryString(uriParameters, tags);
                if (!string.IsNullOrEmpty(queryString))
                {
                    if (!queryString.StartsWith("?"))
                        queryString = "?" + queryString;

                    uri += queryString;
                }

                return uri;
            }
        }

        private static bool returnsTask(MethodInfo method)
        {
            return method.ReturnType == typeof(Task);
        }
        private static bool returnsGenericTask(MethodInfo method)
        {
            return method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>);
        }
    }
}
