namespace ByteBard.AsyncAPI.Tests.Bindings.MQTT
{
    using ByteBard.AsyncAPI.Bindings;
    using ByteBard.AsyncAPI.Bindings.MQTT;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers;
    using FluentAssertions;
    using NUnit.Framework;

    public class MQTTBindingVariants_Should
    {
        [Test]
        public void V2_SchemaAndReferenceVariants_AndBindingVersions_AreParsedAndSerialized()
        {
            var settings = new AsyncApiReaderSettings { Bindings = BindingsCollection.MQTT };
            var serverYaml =
                """
                bindings:
                  mqtt:
                    sessionExpiryInterval:
                      type: integer
                      minimum: 1
                    maximumPacketSize:
                      $ref: '#/components/schemas/PacketSize'
                    bindingVersion: 0.2.0
                """;
            serverYaml = "url: localhost\nprotocol: mqtt\n" + serverYaml;
            var server = new AsyncApiStringReader(settings).ReadFragment<AsyncApiServer>(serverYaml, AsyncApiVersion.AsyncApi2_0, out var serverDiagnostics);
            serverDiagnostics.Errors.Should().BeEmpty();
            var serverBinding = server.Bindings["mqtt"] as MQTTServerBinding;

            serverBinding.BindingVersion.Should().Be("0.2.0");
            serverBinding.SessionExpiryInterval.Should().BeOfType<MQTTIntegerSchema>().Which.Schema.Type.Should().Be(SchemaType.Integer);
            serverBinding.MaximumPacketSize.Should().BeOfType<MQTTIntegerSchema>().Which.Schema.Should().BeOfType<AsyncApiJsonSchemaReference>();
            server.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0).Should().Contain("minimum: 1").And.Contain("$ref: '#/components/schemas/PacketSize'");

            var operationYaml =
                """
                bindings:
                  mqtt:
                    qos: 1
                    retain: false
                    messageExpiryInterval:
                      $ref: '#/components/schemas/Expiry'
                    bindingVersion: 0.2.0
                """;
            var operation = new AsyncApiStringReader(settings).ReadFragment<AsyncApiOperation>(operationYaml, AsyncApiVersion.AsyncApi2_0, out _);
            var operationBinding = operation.Bindings["mqtt"] as MQTTOperationBinding;
            operationBinding.BindingVersion.Should().Be("0.2.0");
            operationBinding.MessageExpiryInterval.Should().BeOfType<MQTTIntegerSchema>().Which.Schema.Should().BeOfType<AsyncApiJsonSchemaReference>();

