namespace ByteBard.AsyncAPI.Tests.Validation;

using System.Linq;
using FluentAssertions;
using ByteBard.AsyncAPI.Models;
using ByteBard.AsyncAPI.Readers;
using ByteBard.AsyncAPI.Validations;
using NUnit.Framework;

public class ValidationRuleTests
{
  [Test]
  public void V2_DocumentWithNoChannels_ShouldError()
  {
    // arrange
    var input =
      """
      asyncapi: 2.6.0
      info:
        title: Chat Application
        version: 1.0.0
      """;

    // act
    new AsyncApiStringReader().Read(input, out var diagnostic);

    // assert
    diagnostic.Errors.Should().Contain(e => e.Message == "The field 'channels' in 'document' object is REQUIRED.");
  }

  [Test]
  public void V2_DocumentWithChannels_ShouldPass()
  {
    // arrange
    var input =
      """
      asyncapi: 2.6.0
      info:
        title: Chat Application
        version: 1.0.0
      channels:
        chat:
          publish:
            operationId: onMessageReceived
            message:
              name: text
              payload:
                type: string
      """;

    // act
    new AsyncApiStringReader().Read(input, out var diagnostic);

    // assert
    diagnostic.Errors.Should().NotContain(e => e.Message.Contains("channels"));
  }

  [Test]
  public void V3_DocumentWithNoChannels_ShouldPass()
  {
    // arrange
    var input =
      """
      asyncapi: 3.0.0
      info:
        title: Chat Application
        version: 1.0.0
      """;

    // act
    new AsyncApiStringReader().Read(input, out var diagnostic);

    // assert
    diagnostic.Errors.Should().NotContain(e => e.Message.Contains("channels") && e.Message.Contains("REQUIRED"));
  }

  [Test]
  public void VersionAwareRuleSet_V2Rule_DoesNotRunOnV3Document()
  {
   public void VersionAwareRuleSet_V2Rule_DoesNotRunOnV3Document()
   {
     // arrange
     var ruleSet = ValidationRuleSet.GetDefaultRuleSet();
    var ruleSet = ValidationRuleSet.GetDefaultRuleSet();

    // act
    var rules = ruleSet.FindRules(typeof(AsyncApiDocument), AsyncApiVersion.AsyncApi3_0);

    // assert
    rules.Should().NotContain(r => r.ApplicableVersions != null && r.ApplicableVersions.Contains(AsyncApiVersion.AsyncApi2_0) && !r.ApplicableVersions.Contains(AsyncApiVersion.AsyncApi3_0));
  }

  [Test]
  public void VersionAwareRuleSet_V3Rule_DoesNotRunOnV2Document()
  {
    // arrange
    var ruleSet = ValidationRuleSet.GetDefaultRuleSet();

    // act
    var rules = ruleSet.FindRules(typeof(AsyncApiOperation), AsyncApiVersion.AsyncApi2_0);

    // assert
    rules.Should().NotContain(r => r.ApplicableVersions != null && r.ApplicableVersions.Contains(AsyncApiVersion.AsyncApi3_0) && !r.ApplicableVersions.Contains(AsyncApiVersion.AsyncApi2_0));
  }

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

    new AsyncApiStringReader().Read(input, out var diagnostic);
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

    new AsyncApiStringReader().Read(input, out var diagnostic);
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
            - $ref: '#/components/messages/messageSent'
      components:
        messages:
          messageSent:
            name: text
            payload:
              type: string
      """;

    new AsyncApiStringReader().Read(input, out var diagnostic);
    diagnostic.Errors.First().Message.Should().Be("The messages of operation 'Message received' MUST be a subset of the referenced channels messages.");
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

    new AsyncApiStringReader().Read(input, out var diagnostic);
    diagnostic.Errors.Should().BeEmpty();
  }
}