// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Tests.Validation
{
    using FluentAssertions;
    using LEGO.AsyncAPI.Readers;
    using NUnit.Framework;
    using System.Linq;

    public class ValidationRuleTests
    {
        [Test]
        public void V2_OperationId_WithNonUniqueKey_DiagnosticsError()
        {
            var input =
                """
                asyncapi: 2.6.0
                info:
                  title: Chat Application
                  version: 1.0.0
                servers:
                  testing:
                    url: test.mosquitto.org:1883
                    protocol: mqtt
                    description: Test broker
                channels:
                  chat/{personId}:
                    publish:
                      operationId: onMessageReceieved
                      message:
                        name: text
                        payload:
                          type: string
                  chat/{personIdentity}:
                    publish:
                      operationId: onMessageReceieved
                      message:
                        name: text
                        payload:
                          type: string
                """;

            var document = new AsyncApiStringReader().Read(input, out var diagnostic);
            diagnostic.Errors.First().Message.Should().Be("OperationId: 'onMessageReceieved' is not unique.");
            diagnostic.Errors.First().Pointer.Should().Be("#/channels/chat~1{personIdentity}");
        }

        [Test]
        public void V3_OperationChannel_NotReferencingARootChannel_DiagnosticsError()
        {
          var input =
            """
            asyncapi: 3.0.0
            info:
              title: Chat Application
              version: 1.0.0
            servers:
              testing:
                host: test.mosquitto.org:1883
                protocol: mqtt
                description: Test broker
            channels:
              chatPersonId:
                address: chat.{personId}
                messages:
                  messageReceived:
                    name: text
                    payload:
                      type: string
            operations:
              onMessageReceived:
                title: Message received
                channel:
                  $ref: '#/components/channels/secondChannel'
                messages:
                  - $ref: '#/channels/chatPersonId/messages/messageReceived'
            components:
              channels:
                secondChannel:
                  address: chat.{secondChannel}
            """;

          var document = new AsyncApiStringReader().Read(input, out var diagnostic);
          diagnostic.Errors.First().Message.Should().Be("The operation 'Message received' MUST point to a channel definition located in the root Channels Object.");
          diagnostic.Errors.First().Pointer.Should().Be("#/operations/onMessageReceived");
        }

        [Test]
        public void V3_OperationMessage_NotReferencingARootChannel_DiagnosticsError()
        {
          var input =
            """
            asyncapi: 3.0.0
            info:
              title: Chat Application
              version: 1.0.0
            servers:
              testing:
                host: test.mosquitto.org:1883
                protocol: mqtt
                description: Test broker
            channels:
              chatPersonId:
                address: chat.{personId}
                messages:
                  messageReceived:
                    name: text
                    payload:
                      type: string
            operations:
              onMessageReceived:
                title: Message received
                channel:
                  $ref: '#/channels/chatPersonId'
                messages:
                  - $ref: '#/channels/chatPersonId/messages/messageReceived'
            """;

          var document = new AsyncApiStringReader().Read(input, out var diagnostic);
          diagnostic.Errors.First().Message.Should().Be("The operation 'Message received' MUST point to a channel definition located in the root Channels Object.");
          diagnostic.Errors.First().Pointer.Should().Be("#/operations/onMessageReceived");
        }

        [Test]
        [TestCase("chat")]
        [TestCase("/some/chat/{personId}")]
        [TestCase("chat-{personId}")]
        [TestCase("chat-{person_id}")]
        [TestCase("chat-{person%2Did}")]
        [TestCase("chat-{personId2}")]
        public void ChannelKey_WithValidKey_Success(string channelKey)
        {
            var input =
                $"""
                asyncapi: 2.6.0
                info:
                  title: Chat Application
                  version: 1.0.0
                servers:
                  testing:
                    url: test.mosquitto.org:1883
                    protocol: mqtt
                    description: Test broker
                channels:
                  {channelKey}:
                    publish:
                      operationId: onMessageReceieved
                      message:
                        name: text
                        payload:
                          type: string
                    subscribe:
                      operationId: sendMessage
                      message:
                        name: text
                        payload:
                          type: string
                """;

            var document = new AsyncApiStringReader().Read(input, out var diagnostic);
            diagnostic.Errors.Should().BeEmpty();
        }
    }

}