            var messageYaml =
                """
                bindings:
                  mqtt:
                    responseTopic:
                      type: string
                      pattern: '^reply/'
                    bindingVersion: 0.2.0
                """;
            var message = new AsyncApiStringReader(settings).ReadFragment<AsyncApiMessage>(messageYaml, AsyncApiVersion.AsyncApi2_0, out _);
            var messageBinding = message.Bindings["mqtt"] as MQTTMessageBinding;
            messageBinding.BindingVersion.Should().Be("0.2.0");
            messageBinding.ResponseTopic.Should().BeOfType<MQTTStringSchema>().Which.Schema.Type.Should().Be(SchemaType.String);
            message.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0).Should().Contain("pattern: ^reply/");
        }

        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void ServerSchemaOnlyBinding_RoundTripsWithoutDiagnostics(AsyncApiVersion version)
        {
            var settings = new AsyncApiReaderSettings { Bindings = BindingsCollection.MQTT };
            var yaml = (version == AsyncApiVersion.AsyncApi2_0 ? "url: localhost\nprotocol: mqtt\n" : "host: localhost\nprotocol: mqtt\n") + """
                bindings:
                  mqtt:
                    sessionExpiryInterval:
                      type: integer
                """;

            var reader = new AsyncApiStringReader(settings);
            var first = reader.ReadFragment<AsyncApiServer>(yaml, version, out var firstDiagnostics);
            firstDiagnostics.Errors.Should().BeEmpty();
            first.SerializeAsYaml(version).Should().NotContain("clientId: null");

            var second = reader.ReadFragment<AsyncApiServer>(first.SerializeAsYaml(version), version, out var secondDiagnostics);
            secondDiagnostics.Errors.Should().BeEmpty();
            (second.Bindings["mqtt"] as MQTTServerBinding).SessionExpiryInterval.Should().BeOfType<MQTTIntegerSchema>().Which.Schema.Type.Should().Be(SchemaType.Integer);
        }

        [Test]
        public void IntegerUnion_PreservesUnsigned32BitMaximum()
        {
            var binding = new MQTTServerBinding { SessionExpiryInterval = 4294967295L };
            var yaml = binding.SerializeAsYaml(AsyncApiVersion.AsyncApi3_0);

            yaml.Should().Contain("sessionExpiryInterval: 4294967295");
            var parsedYaml = "host: localhost\nprotocol: mqtt\n" + """
                    bindings:
                      mqtt:
                        sessionExpiryInterval: 4294967295
                    """;
            var parsed = new AsyncApiStringReader(new AsyncApiReaderSettings { Bindings = BindingsCollection.MQTT })
                .ReadFragment<AsyncApiServer>(parsedYaml, AsyncApiVersion.AsyncApi3_0, out var diagnostics);
            diagnostics.Errors.Should().BeEmpty();
            (parsed.Bindings["mqtt"] as MQTTServerBinding).SessionExpiryInterval.Should().BeOfType<MQTTInteger>().Which.Value.Should().Be(4294967295L);
        }

        [Test]
        public void Unions_ConvertImplicitlyToAndFromTheirAlternatives()
        {
            var schema = new AsyncApiJsonSchema { Type = SchemaType.Integer };
            var server = new MQTTServerBinding { SessionExpiryInterval = 600, MaximumPacketSize = schema };
            var message = new MQTTMessageBinding { ResponseTopic = "reply/topic" };

            long interval = server.SessionExpiryInterval.Should().BeOfType<MQTTInteger>().Subject;
            AsyncApiJsonSchema packetSize = server.MaximumPacketSize.Should().BeOfType<MQTTIntegerSchema>().Subject;
            string topic = message.ResponseTopic.Should().BeOfType<MQTTString>().Subject;

            interval.Should().Be(600);
            packetSize.Should().BeSameAs(schema);
            topic.Should().Be("reply/topic");

            message.ResponseTopic = (string)null;
            message.ResponseTopic.Should().BeNull();
        }

        [Test]
        public void OperationBinding_OmitsUnsetQosAndRetain()
        {
            var yaml = new MQTTOperationBinding().SerializeAsYaml(AsyncApiVersion.AsyncApi3_0);
            yaml.Should().NotContain("qos:").And.NotContain("retain:");
        }

        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void LastWill_WithoutTopicAndRetain_RoundTripsWithoutAddingThem(AsyncApiVersion version)
        {
            var settings = new AsyncApiReaderSettings { Bindings = BindingsCollection.MQTT };
            var yaml = (version == AsyncApiVersion.AsyncApi2_0 ? "url" : "host") + ": localhost\n" +
                """
                protocol: mqtt
                bindings:
                  mqtt:
                    lastWill:
                      qos: 1
                """;

            var server = new AsyncApiStringReader(settings).ReadFragment<AsyncApiServer>(yaml, version, out var diagnostics);
            diagnostics.Errors.Should().BeEmpty();

            var actual = server.SerializeAsYaml(version);
            actual.Should().Contain("qos: 1").And.NotContain("topic:").And.NotContain("retain:");
        }

        [Test]
        public void LastWill_WithExplicitRetainFalse_WritesRetain()
        {
            var lastWill = new LastWill { Topic = "/last-wills", Retain = false };
            var server = new MQTTServerBinding { LastWill = lastWill };

            server.SerializeAsYaml(AsyncApiVersion.AsyncApi3_0).Should().Contain("topic: /last-wills").And.Contain("retain: false");
        }
    }
}
