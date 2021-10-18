namespace Byteology.TypedHttpClients
{
    /// <summary>
    /// Represents a URI parameter.
    /// </summary>
    public class HttpUriParameter
    {
        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Gets the value of the parameter.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpUriParameter"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        public HttpUriParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }
}
