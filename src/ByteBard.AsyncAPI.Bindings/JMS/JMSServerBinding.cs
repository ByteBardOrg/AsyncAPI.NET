namespace ByteBard.AsyncAPI.Bindings.JMS
{
    using System;
    using System.Collections.Generic;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;

    /// <summary>Binding for a JMS server connection.</summary>
    public class JMSServerBinding : ServerBinding<JMSServerBinding>
    {
        public string JmsConnectionFactory { get; set; }

        public IList<JMSProperty> Properties { get; set; }

        public string ClientID { get; set; }

        protected override FixedFieldMap<JMSServerBinding> FixedFieldMap => new()
        {
            { "jmsConnectionFactory", (a, n) => { a.JmsConnectionFactory = n.GetScalarValue(); } },
            { "properties", (a, n) => { a.Properties = n.CreateList(node => node.ParseMap(JMSPropertyFixedFields)); } },
            { "clientID", (a, n) => { a.ClientID = n.GetScalarValue(); } },
            { "bindingVersion", (a, n) => { a.BindingVersion = n.GetScalarValue(); } },
        };

        private static readonly FixedFieldMap<JMSProperty> JMSPropertyFixedFields = new()
        {
            { "name", (a, n) => { a.Name = n.GetScalarValue(); } },
            { "value", (a, n) => { a.Value = n.CreateAny(); } },
        };

        public override string BindingKey => "jms";

        public override void SerializeV2(IAsyncApiWriter writer) => this.SerializeV3(writer);

        public override void SerializeV3(IAsyncApiWriter writer) => this.SerializeProperties(writer);

        public override void SerializeProperties(IAsyncApiWriter writer)
        {
            if (writer is null) throw new ArgumentNullException(nameof(writer));
            writer.WriteStartObject();
            writer.WriteRequiredProperty("jmsConnectionFactory", this.JmsConnectionFactory);
            writer.WriteOptionalCollection("properties", this.Properties, (w, p) => p.Serialize(w));
            writer.WriteOptionalProperty("clientID", this.ClientID);
            writer.WriteOptionalProperty(AsyncApiConstants.BindingVersion, this.BindingVersion);
            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }
    }
}
