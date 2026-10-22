namespace ByteBard.AsyncAPI.Tests.Bindings.MQTT5
{
    using FluentAssertions;
    using ByteBard.AsyncAPI.Bindings.MQTT;
    using ByteBard.AsyncAPI.Bindings.MQTT5;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers;
    using NUnit.Framework;

    [System.Obsolete("The MQTT 5 server binding is deprecated.")]
    internal class MQTT5Bindings_Should : TestBase
    {
        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void ServerBinding_SerializesScalarZeroAndMinusOne(AsyncApiVersion version)
        {
            var binding = new MQTT5ServerBinding
            {
                SessionExpiryInterval = 0,
                BindingVersion = "0.1.0",
            };
            var actual = binding.SerializeAsYaml(version);
            actual.Should().Contain("sessionExpiryInterval: 0").And.Contain("bindingVersion: 0.1.0");

            binding.SessionExpiryInterval = -1;
            actual = binding.SerializeAsYaml(version);
            actual.Should().Contain("sessionExpiryInterval: -1");

            binding.SessionExpiryInterval = 4294967295L;
            binding.SerializeAsYaml(version).Should().Contain("sessionExpiryInterval: 4294967295");
        }

        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void ServerBinding_SerializesSchemaAndReference(AsyncApiVersion version)
        {
            var schemaBinding = new MQTT5ServerBinding
            {
                SessionExpiryInterval = new AsyncApiJsonSchema { Type = SchemaType.Integer },
            };
            schemaBinding.SerializeAsYaml(version).Should().Contain("type: integer");

            var referenceBinding = new MQTT5ServerBinding
            {
                SessionExpiryInterval = new AsyncApiJsonSchemaReference("#/components/schemas/interval"),
            };
            referenceBinding.SerializeAsYaml(version).Should().Contain("#/components/schemas/interval");
        }

        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void ServerBinding_ReadsAndRoundTripsSchemaAndReference(AsyncApiVersion version)
        {
            var parsers = ByteBard.AsyncAPI.Bindings.BindingsCollection.MQTT;
            parsers.Add(new MQTT5ServerBinding());
            var reader = new AsyncApiStringReader(new AsyncApiReaderSettings { Bindings = parsers });
            var yaml = (version == AsyncApiVersion.AsyncApi2_0 ? "url: localhost\nprotocol: mqtt\n" : "host: localhost\nprotocol: mqtt\n") + """
                bindings:
                  mqtt5:
                    sessionExpiryInterval:
                      $ref: '#/components/schemas/interval'
                """;

            var parsed = reader.ReadFragment<AsyncApiServer>(yaml, version, out var diagnostics);
            diagnostics.Errors.Should().BeEmpty();
            var binding = parsed.Bindings["mqtt5"] as MQTT5ServerBinding;
            binding.SessionExpiryInterval.Should().BeOfType<MQTTIntegerSchema>().Which.Schema.Should().BeOfType<AsyncApiJsonSchemaReference>();

            var serialized = parsed.SerializeAsYaml(version);
            serialized.Should().Contain("#/components/schemas/interval");
            var reparsed = reader.ReadFragment<AsyncApiServer>(serialized, version, out var roundTripDiagnostics);
            roundTripDiagnostics.Errors.Should().BeEmpty();
            (reparsed.Bindings["mqtt5"] as MQTT5ServerBinding).SessionExpiryInterval.Should().BeOfType<MQTTIntegerSchema>().Which.Schema.Should().BeOfType<AsyncApiJsonSchemaReference>();

            var schemaYaml = (version == AsyncApiVersion.AsyncApi2_0 ? "url: localhost\nprotocol: mqtt\n" : "host: localhost\nprotocol: mqtt\n") + """
                bindings:
                  mqtt5:
                    sessionExpiryInterval:
                      type: integer
                      minimum: 1
                """;
            var schemaParsed = reader.ReadFragment<AsyncApiServer>(schemaYaml, version, out var schemaDiagnostics);
            schemaDiagnostics.Errors.Should().BeEmpty();
            (schemaParsed.Bindings["mqtt5"] as MQTT5ServerBinding).SessionExpiryInterval.Should().BeOfType<MQTTIntegerSchema>().Which.Schema.Type.Should().Be(SchemaType.Integer);
            schemaParsed.SerializeAsYaml(version).Should().Contain("minimum: 1");
        }
    }
}
