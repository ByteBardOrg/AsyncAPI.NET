// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LEGO.AsyncAPI.Extensions;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Writers;

    public class AsyncApiOperationReplyAddress : IAsyncApiSerializable, IAsyncApiExtensible
    {

        /// <summary>
        /// An optional description of the address. CommonMark syntax can be used for rich text representation.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// REQUIRED. A runtime expression that specifies the location of the reply address.
        /// </summary>
        public virtual string Location { get; set; }

        public virtual IDictionary<string, IAsyncApiExtension> Extensions { get; set; } = new Dictionary<string, IAsyncApiExtension>();

        public void SerializeV2(IAsyncApiWriter writer)
        {
            throw new NotImplementedException();
        }

        public virtual void SerializeV3(IAsyncApiWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteStartObject();
            writer.WriteOptionalProperty(AsyncApiConstants.Description, this.Description);
            writer.WriteRequiredProperty(AsyncApiConstants.Location, this.Location);
            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }
    }
}