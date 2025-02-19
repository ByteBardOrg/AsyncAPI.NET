// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Writers;

    /// <summary>
    /// Allows referencing an external resource for extended documentation.
    /// </summary>
    public class AsyncApiExternalDocumentation : IAsyncApiExtensible, IAsyncApiSerializable
    {
        /// <summary>
        /// a short description of the target documentation. CommonMark syntax can be used for rich text representation.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// REQUIRED. The URL for the target documentation. Value MUST be in the format of a URL.
        /// </summary>
        public virtual Uri Url { get; set; }

        /// <inheritdoc/>
        public virtual IDictionary<string, IAsyncApiExtension> Extensions { get; set; } = new Dictionary<string, IAsyncApiExtension>();

        public virtual void SerializeV2(IAsyncApiWriter writer)
        {
            this.SerializeCore(writer);
        }

        public virtual void SerializeV3(IAsyncApiWriter writer)
        {
            this.SerializeCore(writer);
        }

        private void SerializeCore(IAsyncApiWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteStartObject();

            writer.WriteOptionalProperty(AsyncApiConstants.Description, this.Description);

            writer.WriteRequiredProperty(AsyncApiConstants.Url, this.Url?.OriginalString);

            writer.WriteExtensions(this.Extensions);

            writer.WriteEndObject();
        }
    }
}