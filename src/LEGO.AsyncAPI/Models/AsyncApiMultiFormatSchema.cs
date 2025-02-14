// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Writers;

    public class AsyncApiMultiFormatSchema : IAsyncApiSerializable, IAsyncApiExtensible
    {
        /// <summary>
        /// Required. A string containing the name of the schema format that is used to define the information. If schemaFormat is missing, it MUST default to application/vnd.aai.asyncapi+json;version={{asyncapi}} where {{asyncapi}} matches the AsyncAPI Version String. In such a case, this would make the Multi Format Schema Object equivalent to the Schema Object. When using Reference Object within the schema, the schemaFormat of the resource being referenced MUST match the schemaFormat of the schema that contains the initial reference. For example, if you reference Avro schema, then schemaFormat of referencing resource and the resource being reference MUST match.
        /// </summary>
        public virtual string SchemaFormat { get; set; }

        /// <summary>
        /// Required. Definition of the message payload. It can be of any type but defaults to Schema Object. It MUST match the schema format defined in schemaFormat, including the encoding type. E.g., Avro should be inlined as either a YAML or JSON object instead of as a string to be parsed as YAML or JSON. Non-JSON-based schemas (e.g., Protobuf or XSD) MUST be inlined as a string.
        /// </summary>
        public virtual IAsyncApiSchema Schema { get; set; }

        public IDictionary<string, IAsyncApiExtension> Extensions { get; set; } = new Dictionary<string, IAsyncApiExtension>();

        public virtual void SerializeV2(IAsyncApiWriter writer)
        {
            this.Schema.SerializeV2(writer);
        }

        public virtual void SerializeV3(IAsyncApiWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            // Serialize without including the schema.
            if (this.SchemaFormat == "application/vnd.aai.asyncapi+json;version=3.0.0" && this.Schema is AsyncApiJsonSchema)
            {
                this.Schema.SerializeV3(writer);
                return;
            }

            writer.WriteStartObject();

            writer.WriteRequiredProperty(AsyncApiConstants.SchemaFormat, this.SchemaFormat);

            writer.WriteRequiredObject(AsyncApiConstants.Schema, this.Schema, (w, s) => s.SerializeV3(w));

            writer.WriteExtensions(this.Extensions);

            writer.WriteEndObject();
        }
    }
}