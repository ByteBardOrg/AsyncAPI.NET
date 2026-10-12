namespace ByteBard.AsyncAPI.Tests.Bindings.Solace
{
    using System.Collections.Generic;
    using ByteBard.AsyncAPI.Bindings;
    using ByteBard.AsyncAPI.Bindings.Solace;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers;
    using ByteBard.AsyncAPI.Readers.Interface;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using NUnit.Framework;
    using FluentAssertions;

    public class SolaceBindings_Should
    {
        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void OperationBinding_RoundTripsNestedDestinationsAndScalarUnionValues(AsyncApiVersion version)
        {
            var operation = new AsyncApiOperation();
            operation.Bindings.Add(new SolaceOperationBinding
            {
                DmqEligible = false,
                TimeToLive = new SolaceIntegerOrSchema { Integer = 0 },
                Priority = new SolaceIntegerOrSchema { Integer = 7 },
                Destinations =
                {
                    new SolaceDestination
                    {
                        DestinationType = "queue",
                        DeliveryMode = "persistent",
                        Queue = new SolaceQueue
                        {
                            Name = "orders",
                            AccessType = "exclusive",
                            MaxMsgSpoolSize = "100",
                            MaxTtl = "3600",
                            TopicSubscriptions = { "orders/>" },
                        },
                    },
                },
            });

            var yaml = operation.SerializeAsYaml(version);
            yaml.Should().Contain("dmqEligible: false").And.Contain("timeToLive: 0").And.Contain("maxTtl:");

            var settings = new AsyncApiReaderSettings();
            settings.Bindings = new List<IBindingParser<IBinding>> { new SolaceOperationBinding() };
            var parsed = new AsyncApiStringReader(settings).ReadFragment<AsyncApiOperation>(yaml, version, out _);
            parsed.Should().BeEquivalentTo(operation);
        }

        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void OperationBinding_RoundTripsSchemaAndReferenceUnionValues(AsyncApiVersion version)
        {
            var operation = new AsyncApiOperation
            {
                Action = AsyncApiAction.Send,
                Channel = new AsyncApiChannelReference("#/channels/orders"),
            };
            operation.Bindings.Add(new SolaceOperationBinding
            {
                TimeToLive = new SolaceIntegerOrSchema
                {
                    Schema = new AsyncApiJsonSchema { Description = "ttl schema" },
                },
                Priority = new SolaceIntegerOrSchema
                {
                    Schema = new AsyncApiJsonSchemaReference("#/components/schemas/Priority"),
                },
            });

            var document = new AsyncApiDocument
            {
                Info = new AsyncApiInfo { Title = "Solace union fixture", Version = "1.0.0" },
            };
            document.Channels.Add("orders", new AsyncApiChannel { Address = "orders" });
            document.Operations.Add("solaceOperation", operation);
            document.Components.Schemas.Add("Priority", new AsyncApiMultiFormatSchema
            {
                Schema = new AsyncApiJsonSchema { Type = SchemaType.Integer },
            });

            var yaml = document.SerializeAsYaml(version);
            yaml.Should().Contain("description: ttl schema").And.Contain("#/components/schemas/Priority");

            var settings = new AsyncApiReaderSettings();
            settings.Bindings = new List<IBindingParser<IBinding>> { new SolaceOperationBinding() };
            var reader = new AsyncApiStringReader(settings);
            var parsed = reader.Read(yaml, out var diagnostics);
            diagnostics.Errors.Should().BeEmpty();
            parsed.Operations.Should().ContainSingle();

            var rewritten = parsed.SerializeAsYaml(version);
            var reparsed = reader.Read(rewritten, out diagnostics);
            diagnostics.Errors.Should().BeEmpty();
            var binding = reparsed.Operations.Values.Should().ContainSingle().Which.Bindings.Values.Should()
                .ContainSingle().Which.Should().BeOfType<SolaceOperationBinding>().Subject;
            binding.TimeToLive.Schema.Description.Should().Be("ttl schema");
            ((AsyncApiJsonSchemaReference)binding.Priority.Schema).Reference.Reference.Should().Be("#/components/schemas/Priority");
        }

        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void OperationBinding_RoundTripsLargeTimeToLive(AsyncApiVersion version)
        {
            const long largeTtl = 2592000000L;
            var operation = new AsyncApiOperation();
            operation.Bindings.Add(new SolaceOperationBinding
            {
                TimeToLive = new SolaceIntegerOrSchema { Integer = largeTtl },
            });

            var yaml = operation.SerializeAsYaml(version);
            yaml.Should().Contain("timeToLive: 2592000000");

            var settings = new AsyncApiReaderSettings();
            settings.Bindings = new List<IBindingParser<IBinding>> { new SolaceOperationBinding() };
            var parsed = new AsyncApiStringReader(settings).ReadFragment<AsyncApiOperation>(yaml, version, out _);
            var binding = parsed.Bindings.Should().ContainSingle().Which.Value.Should().BeOfType<SolaceOperationBinding>().Subject;
            binding.TimeToLive.Integer.Should().Be(largeTtl);
        }

        [Test]
        public void OperationBinding_RejectsEmptyIntegerOrSchemaBeforeWriting()
        {
            var operation = new AsyncApiOperation();
            operation.Bindings.Add(new SolaceOperationBinding
            {
                TimeToLive = new SolaceIntegerOrSchema(),
            });

            var exception = FluentActions.Invoking(() => operation.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0))
                .Should().Throw<System.InvalidOperationException>()
                .Which;
            exception.Message.Should().Contain("must contain an integer or a schema");
        }

        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void OperationBinding_ReportsIntegerOutsideInt64AsReaderDiagnostic(AsyncApiVersion version)
        {
            const string yaml = "bindings:\n  solace:\n    timeToLive: 9223372036854775808\n";
            var settings = new AsyncApiReaderSettings();
            settings.Bindings = new List<IBindingParser<IBinding>> { new SolaceOperationBinding() };

            new AsyncApiStringReader(settings)
                .ReadFragment<AsyncApiOperation>(yaml, version, out var diagnostics);
            diagnostics.Errors.Should().NotBeEmpty();
            diagnostics.Errors.Should().Contain(error => error.Message.Contains("could not parse to long"));
        }

        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void ServerBinding_RoundTripsFields(AsyncApiVersion version)
        {
            var yaml = version == AsyncApiVersion.AsyncApi2_0
                ? "url: localhost\nprotocol: solace\nbindings:\n  solace:\n    msgVpn: orders\n    clientName: consumer\n"
                : "host: localhost\nprotocol: solace\nbindings:\n  solace:\n    msgVpn: orders\n    clientName: consumer\n";
            var settings = new AsyncApiReaderSettings();
            settings.Bindings = new List<IBindingParser<IBinding>> { new SolaceServerBinding() };
            var reader = new AsyncApiStringReader(settings);
            var parsed = reader.ReadFragment<AsyncApiServer>(yaml, version, out _);
            var rewritten = parsed.SerializeAsYaml(version);
            var reparsed = reader.ReadFragment<AsyncApiServer>(rewritten, version, out _);
            reparsed.Bindings.Should().BeEquivalentTo(parsed.Bindings);
        }

        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void ParsedOperationBinding_RoundTripsNestedQueueAndTopicDestinations(AsyncApiVersion version)
        {
            const string yaml = """
                bindings:
                  solace:
                    destinations:
                      - destinationType: queue
                        deliveryMode: direct
                        queue:
                          name: orders
                          topicSubscriptions: [orders/>]
                          accessType: nonexclusive
                          maxMsgSpoolSize: 500
                          maxTtl: 900
                      - destinationType: topic
                        topic:
                          topicSubscriptions: [audit/>]
                """;
            var settings = new AsyncApiReaderSettings();
            settings.Bindings = new List<IBindingParser<IBinding>> { new SolaceOperationBinding() };
            var reader = new AsyncApiStringReader(settings);
            var parsed = reader.ReadFragment<AsyncApiOperation>(yaml, version, out _);
            var rewritten = parsed.SerializeAsYaml(version);
            var reparsed = reader.ReadFragment<AsyncApiOperation>(rewritten, version, out _);

            reparsed.Should().BeEquivalentTo(parsed);
        }
    }
}
