namespace ByteBard.AsyncAPI.Tests.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using ByteBard.AsyncAPI.Bindings.Kafka;
    using ByteBard.AsyncAPI.Bindings.WebSockets;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers;
    using NUnit.Framework;

    public class AsyncApiChannel_Should : TestBase
    {
        [Test]
        public void V2_AsyncApiChannel_WithInlineParameter_DoesNotCreateReference()
        {
            var input =
                """
                    parameters:
                      id:
                        description: ids
                        schema:
                          type: string
                          enum:
                            - 08735ae0-6a1a-4578-8b4a-35aa26d15993
                            - 97845c62-329c-4d87-ad24-4f611b909a10
                """;

            var channel = new AsyncApiStringReader().ReadFragment<AsyncApiChannel>(input, AsyncApiVersion.AsyncApi2_0, out var _);
            channel.Parameters.First().Value.Should().NotBeOfType<AsyncApiParameterReference>();
        }

        [Test]
        public void V2_AsyncApiChannel_WithWebSocketsBinding_Serializes()
        {
            var expected = """
                bindings:
                  ws:
                    method: POST
                    query:
                      properties:
                        index:
                          description: the index
                    headers:
                      properties:
                        x-correlation-id:
                          description: the correlationid
                    bindingVersion: 0.1.0
                """;

            var channel = new AsyncApiChannel
            {
                Bindings = new AsyncApiBindings<IChannelBinding>
                {
                    {
                        new WebSocketsChannelBinding()
                        {
                            Method = "POST",
                            Query = new AsyncApiJsonSchema()
                            {
                                Properties = new Dictionary<string, AsyncApiJsonSchema>()
                                {
                                    {
                                        "index", new AsyncApiJsonSchema()
                                        {
                                            Description = "the index",
                                        }
                                    },
                                },
                            },
                            Headers = new AsyncApiJsonSchema()
                            {
                                Properties = new Dictionary<string, AsyncApiJsonSchema>()
                                {
                                    {
                                        "x-correlation-id", new AsyncApiJsonSchema()
                                        {
                                            Description = "the correlationid",
                                        }
                                    },
                                },
                            },
                            BindingVersion = "0.1.0",
                        }
                    },
                },
            };

            var actual = channel.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);

            // Assert
            actual.Should()
                  .BePlatformAgnosticEquivalentTo(expected);
        }

        [Test]
        public void V2_AsyncApiChannel_WithKafkaBinding_Serializes()
        {
            var expected =
                """
                bindings:
                  kafka:
                    topic: topic
                    partitions: 5
                    replicas: 2
                """;

            var channel = new AsyncApiChannel
            {
                Bindings = new AsyncApiBindings<IChannelBinding>
                {
                    {
                        new KafkaChannelBinding
                        {
                            Topic = "topic",
                            Partitions = 5,
                            Replicas = 2,
                        }
                    },
                },
            };

            var actual = channel.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);

            // Assert
            actual.Should()
                  .BePlatformAgnosticEquivalentTo(expected);
        }

        [Test]
        public void V2_AsyncApiChannel_UpgradesAndNormalizesKey()
        {
            var yaml =
                """
                asyncapi: 2.6.0
                info:
                  title: test spec
                  version: 1.0.0
                channels:
                  'mychannel/{param}':
                    description: test channel
                    parameters:
                      param:
                        description: some parameter
                """;

            var document = new AsyncApiStringReader().Read(yaml, out var diag);

            document.Channels.First().Key.Should().Be("mychannelparam");
            document.Channels.First().Value.Address.Should().Be("mychannel/{param}");
            document.Channels.First().Value.Parameters.First().Key.Should().Be("param");

            var reserialized = document.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);

            reserialized.Should().BePlatformAgnosticEquivalentTo(yaml);
        }

        [Test]
        public void V2_AsyncApiChannel_WithServers_SerializesServerNames()
        {
            var yaml =
                """
                asyncapi: 2.6.0
                info:
                  title: test spec
                  version: 1.0.0
                servers:
                  production:
                    url: example.com
                    protocol: kafka
                channels:
                  mychannel:
                    servers:
                      - production
                """;

            var document = new AsyncApiStringReader().Read(yaml, out var diag);

            document.Channels.First().Value.Servers.Single().Reference.Reference.Should().Be("#/servers/production");

            var reserialized = document.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);

            reserialized.Should().BePlatformAgnosticEquivalentTo(yaml);
        }

        [Test]
        public void V2_AsyncApiChannel_WithServerReference_SerializesServerName()
        {
            var expected =
                """
                servers:
                  - production
                """;

            var channel = new AsyncApiChannel
            {
                Servers = new List<AsyncApiServerReference> { new AsyncApiServerReference("#/servers/production") },
            };

            var actual = channel.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);

            actual.Should().BePlatformAgnosticEquivalentTo(expected);
        }

        [TestCase("#/servers/east~1west", "east/west")]
        [TestCase("#/servers/a~0b", "a~b")]
        [TestCase("other.yaml#/servers/production", "other.yaml#/servers/production")]
        [TestCase("#/components/servers/production", "'#/components/servers/production'")]
        public void V2_AsyncApiChannel_WithServerReference_SerializesServerNameOnlyForLocalServer(string reference, string expectedServer)
        {
            var channel = new AsyncApiChannel
            {
                Servers = new List<AsyncApiServerReference> { new AsyncApiServerReference(reference) },
            };

            var actual = channel.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);

            actual.Should().BePlatformAgnosticEquivalentTo($"servers:\n  - {expectedServer}");
        }

        [Test]
        public void V3_AsyncApiChannel_WithServers_Reserializes()
        {
            var yaml =
                """
                asyncapi: 3.1.0
                info:
                  title: test spec
                  version: 1.0.0
                servers:
                  production:
                    host: example.com
                    protocol: kafka
                channels:
                  mychannel:
                    servers:
                      - $ref: '#/servers/production'
                components: { }
                """;

            var document = new AsyncApiStringReader().Read(yaml, out var diag);

            diag.Errors.Should().BeEmpty();
            var server = document.Channels.First().Value.Servers.Single();
            server.Reference.Reference.Should().Be("#/servers/production");
            server.Host.Should().Be("example.com");

            var reserialized = document.SerializeAsYaml(AsyncApiVersion.AsyncApi3_0);

            reserialized.Should().BePlatformAgnosticEquivalentTo(yaml);
        }

        [Test]
        public void V3_AsyncApiChannel_WithServerReference_SerializesReference()
        {
            var expected =
                """
                servers:
                  - $ref: '#/servers/production'
                """;

            var channel = new AsyncApiChannel
            {
                Servers = new List<AsyncApiServerReference> { new AsyncApiServerReference("#/servers/production") },
            };

            var actual = channel.SerializeAsYaml(AsyncApiVersion.AsyncApi3_0);

            actual.Should().BePlatformAgnosticEquivalentTo(expected);
        }

        [Test]
        public void V2_AsyncApiChannel_WithServers_UpgradesToV3References()
        {
            var yaml =
                """
                asyncapi: 2.6.0
                info:
                  title: test spec
                  version: 1.0.0
                servers:
                  production:
                    url: example.com
                    protocol: kafka
                channels:
                  mychannel:
                    servers:
                      - production
                """;

            var document = new AsyncApiStringReader().Read(yaml, out var diag);

            var reserialized = document.SerializeAsYaml(AsyncApiVersion.AsyncApi3_0);

            reserialized.Should().Contain("- $ref: '#/servers/production'");
        }

        [Test]
        public void V2_AsyncApiChannel_reserializes()
        {
            var expected =
                """
                asyncapi: 2.6.0
                info:
                  title: test spec
                  version: 1.0.0
                channels:
                  mychannel:
                    description: test channel
                    subscribe:
                      description: what ever
                      message:
                        $ref: '#/components/messages/anonymous-message-1'
                components:
                  messages:
                    anonymous-message-1:
                      payload:
                        type: string
                """;

            var yaml =
                """
                asyncapi: 2.6.0
                info:
                  title: test spec
                  version: 1.0.0
                channels:
                  mychannel:
                    description: test channel
                    subscribe:
                      description: what ever
                      message:
                        payload:
                          type: string
                """;

            var document = new AsyncApiStringReader().Read(yaml, out var diag);
            var reserialized = document.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);

            reserialized.Should().BePlatformAgnosticEquivalentTo(expected);
        }
    }
}
