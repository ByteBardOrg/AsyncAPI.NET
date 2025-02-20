// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Tests.Models
{
    using System;
    using System.IO;
    using System.Linq;
    using FluentAssertions;
    using LEGO.AsyncAPI.Bindings.Http;
    using LEGO.AsyncAPI.Bindings.Kafka;
    using LEGO.AsyncAPI.Models;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Readers;
    using LEGO.AsyncAPI.Writers;
    using NUnit.Framework;

    public class AsyncApiOperation_Should : TestBase
    {
        [Test]
        public void V2_SerializeV2_WithNullWriter_Throws()
        {
            // Arrange
            var asyncApiOperation = new AsyncApiOperation();

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => { asyncApiOperation.SerializeV2(null); });
        }

        [Test]
        public void V2_SerializeV2_WithMultipleMessages_Upgrades()
        {
            // Arrange
            var yaml =
                """
                asyncapi: 2.6.0
                info:
                  title: Example API with oneOf in messages
                  version: 1.0.0

                channels:
                  SomeChannel:
                    description: A channel where messages can be sent and received.
                    subscribe:
                      operationId: receiveMessage
                      summary: 'Receives a message that can be either a text or a file.'
                      message:
                        oneOf:
                          - $ref: '#/components/messages/TextMessage'
                          - $ref: '#/components/messages/FileMessage'
                          - description: Http Message
                            contentType: 'application/json'
                            payload:
                              type: string
                          - description: web socket Message
                            messageId: wsMessage
                            contentType: 'application/json'
                            payload:
                              type: string
                components:
                  messages:
                    TextMessage:
                      contentType: 'application/json'
                      payload:
                        type: 'object'
                        properties:
                          type:
                            type: 'string'
                            enum:
                              - 'text'
                          content:
                            type: 'string'
                            description: 'The text content of the message.'

                    FileMessage:
                      contentType: 'application/json'
                      payload:
                        type: 'object'
                        properties:
                          type:
                            type: 'string'
                            enum:
                              - 'file'
                          filename:
                            type: 'string'
                            description: 'The name of the file.'
                          fileData:
                            type: 'string'
                            format: 'byte'
                            description: 'The file content encoded in base64.'

                """;

            // Act
            var document = new AsyncApiStringReader().Read(yaml, out var diagnostics);

            // Assert
            document.Components.Messages.Should().HaveCount(4);
            document.Operations.Should().HaveCount(1);
            document.Operations.First().Value.Messages.Should().HaveCount(4);
            document.Channels.First().Value.Messages.Should().HaveCount(4);
            // Missing resolution of references of references.
        }

        [Test]
        public void V2_SerializeV2_WithMultipleMessages_SerializesWithOneOf()
        {
            // Arrange
            var expected = """
                message:
                  oneOf:
                    - $ref: '#/components/messages/first'
                    - $ref: '#/components/messages/second'
                """;

            var asyncApiOperation = new AsyncApiOperation();
            asyncApiOperation.Messages.Add(new AsyncApiMessageReference("#/components/messages/first"));
            asyncApiOperation.Messages.Add(new AsyncApiMessageReference("#/components/messages/second"));
            var outputString = new StringWriter();
            var settings = new AsyncApiWriterSettings();
            var writer = new AsyncApiYamlWriter(outputString, settings);

            // Act
            asyncApiOperation.SerializeV2(writer);

            // Assert
            var actual = outputString.GetStringBuilder().ToString();

            actual.Should()
                .BePlatformAgnosticEquivalentTo(expected);
        }

        [Test]
        public void V2_SerializeV2_WithSingleMessage_Serializes()
        {
            // Arrange
            var expected = """
                message:
                  $ref: '#/components/messages/first'
                """;

            var asyncApiOperation = new AsyncApiOperation();
            asyncApiOperation.Messages.Add(new AsyncApiMessageReference("#/components/messages/first"));
            var settings = new AsyncApiWriterSettings();
            var outputString = new StringWriter();
            var writer = new AsyncApiYamlWriter(outputString, settings);

            // Act
            asyncApiOperation.SerializeV2(writer);

            // Assert
            var actual = outputString.GetStringBuilder().ToString();

            actual.Should()
                  .BePlatformAgnosticEquivalentTo(expected);
        }

        [Test]
        public void V2_AsyncApiOperation_WithBindings_Serializes()
        {
            var expected =
                """
                bindings:
                  http:
                    type: request
                    method: PUT
                    query:
                      description: some query
                  kafka:
                    groupId:
                      description: some Id
                    clientId:
                      description: some Id
                """;

            var operation = new AsyncApiOperation
            {
                Bindings = new AsyncApiBindings<IOperationBinding>
                {
                    {
                        new HttpOperationBinding
                        {
                            Type = HttpOperationBinding.HttpOperationType.Request,
                            Method = "PUT",
                            Query = new AsyncApiJsonSchema
                            {
                                Description = "some query",
                            },
                        }
                    },
                    {
                        new KafkaOperationBinding
                        {
                            GroupId = new AsyncApiJsonSchema
                            {
                                Description = "some Id",
                            },
                            ClientId = new AsyncApiJsonSchema
                            {
                                Description = "some Id",
                            },
                        }
                    },
                },
            };

            var actual = operation.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);

            // Assert
            actual.Should()
                .BePlatformAgnosticEquivalentTo(expected);
        }
    }
}
