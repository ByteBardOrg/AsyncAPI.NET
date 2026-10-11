namespace ByteBard.AsyncAPI.Bindings.GooglePubSub
{
    using System;
    using System.Collections.Generic;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;

    public class GooglePubSubChannelBinding : ChannelBinding<GooglePubSubChannelBinding>
    {
        public Dictionary<string, string> Labels { get; set; }
        public string MessageRetentionDuration { get; set; }
        public MessageStoragePolicy MessageStoragePolicy { get; set; }
        public SchemaSettings SchemaSettings { get; set; }

        public override string BindingKey => "googlepubsub";

        protected override FixedFieldMap<GooglePubSubChannelBinding> FixedFieldMap => new()
        {
            { "bindingVersion", (a, n) => { a.BindingVersion = n.GetScalarValue(); } },
            { "labels", (a, n) => { a.Labels = n.CreateSimpleMap(s => s.GetScalarValue()); } },
            { "messageRetentionDuration", (a, n) => { a.MessageRetentionDuration = n.GetScalarValue(); } },
            { "messageStoragePolicy", (a, n) => { a.MessageStoragePolicy = n.ParseMap(messageStoragePolicyFields); } },
            { "schemaSettings", (a, n) => { a.SchemaSettings = n.ParseMap(schemaSettingsFields); } },
        };

        private static readonly FixedFieldMap<MessageStoragePolicy> messageStoragePolicyFields = new()
        {
            { "allowedPersistenceRegions", (a, n) => { a.AllowedPersistenceRegions = n.CreateSimpleList(s => s.GetScalarValue()); } },
        };

        private static readonly FixedFieldMap<SchemaSettings> schemaSettingsFields = new()
        {
            { "encoding", (a, n) => { a.Encoding = n.GetScalarValue(); } },
            { "firstRevisionId", (a, n) => { a.FirstRevisionId = n.GetScalarValue(); } },
            { "lastRevisionId", (a, n) => { a.LastRevisionId = n.GetScalarValue(); } },
            { "name", (a, n) => { a.Name = n.GetScalarValue(); } },
        };

        public override void SerializeV2(IAsyncApiWriter writer) => this.SerializeV3(writer);
        public override void SerializeV3(IAsyncApiWriter writer) => this.SerializeProperties(writer);

        public override void SerializeProperties(IAsyncApiWriter writer)
        {
            if (writer is null) throw new ArgumentNullException(nameof(writer));
            writer.WriteStartObject();
            writer.WriteOptionalMap("labels", this.Labels, (w, v) => w.WriteValue(v));
            writer.WriteOptionalProperty("messageRetentionDuration", this.MessageRetentionDuration);
            writer.WriteOptionalObject("messageStoragePolicy", this.MessageStoragePolicy, (w, v) => v.Serialize(w));
            writer.WriteOptionalObject("schemaSettings", this.SchemaSettings, (w, v) => v.Serialize(w));
            writer.WriteOptionalProperty("bindingVersion", this.BindingVersion);
            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }
    }
}
