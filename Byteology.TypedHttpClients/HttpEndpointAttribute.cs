using Byteology.GuardClauses;
using System;

namespace Byteology.TypedHttpClients
{
    /// <summary>
    /// Used to mark a method's signature as a description of an HTTP endpoint.
    /// See <see cref="TypedHttpClient{TServiceContract}"/> for more information.
    /// </summary>
    /// <seealso cref="TypedHttpClient{TServiceContract}"/>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HttpEndpointAttribute : Attribute
    {
        /// <summary>
        /// Gets the endpoint route. May contain parameter names surrounded by curly brackets
        /// which will be replaced by the passed argument.
        /// </summary>
        public string RouteTemplate { get; }

        /// <summary>
        /// Gets the HTTP verb of the endpoint.
        /// </summary>
        public string Verb { get; }

        /// <summary>
        /// Gets or sets tags for the endpoint. These tags will be 
        /// passed to all methods connected to building HTTP requests and parsing their responses 
        /// and can be used to distinguish the endpoints which must be treated differently.
        /// </summary>
        public string[] Tags { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpEndpointAttribute"/> class.
        /// It is used to mark a method's signature as a description of an HTTP endpoint.
        /// See <see cref="TypedHttpClient{TServiceContract}"/> for more information.
        /// </summary>
        /// <param name="verb">The HTTP verb of the endpoint.</param>
        /// <param name="routeTemplate">The endpoint route. 
        /// May contain parameter names surrounded by curly brackets which will be replaced by the passed argument.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <seealso cref="TypedHttpClient{TServiceContract}" />
        public HttpEndpointAttribute(string verb, string routeTemplate)
        {
            Guard.Argument(verb, nameof(verb)).NotNullOrWhiteSpace();
            Guard.Argument(routeTemplate, nameof(routeTemplate)).NotNull();

            Verb = verb;
            RouteTemplate = routeTemplate;
        }
    }
}
