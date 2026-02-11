namespace ByteBard.AsyncAPI.Bindings.Http
{
    using System;
    using ByteBard.AsyncAPI.Attributes;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;

    /// <summary>
    /// Binding class for http operations.
    /// </summary>
    /// <remarks>
    /// The 'type' field exists in AsyncAPI V2 but is removed in V3 (inferred from operation action).
    /// </remarks>
    public class HttpOperationBinding : OperationBinding<HttpOperationBinding>
    {
        private const string V2BindingVersion = "0.2.0";
        private const string V3BindingVersion = "0.3.0";

        /// <summary>
        /// Represents the HTTP operation type (used in V2 serialization).
        /// </summary>
        public enum HttpOperationType
        {
            [Display("request")]
            Request,

            [Display("response")]
            Response,
        }

        /// <summary>
        /// The HTTP method, e.g. GET, POST, PUT, PATCH, DELETE, HEAD, OPTIONS, CONNECT, and TRACE.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// A Schema object containing the definitions for each query parameter. This schema MUST be of type object and have a properties key.
        /// </summary>
        public AsyncApiJsonSchema Query { get; set; }

        public override string BindingKey => "http";

        protected override FixedFieldMap<HttpOperationBinding> FixedFieldMap => new()
        {
            { "bindingVersion", (a, n) => { a.BindingVersion = n.GetScalarValue(); } },
            { "method", (a, n) => { a.Method = n.GetScalarValue(); } },
            { "query", (a, n) => { a.Query = AsyncApiJsonSchemaDeserializer.LoadSchema(n); } },
        };

        public override void SerializeV2(IAsyncApiWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteStartObject();

            var typeValue = this.InferTypeFromContext(writer);
            if (typeValue.HasValue)
            {
                writer.WriteRequiredProperty(AsyncApiConstants.Type, typeValue.GetDisplayName());
            }

            writer.WriteOptionalProperty(AsyncApiConstants.Method, this.Method);
            writer.WriteOptionalObject(AsyncApiConstants.Query, this.Query, (w, h) => h.SerializeV2(w));
            writer.WriteOptionalProperty(AsyncApiConstants.BindingVersion, this.BindingVersion ?? V2BindingVersion);
            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }

        public override void SerializeV3(IAsyncApiWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteStartObject();
            writer.WriteOptionalProperty(AsyncApiConstants.Method, this.Method);
            writer.WriteOptionalObject(AsyncApiConstants.Query, this.Query, (w, h) => h.SerializeV3(w));
            writer.WriteOptionalProperty(AsyncApiConstants.BindingVersion, this.BindingVersion ?? V3BindingVersion);
            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }

        public override void SerializeProperties(IAsyncApiWriter writer)
        {
            this.SerializeV3(writer);
        }

        private HttpOperationType? InferTypeFromContext(IAsyncApiWriter writer)
        {
            var parentOperation = writer.Workspace?.GetSerializationContext<AsyncApiOperation>();
            if (parentOperation == null)
            {
                return null;
            }

            return parentOperation.Action == AsyncApiAction.Send
                ? HttpOperationType.Request
                : HttpOperationType.Response;
        }
    }
}
