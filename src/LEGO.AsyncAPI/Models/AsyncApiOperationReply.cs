// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
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
            writer.WriteOptionalObject(AsyncApiConstants.Channel, this.Channel, (w, c) => c.Reference.SerializeV3(w));
            writer.WriteOptionalCollection(AsyncApiConstants.Messages, this.Messages, (w, m) => m.Reference.SerializeV3(w));
            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }
    }
}