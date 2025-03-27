namespace ByteBard.AsyncAPI.Bindings.Sns
{
    using System;
    using System.Collections.Generic;
    using ByteBard.AsyncAPI.Attributes;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Writers;

    public class Ordering : IAsyncApiExtensible
    {
        /// <summary>
        /// What type of SNS Topic is this?.
        /// </summary>
        public OrderingType Type { get; set; }

        /// <summary>
        /// True to turn on de-duplication of messages for a channel.
        /// </summary>
        public bool ContentBasedDeduplication { get; set; }

        public IDictionary<string, IAsyncApiExtension> Extensions { get; set; } = new Dictionary<string, IAsyncApiExtension>();

        public void Serialize(IAsyncApiWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteStartObject();
            writer.WriteRequiredProperty("type", this.Type.GetDisplayName());
            writer.WriteOptionalProperty("contentBasedDeduplication", this.ContentBasedDeduplication);
            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }
    }

    public enum OrderingType
    {
        [Display("standard")]
        Standard,
        [Display("FIFO")]
        Fifo,
    }
}