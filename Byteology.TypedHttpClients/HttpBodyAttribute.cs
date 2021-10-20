using System;

namespace Byteology.TypedHttpClients
{
    /// <summary>
    /// Used to mark a parameter as the body of an HTTP request.
    /// </summary>
    /// <seealso cref="Attribute" />
    /// <seealso cref="TypedHttpClient{TServiceContract}" />
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class HttpBodyAttribute : Attribute { }
}
