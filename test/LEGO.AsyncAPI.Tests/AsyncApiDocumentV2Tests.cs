// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using FluentAssertions;
    using LEGO.AsyncAPI.Bindings;
    using LEGO.AsyncAPI.Bindings.Http;
    using LEGO.AsyncAPI.Bindings.Kafka;
    using LEGO.AsyncAPI.Bindings.Pulsar;
    using LEGO.AsyncAPI.Models;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Readers;
    using LEGO.AsyncAPI.Writers;
    using NUnit.Framework;

    public class ExtensionClass
    {
        public string Key { get; set; }

        public long OtherKey { get; set; }
    }

    public class AsyncApiDocumentV2Tests : TestBase
    {
        [Test]
        public void V2_AsyncApiDocument_WithStreetLightsExample_SerializesAndDeserializes()
        {
            // Arrange
            var expected =
                """
                asyncapi: 2.6.0
                info:
                  title: Streetlights Kafka API
                  version: 1.0.0
                  description: The Smartylighting Streetlights API allows you to remotely manage the city lights.
                  license:
                    name: Apache 2.0
                    url: https://www.apache.org/licenses/LICENSE-2.0
                servers:
                  scram-connections:
                    url: test.mykafkacluster.org:18092
                    protocol: kafka-secure
                    description: Test broker secured with scramSha256
                    security:
                      - saslScram:
                    tags:
                      - name: env:test-scram
                        description: This environment is meant for running internal tests through scramSha256
                      - name: kind:remote
                        description: This server is a remote server. Not exposed by the application
                      - name: visibility:private
                        description: This resource is private and only available to certain users
                  mtls-connections:
                    url: test.mykafkacluster.org:28092
                    protocol: kafka-secure
                    description: Test broker secured with X509
                    security:
                      - certs:
                    tags:
                      - name: env:test-mtls
                        description: This environment is meant for running internal tests through mtls
                      - name: kind:remote
                        description: This server is a remote server. Not exposed by the application
                      - name: visibility:private
                        description: This resource is private and only available to certain users
                defaultContentType: application/json
                channels:
                  'smartylighting.streetlights.1.0.event.{streetlightId}.lighting.measured':
                    description: The topic on which measured values may be produced and consumed.
                    publish:
                      operationId: receiveLightMeasurement
                      summary: Inform about environmental lighting conditions of a particular streetlight.
                      traits:
                        - $ref: '#/components/operationTraits/kafka'
                      message:
                        $ref: '#/components/messages/lightMeasured'
                    parameters:
                      streetlightId:
                        $ref: '#/components/parameters/streetlightId'
                  'smartylighting.streetlights.1.0.action.{streetlightId}.turn.on':
                    subscribe:
                      operationId: turnOn
                      traits:
                        - $ref: '#/components/operationTraits/kafka'
                      message:
                        $ref: '#/components/messages/turnOnOff'
                    parameters:
                      streetlightId:
                        $ref: '#/components/parameters/streetlightId'
                  'smartylighting.streetlights.1.0.action.{streetlightId}.turn.off':
                    subscribe:
                      operationId: turnOff
                      traits:
                        - $ref: '#/components/operationTraits/kafka'
                      message:
                        $ref: '#/components/messages/turnOnOff'
                    parameters:
                      streetlightId:
                        $ref: '#/components/parameters/streetlightId'
                  'smartylighting.streetlights.1.0.action.{streetlightId}.dim':
                    subscribe:
                      operationId: dimLight
                      traits:
                        - $ref: '#/components/operationTraits/kafka'
                      message:
                        $ref: '#/components/messages/dimLight'
                    parameters:
                      streetlightId:
                        $ref: '#/components/parameters/streetlightId'
                components:
                  schemas:
                    lightMeasuredPayload:
                      type: object
                      properties:
                        lumens:
                          type: integer
                          description: Light intensity measured in lumens.
                          minimum: 0
                        sentAt:
                          $ref: '#/components/schemas/sentAt'
                    turnOnOffPayload:
                      type: object
                      properties:
                        command:
                          type: string
                          description: Whether to turn on or off the light.
                          enum:
                            - on
                            - off
                        sentAt:
                          $ref: '#/components/schemas/sentAt'
                    dimLightPayload:
                      type: object
                      properties:
                        percentage:
                          type: integer
                          description: Percentage to which the light should be dimmed to.
                          maximum: 100
                          minimum: 0
                        sentAt:
                          $ref: '#/components/schemas/sentAt'
                    sentAt:
                      type: string
                      format: date-time
                      description: Date and time when the message was sent.
                  messages:
                    lightMeasured:
                      payload:
                        $ref: '#/components/schemas/lightMeasuredPayload'
                      contentType: application/json
                      name: lightMeasured
                      title: Light measured
                      summary: Inform about environmental lighting conditions of a particular streetlight.
                      traits:
                        - $ref: '#/components/messageTraits/commonHeaders'
                    turnOnOff:
                      payload:
                        $ref: '#/components/schemas/turnOnOffPayload'
                      name: turnOnOff
                      title: Turn on/off
                      summary: Command a particular streetlight to turn the lights on or off.
                      traits:
                        - $ref: '#/components/messageTraits/commonHeaders'
                    dimLight:
                      payload:
                        $ref: '#/components/schemas/dimLightPayload'
                      name: dimLight
                      title: Dim light
                      summary: Command a particular streetlight to dim the lights.
                      traits:
                        - $ref: '#/components/messageTraits/commonHeaders'
                  securitySchemes:
                    saslScram:
                      type: scramSha256
                      description: Provide your username and password for SASL/SCRAM authentication
                    certs:
                      type: X509
                      description: Download the certificate files from service provider
                  parameters:
                    streetlightId:
                      description: The ID of the streetlight.
                      schema:
                        type: string
                  operationTraits:
                    kafka:
                      bindings:
                        kafka:
                          clientId:
                            type: string
                            enum:
                              - my-app-id
                  messageTraits:
                    commonHeaders:
                      headers:
                        type: object
                        properties:
                          my-app-header:
                            type: integer
                            maximum: 100
                            minimum: 0
                """;

            var asyncApiDocument = new AsyncApiDocumentBuilder()
                .WithInfo(new AsyncApiInfo
                {
                    Title = "Streetlights Kafka API",
                    Version = "1.0.0",
                    Description = "The Smartylighting Streetlights API allows you to remotely manage the city lights.",
                    License = new AsyncApiLicense
                    {
                        Name = "Apache 2.0",
                        Url = new Uri("https://www.apache.org/licenses/LICENSE-2.0"),
                    },
                })
                .WithServer("scram-connections", new AsyncApiServer
                {
                    Host = "test.mykafkacluster.org:18092",
                    Protocol = "kafka-secure",
                    Description = "Test broker secured with scramSha256",
                    Security = new List<AsyncApiSecurityScheme>
                    {
                        new AsyncApiSecuritySchemeReference("#/components/securitySchemes/saslScram"),
                    },
                    Tags = new List<AsyncApiTag>
                    {
                    new AsyncApiTag
                    {
                        Name = "env:test-scram",
                        Description = "This environment is meant for running internal tests through scramSha256",
                    },
                    new AsyncApiTag
                    {
                        Name = "kind:remote",
                        Description = "This server is a remote server. Not exposed by the application",
                    },
                    new AsyncApiTag
                    {
                        Name = "visibility:private",
                        Description = "This resource is private and only available to certain users",
                    },
                    },
                })
                .WithServer("mtls-connections", new AsyncApiServer
                {
                    Host = "test.mykafkacluster.org:28092",
                    Protocol = "kafka-secure",
                    Description = "Test broker secured with X509",
                    Security = new List<AsyncApiSecurityScheme>
                    {
                        new AsyncApiSecuritySchemeReference("#/components/securitySchemes/certs"),
                    },
                    Tags = new List<AsyncApiTag>
                    {
                    new AsyncApiTag
                    {
                        Name = "env:test-mtls",
                        Description = "This environment is meant for running internal tests through mtls",
                    },
                    new AsyncApiTag
                    {
                        Name = "kind:remote",
                        Description = "This server is a remote server. Not exposed by the application",
                    },
                    new AsyncApiTag
                    {
                        Name = "visibility:private",
                        Description = "This resource is private and only available to certain users",
                    },
                    },
                })
                .WithDefaultContentType()
                .WithChannel(
                "smartylighting.streetlights.1.0.event.{streetlightId}.lighting.measured",
                new AsyncApiChannel()
                {
                    Description = "The topic on which measured values may be produced and consumed.",
                    Parameters = new Dictionary<string, AsyncApiParameter>
                    {
                    {
                        "streetlightId", new AsyncApiParameterReference("#/components/parameters/streetlightId")
                    },
                    },
                })
                .WithChannel(
                "smartylighting.streetlights.1.0.action.{streetlightId}.turn.on",
                new AsyncApiChannel()
                {
                    Parameters = new Dictionary<string, AsyncApiParameter>
                    {
                    {
                        "streetlightId", new AsyncApiParameterReference("#/components/parameters/streetlightId")
                    },
                    },
                })
                .WithChannel(
                "smartylighting.streetlights.1.0.action.{streetlightId}.turn.off",
                new AsyncApiChannel()
                {
                    Parameters = new Dictionary<string, AsyncApiParameter>
                    {
                    {
                        "streetlightId", new AsyncApiParameterReference("#/components/parameters/streetlightId")
                    },
                    },
                })
                .WithChannel(
                "smartylighting.streetlights.1.0.action.{streetlightId}.dim",
                new AsyncApiChannel()
                {
                    Parameters = new Dictionary<string, AsyncApiParameter>
                    {
                    {
                        "streetlightId", new AsyncApiParameterReference("#/components/parameters/streetlightId")
                    },
                    },
                })
                .WithOperation("receiveLightMeasurement", new AsyncApiOperation()
                {
                    Action = AsyncApiAction.Send,
                    Summary = "Inform about environmental lighting conditions of a particular streetlight.",
                    Channel = new AsyncApiChannelReference("#/channels/smartylighting.streetlights.1.0.event.{streetlightId}.lighting.measured"),
                    Traits = new List<AsyncApiOperationTrait>
                    {
                        new AsyncApiOperationTraitReference("#/components/operationTraits/kafka"),
                    },
                    Messages = new List<AsyncApiMessageReference>
                    {
                        new("#/components/messages/lightMeasured"),
                    },
                })
                .WithOperation("turnOn", new AsyncApiOperation()
                {
                    Action = AsyncApiAction.Receive,
                    Channel = new AsyncApiChannelReference("#/channels/smartylighting.streetlights.1.0.action.{streetlightId}.turn.on"),
                    Traits = new List<AsyncApiOperationTrait>
                    {
                        new AsyncApiOperationTraitReference("#/components/operationTraits/kafka"),
                    },
                    Messages = new List<AsyncApiMessageReference>
                    {
                        new("#/components/messages/turnOnOff"),
                    },
                })
                .WithOperation("turnOff", new AsyncApiOperation()
                {
                    Action = AsyncApiAction.Receive,
                    Channel = new AsyncApiChannelReference("#/channels/smartylighting.streetlights.1.0.action.{streetlightId}.turn.off"),
                    Traits = new List<AsyncApiOperationTrait>
                    {
                        new AsyncApiOperationTraitReference("#/components/operationTraits/kafka"),
                    },
                    Messages = new List<AsyncApiMessageReference>
                    {
                        new("#/components/messages/turnOnOff"),
                    },
                })
                .WithOperation("dimLight", new AsyncApiOperation()
                {
                    Action = AsyncApiAction.Receive,
                    Channel = new AsyncApiChannelReference("#/channels/smartylighting.streetlights.1.0.action.{streetlightId}.dim"),
                    Traits = new List<AsyncApiOperationTrait>
                    {
                        new AsyncApiOperationTraitReference("#/components/operationTraits/kafka"),
                    },
                    Messages = new List<AsyncApiMessageReference>
                    {
                        new("#/components/messages/dimLight"),
                    },
                })
                .WithComponent("lightMeasured", new AsyncApiMessage()
                {
                    Name = "lightMeasured",
                    Title = "Light measured",
                    Summary = "Inform about environmental lighting conditions of a particular streetlight.",
                    ContentType = "application/json",
                    Traits = new List<AsyncApiMessageTrait>()
                    {
                    new AsyncApiMessageTraitReference("#/components/messageTraits/commonHeaders"),
                    },
                    Payload = new AsyncApiJsonSchemaReference("#/components/schemas/lightMeasuredPayload"),
                })
                .WithComponent("turnOnOff", new AsyncApiMessage()
                {
                    Name = "turnOnOff",
                    Title = "Turn on/off",
                    Summary = "Command a particular streetlight to turn the lights on or off.",
                    Traits = new List<AsyncApiMessageTrait>()
                    {
                    new AsyncApiMessageTraitReference("#/components/messageTraits/commonHeaders"),
                    },
                    Payload = new AsyncApiJsonSchemaReference("#/components/schemas/turnOnOffPayload"),
                })
                .WithComponent("dimLight", new AsyncApiMessage()
                {
                    Name = "dimLight",
                    Title = "Dim light",
                    Summary = "Command a particular streetlight to dim the lights.",
                    Traits = new List<AsyncApiMessageTrait>()
                    {
                    new AsyncApiMessageTraitReference("#/components/messageTraits/commonHeaders"),
                    },
                    Payload = new AsyncApiJsonSchemaReference("#/components/schemas/dimLightPayload"),
                })
                .WithComponent("lightMeasuredPayload", new AsyncApiJsonSchema()
                {
                    Type = SchemaType.Object,
                    Properties = new Dictionary<string, AsyncApiJsonSchema>()
                    {
                    {
                        "lumens", new AsyncApiJsonSchema()
                        {
                            Type = SchemaType.Integer,
                            Minimum = 0,
                            Description = "Light intensity measured in lumens.",
                        }
                    },
                    {
                        "sentAt", new AsyncApiJsonSchemaReference("#/components/schemas/sentAt")
                    },
                    },
                })
                .WithComponent("turnOnOffPayload", new AsyncApiJsonSchema()
                {
                    Type = SchemaType.Object,
                    Properties = new Dictionary<string, AsyncApiJsonSchema>()
                    {
                    {
                        "command", new AsyncApiJsonSchema()
                        {
                            Type = SchemaType.String,
                            Enum = new List<AsyncApiAny>
                            {
                                new AsyncApiAny("on"),
                                new AsyncApiAny("off"),
                            },
                            Description = "Whether to turn on or off the light.",
                        }
                    },
                    {
                        "sentAt", new AsyncApiJsonSchemaReference("#/components/schemas/sentAt")
                    },
                    },
                })
                .WithComponent("dimLightPayload", new AsyncApiJsonSchema()
                {
                    Type = SchemaType.Object,
                    Properties = new Dictionary<string, AsyncApiJsonSchema>()
                    {
                    {
                        "percentage", new AsyncApiJsonSchema()
                        {
                            Type = SchemaType.Integer,
                            Description = "Percentage to which the light should be dimmed to.",
                            Minimum = 0,
                            Maximum = 100,
                        }
                    },
                    {
                        "sentAt", new AsyncApiJsonSchemaReference("#/components/schemas/sentAt")
                    },
                    },
                })
                .WithComponent("sentAt", new AsyncApiJsonSchema()
                {
                    Type = SchemaType.String,
                    Format = "date-time",
                    Description = "Date and time when the message was sent.",
                })
                .WithComponent("saslScram", AsyncApiSecurityScheme.ScramSha256("Provide your username and password for SASL/SCRAM authentication"))
                .WithComponent("certs", AsyncApiSecurityScheme.X509("Download the certificate files from service provider"))
                .WithComponent("streetlightId", new AsyncApiParameter()
                {
                    Description = "The ID of the streetlight.",
                })
                .WithComponent("commonHeaders", new AsyncApiMessageTrait()
                {
                    Headers = new AsyncApiJsonSchema()
                    {
                        Type = SchemaType.Object,
                        Properties = new Dictionary<string, AsyncApiJsonSchema>()
                        {
                        {
                            "my-app-header", new AsyncApiJsonSchema()
                            {
                                Type = SchemaType.Integer,
                                Minimum = 0,
                                Maximum = 100,
                            }
                        },
                        },
                    },
                })
                .WithComponent("kafka", new AsyncApiOperationTrait()
                {
                    Bindings = new AsyncApiBindings<IOperationBinding>()
                    {
                    {
                        "kafka", new KafkaOperationBinding()
                        {
                            ClientId = new AsyncApiJsonSchema()
                            {
                                Type = SchemaType.String,
                                Enum = new List<AsyncApiAny>
                                {
                                    new AsyncApiAny("my-app-id"),
                                },
                            },
                        }
                    },
                    },
                })
                .Build();

            // Act
            var actual = asyncApiDocument.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);

            actual.Should()
                  .BePlatformAgnosticEquivalentTo(expected);
        }
    }
}