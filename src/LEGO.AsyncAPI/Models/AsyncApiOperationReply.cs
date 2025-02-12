// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LEGO.AsyncAPI.Extensions;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Writers;

    public class AsyncApiOperationReply : IAsyncApiSerializable, IAsyncApiExtensible
    {
        public virtual AsyncApiOperationReplyAddress Address { get; set; }

        public virtual AsyncApiChannelReference Channel { get; set; }

        public virtual IList<AsyncApiMessageReference> Messages { get; set; } = new List<AsyncApiMessageReference>();

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
            writer.WriteOptionalObject(AsyncApiConstants.Address, this.Address, (w, a) => a.SerializeV3(w));
            writer.WriteOptionalProperty(AsyncApiConstants.Channel, this.Channel.SerializeAsDollarRef());
            writer.WriteOptionalCollection(AsyncApiConstants.Messages, this.Messages, (w, m) => m.SerializeAsDollarRef());
            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }
    }
}