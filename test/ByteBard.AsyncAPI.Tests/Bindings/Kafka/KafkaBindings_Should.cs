namespace ByteBard.AsyncAPI.Tests.Bindings.Kafka
{
    using System.Collections.Generic;
    using System.Text.Json.Nodes;
    using FluentAssertions;
    using ByteBard.AsyncAPI.Bindings;
    using ByteBard.AsyncAPI.Bindings.Kafka;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers;
    using NUnit.Framework;

    internal class KafkaBindings_Should : TestBase
    {
        [Test]
        public void V2_KafkaTopicConfiguration_PreservesAdditionalScalarAndListValues()
        {
            var yaml = """
                bindings:
                  kafka:
                    topicConfiguration:
                      custom.string: value
                      custom.integer: 12
                      custom.boolean: false
                      custom.list:
                        - one
                        - 2
                        - true
                """;
            var settings = new AsyncApiReaderSettings { Bindings = BindingsCollection.Kafka };
            var reader = new AsyncApiStringReader(settings);

            var channel = reader.ReadFragment<AsyncApiChannel>(yaml, AsyncApiVersion.AsyncApi2_0, out var diagnostic);
            diagnostic.Errors.Should().BeEmpty();

            var serialized = channel.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);
            var roundTripped = reader.ReadFragment<AsyncApiChannel>(serialized, AsyncApiVersion.AsyncApi2_0, out diagnostic);

            diagnostic.Errors.Should().BeEmpty();
            var configuration = ((KafkaChannelBinding)roundTripped.Bindings["kafka"]).TopicConfiguration;
            configuration.AdditionalProperties["custom.string"].GetValue<string>().Should().Be("value");
            configuration.AdditionalProperties["custom.integer"].GetValue<decimal>().Should().Be(12);
            configuration.AdditionalProperties["custom.boolean"].GetValue<bool>().Should().BeFalse();
            configuration.AdditionalProperties["custom.list"].GetNode().Should().BeOfType<JsonArray>();
            configuration.AdditionalProperties["custom.list"].GetNode().ToJsonString().Should().Be("[\"one\",2,true]");
        }

        [Test]
        public void V2_KafkaChannelBinding_WithFilledObject_SerializesAndDeserializes()
        {
            // Arrange
            var expected =
                """
                bindings:
                  kafka:
                    topic: myTopic
                    partitions: 5
                    replicas: 4
                    topicConfiguration:
                      cleanup.policy:
                        - delete
                        - compact
                      retention.ms: 15552000000
                      retention.bytes: 2
                      delete.retention.ms: 3
                      max.message.bytes: 4
                      confluent.key.schema.validation: true
                      confluent.key.subject.name.strategy: TopicNameStrategy
                      confluent.value.schema.validation: true
                      confluent.value.subject.name.strategy: TopicNameStrategy
                """;

            var channel = new AsyncApiChannel();
            channel.Bindings.Add(new KafkaChannelBinding
            {
                Topic = "myTopic",
                Partitions = 5,
                Replicas = 4,
                TopicConfiguration = new TopicConfigurationObject()
                {
                    CleanupPolicy = new List<string> { "delete", "compact" },
                    RetentionMilliseconds = 15552000000,
                    RetentionBytes = 2,
                    DeleteRetentionMilliseconds = 3,
                    MaxMessageBytes = 4,
                    ConfluentKeySchemaValidation = true,
                    ConfluentKeySubjectName = "TopicNameStrategy",
                    ConfluentValueSchemaValidation = true,
                    ConfluentValueSubjectName = "TopicNameStrategy",
                },
            });

            // Act
            var actual = channel.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);

            // Assert
            var settings = new AsyncApiReaderSettings();
            settings.Bindings = BindingsCollection.Kafka;
            var binding = new AsyncApiStringReader(settings).ReadFragment<AsyncApiChannel>(actual, AsyncApiVersion.AsyncApi2_0, out _);

            // Assert
            actual.Should()
                  .BePlatformAgnosticEquivalentTo(expected);
            binding.Should().BeEquivalentTo(channel);
        }

        [Test]
        public void V2_KafkaServerBinding_WithFilledObject_SerializesAndDeserializes()
        {
            // Arrange
            var expected =
                """
                url: example.com
                protocol: kafka
                bindings:
                  kafka:
                    schemaRegistryUrl: https://example.com/schemaregistry
                    schemaRegistryVendor: confluent
                """;

            var server = new AsyncApiServer()
            {
                Host = "example.com",
                Protocol = "kafka",
            };

            server.Bindings.Add(new KafkaServerBinding
            {
                SchemaRegistryUrl = "https://example.com/schemaregistry",
                SchemaRegistryVendor = "confluent",
            });

            // Act
            var actual = server.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);
            var settings = new AsyncApiReaderSettings();
            settings.Bindings = BindingsCollection.Kafka;
            var binding = new AsyncApiStringReader(settings).ReadFragment<AsyncApiServer>(actual, AsyncApiVersion.AsyncApi2_0, out _);

            // Assert
            actual.Should()
                  .BePlatformAgnosticEquivalentTo(expected);
            binding.Should().BeEquivalentTo(server);
        }

        [Test]
        public void V2_KafkaMessageBinding_WithFilledObject_SerializesAndDeserializes()
        {
            // Arrange
            var expected =
                """
                bindings:
                  kafka:
                    key:
                      description: this mah other binding
                    schemaIdLocation: test
                    schemaIdPayloadEncoding: test
                    schemaLookupStrategy: header
                """;

            var message = new AsyncApiMessage();

            message.Bindings.Add(new KafkaMessageBinding
            {
                Key = new AsyncApiJsonSchema
                {
                    Description = "this mah other binding",
                },
                SchemaIdLocation = "test",
                SchemaIdPayloadEncoding = "test",
                SchemaLookupStrategy = "header",
            });

            // Act
            var actual = message.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);
            var settings = new AsyncApiReaderSettings();
            settings.Bindings = BindingsCollection.Kafka;
            var binding = new AsyncApiStringReader(settings).ReadFragment<AsyncApiMessage>(actual, AsyncApiVersion.AsyncApi2_0, out _);

            // Assert
            actual.Should()
                  .BePlatformAgnosticEquivalentTo(expected);
            binding.Should().BeEquivalentTo(message);
        }

        [Test]
        public void V2_KafkaOperationBinding_WithFilledObject_SerializesAndDeserializes()
        {
            // Arrange
            var expected =
                """
                bindings:
                  kafka:
                    groupId:
                      description: this mah groupId
                    clientId:
                      description: this mah clientId
                """;

            var operation = new AsyncApiOperation();

            operation.Bindings.Add(new KafkaOperationBinding
            {
                GroupId = new AsyncApiJsonSchema
                {
                    Description = "this mah groupId",
                },
                ClientId = new AsyncApiJsonSchema
                {
                    Description = "this mah clientId",
                },
            });

            // Act
            var actual = operation.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);

            var settings = new AsyncApiReaderSettings();
            settings.Bindings = BindingsCollection.Kafka;
            var binding = new AsyncApiStringReader(settings).ReadFragment<AsyncApiOperation>(actual, AsyncApiVersion.AsyncApi2_0, out _);

            // Assert
            actual.Should()
                  .BePlatformAgnosticEquivalentTo(expected);
            binding.Should().BeEquivalentTo(operation);
        }
    }
}
