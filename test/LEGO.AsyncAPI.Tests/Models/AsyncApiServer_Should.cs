// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Tests.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using LEGO.AsyncAPI.Bindings;
    using LEGO.AsyncAPI.Bindings.Kafka;
    using LEGO.AsyncAPI.Models;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Readers;
    using NUnit.Framework;

    internal class AsyncApiServer_Should : TestBase
    {
        [Test]
        public void V2_AsyncApiServer_Upgrades()
        {
            // Arrange
            var actual =
                """
                url: 'test://example.com/{channelKey}'
                protocol: test
                protocolVersion: 0.1.0
                description: some description
                variables:
                  channelKey:
                    description: some description
                security:
                  - schem1:
                      - requirement
                tags:
                  - name: mytag1
                    description: description of tag1
                bindings:
                  kafka:
                    schemaRegistryUrl: http://example.com
                    schemaRegistryVendor: kafka
                """;

            var deserialized = new AsyncApiStringReader(new AsyncApiReaderSettings { Bindings = BindingsCollection.All }).ReadFragment<AsyncApiServer>(actual, AsyncApiVersion.AsyncApi2_0, out var diag);
            deserialized.Security.Should().HaveCount(1);
            deserialized.Host.Should().Be("example.com");
            deserialized.PathName.Should().Be("/{channelKey}");
            deserialized.Protocol.Should().Be("test");
            deserialized.ProtocolVersion.Should().Be("0.1.0");
            deserialized.Description.Should().Be("some description");
            deserialized.Variables.Should().HaveCount(1);
            deserialized.Variables["channelKey"].Description.Should().Be("some description");
            deserialized.Tags.Should().HaveCount(1);
            deserialized.Tags.First().Name.Should().Be("mytag1");
            deserialized.Tags.First().Description.Should().Be("description of tag1");
            deserialized.Bindings.Should().HaveCount(1);
            deserialized.Bindings["kafka"].As<KafkaServerBinding>().SchemaRegistryUrl.Should().Be("http://example.com");
            deserialized.Bindings["kafka"].As<KafkaServerBinding>().SchemaRegistryVendor.Should().Be("kafka");
        }

        [Test]
        public void V2_AsyncApiServer_Serializes()
        {
            // Arrange
            var expected =
                """
                asyncapi: 2.6.0
                info:
                  title: test
                  version: 1.0.0
                servers:
                  testServer:
                    url: 'test://example.com/{channelkey}'
                    protocol: test
                    protocolVersion: 0.1.0
                    description: some description
                    variables:
                      channelkey:
                        description: some description
                    security:
                      - schem1:
                          - requirement
                      - schem2:
                          - otherRequirement
                    tags:
                      - name: mytag1
                        description: description of tag1
                    bindings:
                      kafka:
                        schemaRegistryUrl: http://example.com
                        schemaRegistryVendor: kafka
                channels:
                  test:
                    description: testChannel
                components:
                  securitySchemes:
                    schem1:
                      type: http
                      scheme: whatever
                    schem2:
                      type: http
                      scheme: whatever
                """;

            var deserialized = new AsyncApiStringReader(new AsyncApiReaderSettings() { Bindings = BindingsCollection.All }).Read(expected, out var diag);

            // Act
            var actual = deserialized.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);

            // Assert
            actual.Should()
                  .BePlatformAgnosticEquivalentTo(expected);
        }

        [Test]
        public void V2_AsyncApiServer_WithKafkaBinding_Serializes()
        {
            var expected =
                """
                url: test://example.com
                protocol: test
                bindings:
                  kafka:
                    schemaRegistryUrl: http://example.com
                    schemaRegistryVendor: kafka
                """;
            var server = new AsyncApiServer
            {
                Host = "example.com",
                Protocol = "test",
                Bindings = new AsyncApiBindings<IServerBinding>
                {
                    {
                        new KafkaServerBinding
                        {
                            SchemaRegistryUrl = "http://example.com",
                            SchemaRegistryVendor = "kafka",
                        }
                    },
                },
            };

            var actual = server.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);

            // Assert
            actual.Should()
                 .BePlatformAgnosticEquivalentTo(expected);
        }
    }
}
