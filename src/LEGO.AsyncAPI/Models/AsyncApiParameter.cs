// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Writers;

    /// <summary>
    /// Describes a parameter included in a channel name.
    /// </summary>
    public class AsyncApiParameter : IAsyncApiExtensible, IAsyncApiSerializable
    {
        /// <summary>
        /// An enumeration of string values to be used if the substitution options are from a limited set.
        /// </summary>
        public virtual IList<string> Enum { get; set; } = new List<string>();

        /// <summary>
        /// The default value to use for substitution, and to send, if an alternate value is not supplied.
        /// </summary>
        public virtual string Default { get; set; }
        /// <summary>
        /// An optional description for the parameter. CommonMark syntax MAY be used for rich text representation.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// An array of examples of the parameter value.
        /// </summary>
        public virtual IList<string> Examples { get; set; } = new List<string>();

        /// <summary>
        /// A runtime expression that specifies the location of the parameter value.
        /// </summary>
        public virtual string Location { get; set; }

        /// <inheritdoc/>
        public virtual IDictionary<string, IAsyncApiExtension> Extensions { get; set; } = new Dictionary<string, IAsyncApiExtension>();

        public virtual void SerializeV2(IAsyncApiWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteStartObject();
            writer.WriteOptionalProperty(AsyncApiConstants.Description, this.Description);
            var schema = new AsyncApiJsonSchema();
            if (this.Enum.Any())
            {
                schema.Enum = this.Enum.Select(e => new AsyncApiAny(e)).ToList();
            }
            if (this.Default != null)
            {
                schema.Default = new AsyncApiAny(this.Default);
            }
            if (this.Examples.Any())
            {
                schema.Examples = this.Examples.Select(e => new AsyncApiAny(e)).ToList();
            }

            writer.WriteOptionalObject(AsyncApiConstants.Schema, schema, (w, s) => s.SerializeV2(w));
            writer.WriteOptionalProperty(AsyncApiConstants.Location, this.Location);
            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }

        public virtual void SerializeV3(IAsyncApiWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteStartObject();
            writer.WriteOptionalCollection(AsyncApiConstants.Enum, this.Enum, (w, e) => w.WriteValue(e));
            writer.WriteOptionalProperty(AsyncApiConstants.Default, this.Default);
            writer.WriteOptionalProperty(AsyncApiConstants.Description, this.Description);
            writer.WriteOptionalCollection(AsyncApiConstants.Examples, this.Examples, (w, e) => w.WriteValue(e));
            writer.WriteOptionalProperty(AsyncApiConstants.Location, this.Location);
            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }
    }
}