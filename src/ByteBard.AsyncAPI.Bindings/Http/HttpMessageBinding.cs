namespace ByteBard.AsyncAPI.Bindings.Http
{
    using System;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;

    /// <summary>
    /// Binding class for http messaging channels.
    /// </summary>
    /// <remarks>
    /// The 'statusCode' field exists in AsyncAPI V3 but not in V2.
    /// </remarks>
    public class HttpMessageBinding : MessageBinding<HttpMessageBinding>
    {
        private const string V2BindingVersion = "0.2.0";
        private const string V3BindingVersion = "0.3.0";

        /// <summary>
        /// A Schema object containing the definitions for HTTP-specific headers. This schema MUST be of type object and have a properties key.
        /// </summary>
        public AsyncApiJsonSchema Headers { get; set; }

        /// <summary>
        /// The HTTP response status code according to RFC 9110. `statusCode` is only relevant for messages referenced by the Operation Reply Object.
        /// Stored as an integer so that codes without a <see cref="System.Net.HttpStatusCode"/> member (e.g. 425, 499, 520) are represented as-is.
        /// Note: This field is only serialized in AsyncAPI V3.
        /// </summary>
        public int? StatusCode { get; set; }

        public override string BindingKey => "http";

        protected override FixedFieldMap<HttpMessageBinding> FixedFieldMap => new()
        {
            { "bindingVersion", (a, n) => { a.BindingVersion = n.GetScalarValue(); } },
            { "headers", (a, n) => { a.Headers = AsyncApiJsonSchemaDeserializer.LoadSchema(n); } },
            { "statusCode", (a, n) => { a.StatusCode = LoadStatusCode(n); } },
        };

        /// <summary>
        /// Reads the status code, reporting values outside the RFC 9110 range (100-599) as errors.
        /// </summary>
        /// <remarks>An out-of-range value is still kept so the document can be written back unchanged.</remarks>
        private static int LoadStatusCode(ParseNode node)
        {
            var code = node.GetIntegerValue();
            if (code < 100 || code > 599)
            {
                node.Context.Diagnostic.Errors.Add(new AsyncApiError(
                    node.Context.GetLocation(),
                    $"statusCode {code} is not a valid HTTP status code. It must be between 100 and 599."));
            }

            return code;
        }

        public override void SerializeV2(IAsyncApiWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteStartObject();
            writer.WriteOptionalObject(AsyncApiConstants.Headers, this.Headers, (w, h) => h.SerializeV2(w));
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
            writer.WriteOptionalObject(AsyncApiConstants.Headers, this.Headers, (w, h) => h.SerializeV3(w));

            if (this.StatusCode.HasValue)
            {
                writer.WriteRequiredProperty(AsyncApiConstants.StatusCode, this.StatusCode.Value);
            }

            writer.WriteOptionalProperty(AsyncApiConstants.BindingVersion, this.BindingVersion ?? V3BindingVersion);
            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }

        public override void SerializeProperties(IAsyncApiWriter writer)
        {
            this.SerializeV3(writer);
        }
    }
}
