namespace ByteBard.AsyncAPI.Tests.Bindings.AnypointMQ
{
    using ByteBard.AsyncAPI.Bindings.AnypointMQ;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers;
    using FluentAssertions;
    using NUnit.Framework;

    internal class AnypointMQBindings_Should : TestBase
    {
        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void ChannelBinding_RoundTripsNestedPropertiesAndOmittedVersion(AsyncApiVersion version)
        {
            var original = new AsyncApiChannel();
            original.Bindings.Add(new AnypointMQChannelBinding { Destination = "orders", DestinationType = "queue" });

            var yaml = original.SerializeAsYaml(version);
            yaml.Should().Contain("destination: orders").And.Contain("destinationType: queue").And.NotContain("bindingVersion");
            var settings = new AsyncApiReaderSettings { Bindings = new[] { new AnypointMQChannelBinding() } };
            var parsed = new AsyncApiStringReader(settings).ReadFragment<AsyncApiChannel>(yaml, version, out _);

            parsed.Bindings["anypointmq"].Should().BeEquivalentTo(original.Bindings["anypointmq"]);
        }

        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void MessageBinding_RoundTripsHeadersSchema(AsyncApiVersion version)
        {
            var original = new AsyncApiMessage();
            original.Bindings.Add(new AnypointMQMessageBinding
            {
                BindingVersion = "0.1.0",
                Headers = new AsyncApiJsonSchema { Type = SchemaType.Object, Properties = { ["traceId"] = new AsyncApiJsonSchema { Type = SchemaType.String } } },
            });

            var yaml = original.SerializeAsYaml(version);
            yaml.Should().Contain("headers:").And.Contain("traceId:").And.Contain("bindingVersion: 0.1.0");
            var settings = new AsyncApiReaderSettings { Bindings = new[] { new AnypointMQMessageBinding() } };
            var parsed = new AsyncApiStringReader(settings).ReadFragment<AsyncApiMessage>(yaml, version, out _);

            parsed.Bindings["anypointmq"].Should().BeEquivalentTo(original.Bindings["anypointmq"]);
        }
    }
}
