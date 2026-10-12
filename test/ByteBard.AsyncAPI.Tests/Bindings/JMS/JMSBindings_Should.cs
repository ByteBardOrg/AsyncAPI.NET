namespace ByteBard.AsyncAPI.Tests.Bindings.JMS
{
    using ByteBard.AsyncAPI.Bindings.JMS;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers;
    using FluentAssertions;
    using NUnit.Framework;

    internal class JMSBindings_Should : TestBase
    {
        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void ServerBinding_RoundTripsNestedTypedPropertiesAndOmittedVersion(AsyncApiVersion version)
        {
            var original = new AsyncApiServer { Host = "localhost", Protocol = "jms" };
            original.Bindings.Add(new JMSServerBinding
            {
                JmsConnectionFactory = "ConnectionFactory",
                ClientID = "worker",
                Properties = new[]
                {
                    new JMSProperty { Name = "enabled", Value = new AsyncApiAny(false) },
                    new JMSProperty { Name = "priority", Value = new AsyncApiAny(0) },
                    new JMSProperty { Name = "region", Value = new AsyncApiAny("west") },
                },
            });

            var yaml = original.SerializeAsYaml(version);
            yaml.Should().Contain("jmsConnectionFactory: ConnectionFactory")
                .And.Contain("clientID: worker").And.Contain("value: false").And.Contain("value: 0")
                .And.NotContain("bindingVersion");
            var settings = new AsyncApiReaderSettings { Bindings = new[] { new JMSServerBinding() } };
            var parsed = new AsyncApiStringReader(settings).ReadFragment<AsyncApiServer>(yaml, version, out _);

            parsed.Bindings["jms"].Should().BeEquivalentTo(original.Bindings["jms"]);
        }

        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void ChannelAndMessageBindings_RoundTrip(AsyncApiVersion version)
        {
            var channel = new AsyncApiChannel();
            channel.Bindings.Add(new JMSChannelBinding { Destination = "orders", DestinationType = "queue" });
            var channelYaml = channel.SerializeAsYaml(version);
            var channelSettings = new AsyncApiReaderSettings { Bindings = new[] { new JMSChannelBinding() } };
            var parsedChannel = new AsyncApiStringReader(channelSettings).ReadFragment<AsyncApiChannel>(channelYaml, version, out _);
            parsedChannel.Bindings["jms"].Should().BeEquivalentTo(channel.Bindings["jms"]);

            var message = new AsyncApiMessage();
            message.Bindings.Add(new JMSMessageBinding
            {
                Headers = new AsyncApiJsonSchema { Type = SchemaType.Object, Properties = { ["correlationId"] = new AsyncApiJsonSchema { Type = SchemaType.String } } },
            });
            var messageYaml = message.SerializeAsYaml(version);
            var messageSettings = new AsyncApiReaderSettings { Bindings = new[] { new JMSMessageBinding() } };
            var parsedMessage = new AsyncApiStringReader(messageSettings).ReadFragment<AsyncApiMessage>(messageYaml, version, out _);
            parsedMessage.Bindings["jms"].Should().BeEquivalentTo(message.Bindings["jms"]);
        }
    }
}
