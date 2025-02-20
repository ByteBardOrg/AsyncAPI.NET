﻿// Copyright (c) The LEGO Group. All rights reserved.

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
                    url: kafka-secure://test.mykafkacluster.org:18092
                    protocol: kafka-secure
                    description: Test broker secured with scramSha256
                    security:
                      - saslScram: []
                    tags:
                      - name: env:test-scram
                        description: This environment is meant for running internal tests through scramSha256
                      - name: kind:remote
                        description: This server is a remote server. Not exposed by the application
                      - name: visibility:private
                        description: This resource is private and only available to certain users
                  mtls-connections:
                    url: kafka-secure://test.mykafkacluster.org:28092
                    protocol: kafka-secure
                    description: Test broker secured with X509
                    security:
                      - certs: []
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
                        default: '1'
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
                "lighting.measured",
                new AsyncApiChannel()
                {
                    Address = "smartylighting.streetlights.1.0.event.{streetlightId}.lighting.measured",
                    Description = "The topic on which measured values may be produced and consumed.",
                    Parameters = new Dictionary<string, AsyncApiParameter>
                    {
                    {
                        "streetlightId", new AsyncApiParameterReference("#/components/parameters/streetlightId")
                    },
                    },
                })
                .WithChannel(
                "turn.on",
                new AsyncApiChannel()
                {
                    Address = "smartylighting.streetlights.1.0.action.{streetlightId}.turn.on",
                    Parameters = new Dictionary<string, AsyncApiParameter>
                    {
                    {
                        "streetlightId", new AsyncApiParameterReference("#/components/parameters/streetlightId")
                    },
                    },
                })
                .WithChannel(
                "turn.off",
                new AsyncApiChannel()
                {
                    Address = "smartylighting.streetlights.1.0.action.{streetlightId}.turn.off",
                    Parameters = new Dictionary<string, AsyncApiParameter>
                    {
                    {
                        "streetlightId", new AsyncApiParameterReference("#/components/parameters/streetlightId")
                    },
                    },
                })
                .WithChannel(
                "dim",
                new AsyncApiChannel()
                {
                    Address = "smartylighting.streetlights.1.0.action.{streetlightId}.dim",
                    Parameters = new Dictionary<string, AsyncApiParameter>
                    {
                    {
                        "streetlightId", new AsyncApiParameterReference("#/components/parameters/streetlightId")
                    },
                    },
                })
                .WithOperation("receiveLightMeasurement", new AsyncApiOperation()
                {
                    Action = AsyncApiAction.Receive,
                    Summary = "Inform about environmental lighting conditions of a particular streetlight.",
                    Channel = new AsyncApiChannelReference("#/channels/lighting.measured"),
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
                    Action = AsyncApiAction.Send,
                    Channel = new AsyncApiChannelReference("#/channels/turn.on"),
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
                    Action = AsyncApiAction.Send,
                    Channel = new AsyncApiChannelReference("#/channels/turn.off"),
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
                    Action = AsyncApiAction.Send,
                    Channel = new AsyncApiChannelReference("#/channels/dim"),
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
                    Default = "1",
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

        [Test]
        public void V2_SerializeV2_WithFullSpec_Serializes()
        {
            var expected =
                """
                asyncapi: 2.6.0
                info:
                  title: apiTitle
                  version: apiVersion
                  description: description
                  termsOfService: https://example.com/termsOfService
                  contact:
                    name: contactName
                    url: https://example.com/contact
                    email: contactEmail
                  license:
                    name: licenseName
                    url: https://example.com/license
                    x-extension: value
                  x-extension: value
                id: documentId
                servers:
                  myServer:
                    url: kafkaprotocol://example.com/server
                    protocol: KafkaProtocol
                    protocolVersion: protocolVersion
                    description: serverDescription
                    security:
                      - securitySchemeName: []
                channels:
                  channel1:
                    description: channelDescription
                    subscribe:
                      operationId: myOperation
                      summary: operationSummary
                      description: operationDescription
                      tags:
                        - name: tagName
                          description: tagDescription
                      externalDocs:
                        description: externalDocsDescription
                        url: https://example.com/externalDocs
                      traits:
                        - operationId: myOperation
                          summary: traitSummary
                          description: traitDescription
                          tags:
                            - name: tagName
                              description: tagDescription
                          externalDocs:
                            description: externalDocsDescription
                            url: https://example.com/externalDocs
                          x-extension: value
                      message:
                        oneOf:
                          - contentType: contentType
                            name: messageName
                            title: messageTitle
                            summary: messageSummary
                            description: messageDescription
                          - correlationId:
                              description: correlationDescription
                              location: correlationLocation
                              x-extension: value
                            contentType: contentType
                            name: messageName
                            title: messageTitle
                            summary: messageSummary
                            description: messageDescription
                            traits:
                              - headers:
                                  title: schemaTitle
                                  description: schemaDescription
                                  writeOnly: true
                                  examples:
                                    - key: value
                                      otherKey: 9223372036854775807
                                name: traitName
                                title: traitTitle
                                summary: traitSummary
                                description: traitDescription
                                tags:
                                  - name: tagName
                                    description: tagDescription
                                externalDocs:
                                  description: externalDocsDescription
                                  url: https://example.com/externalDocs
                                examples:
                                  - name: exampleName
                                    summary: exampleSummary
                                    payload:
                                      key: value
                                      otherKey: 9223372036854775807
                                    x-extension: value
                                x-extension: value
                            x-extension: value
                      x-extension: value
                components:
                  securitySchemes:
                    securitySchemeName:
                      type: oauth2
                      description: securitySchemeDescription
                      flows:
                        implicit:
                          authorizationUrl: https://example.com/authorization
                          tokenUrl: https://example.com/tokenUrl
                          refreshUrl: https://example.com/refresh
                          scopes:
                            securitySchemeScopeKey: securitySchemeScopeValue
                          x-extension: value
                """;

            // Arrange
            var title = "apiTitle";
            string contactName = "contactName";
            string contactEmail = "contactEmail";
            string contactUri = "https://example.com/contact";
            string description = "description";
            string licenseName = "licenseName";
            string licenseUri = "https://example.com/license";
            string extensionKey = "x-extension";
            string extensionString = "value";
            string apiVersion = "apiVersion";
            string termsOfServiceUri = "https://example.com/termsOfService";
            string channelKey = "channel1";
            string channelDescription = "channelDescription";
            string operationDescription = "operationDescription";
            string operationId = "myOperation";
            string operationSummary = "operationSummary";
            string externalDocsUri = "https://example.com/externalDocs";
            string externalDocsDescription = "externalDocsDescription";
            string messageDescription = "messageDescription";
            string messageTitle = "messageTitle";
            string messageSummary = "messageSummary";
            string messageName = "messageName";
            string messageKeyOne = "messageKeyOne";
            string messageKeyTwo = "messageKeyTwo";
            string contentType = "contentType";
            string schemaFormat = "schemaFormat";
            string correlationLocation = "correlationLocation";
            string correlationDescription = "correlationDescription";
            string traitName = "traitName";
            string traitTitle = "traitTitle";
            string schemaTitle = "schemaTitle";
            string schemaDescription = "schemaDescription";
            string anyStringValue = "value";
            long anyLongValue = long.MaxValue;
            string exampleSummary = "exampleSummary";
            string exampleName = "exampleName";
            string traitDescription = "traitDescription";
            string traitSummary = "traitSummary";
            string tagName = "tagName";
            string tagDescription = "tagDescription";
            string documentId = "documentId";
            string serverKey = "myServer";
            string serverDescription = "serverDescription";
            string protocolVersion = "protocolVersion";
            string serverHost = "example.com/server";
            string protocol = "KafkaProtocol";
            string securirySchemeDescription = "securitySchemeDescription";
            string securitySchemeName = "securitySchemeName";
            string bearerFormat = "bearerFormat";
            string scheme = "scheme";
            string scopeKey = "securitySchemeScopeKey";
            string scopeValue = "securitySchemeScopeValue";
            string tokenUrl = "https://example.com/tokenUrl";
            string refreshUrl = "https://example.com/refresh";
            string authorizationUrl = "https://example.com/authorization";
            string requirementString = "requirementItem";

            var document = new AsyncApiDocument()
            {
                Id = documentId,
                Components = new AsyncApiComponents
                {
                    SecuritySchemes = new Dictionary<string, AsyncApiSecurityScheme>
                    {
                        {
                            securitySchemeName, new AsyncApiSecurityScheme
                            {
                                Description = securirySchemeDescription,
                                Name = securitySchemeName,
                                BearerFormat = bearerFormat,
                                Scheme = scheme,
                                Type = SecuritySchemeType.OAuth2,
                                Flows = new AsyncApiOAuthFlows
                                {
                                    Implicit = new AsyncApiOAuthFlow
                                    {
                                        AvailableScopes = new Dictionary<string, string>
                                        {
                                            { scopeKey, scopeValue },
                                        },
                                        TokenUrl = new Uri(tokenUrl),
                                        RefreshUrl = new Uri(refreshUrl),
                                        AuthorizationUrl = new Uri(authorizationUrl),
                                        Extensions = new Dictionary<string, IAsyncApiExtension>
                                        {
                                            { extensionKey, new AsyncApiAny(extensionString) },
                                        },
                                    },
                                },
                            }
                        },
                    },
                },
                Servers = new Dictionary<string, AsyncApiServer>
                {
                    {
                        serverKey, new AsyncApiServer
                        {
                            Description = serverDescription,
                            ProtocolVersion = protocolVersion,
                            Host = serverHost,
                            Protocol = protocol,
                            Security = new List<AsyncApiSecurityScheme>
                            {
                                new AsyncApiSecuritySchemeReference($"#/components/securitySchemes/{securitySchemeName}"),
                            },
                        }
                    },
                },
                Info = new AsyncApiInfo()
                {
                    Title = title,
                    Contact = new AsyncApiContact()
                    {
                        Name = contactName,
                        Email = contactEmail,
                        Url = new Uri(contactUri),
                    },
                    Description = description,
                    License = new AsyncApiLicense()
                    {
                        Name = licenseName,
                        Url = new Uri(licenseUri),
                        Extensions = new Dictionary<string, IAsyncApiExtension>
                        {
                            { extensionKey, new AsyncApiAny(extensionString) },
                        },
                    },
                    Version = apiVersion,
                    TermsOfService = new Uri(termsOfServiceUri),
                    Extensions = new Dictionary<string, IAsyncApiExtension>
                    {
                        { extensionKey, new AsyncApiAny(extensionString) },
                    },
                },
                Channels = new Dictionary<string, AsyncApiChannel>
                {
                    {
                        channelKey, new AsyncApiChannel
                        {
                            Description = channelDescription,
                            Messages = new Dictionary<string, AsyncApiMessage>()
                            {
                                {
                                    messageKeyOne, new AsyncApiMessage
                                    {
                                        Description = messageDescription,
                                        Title = messageTitle,
                                        Summary = messageSummary,
                                        Name = messageName,
                                        ContentType = contentType,
                                    }
                                },
                                {
                                    messageKeyTwo, new AsyncApiMessage
                                    {
                                        Description = messageDescription,
                                        Title = messageTitle,
                                        Summary = messageSummary,
                                        Name = messageName,
                                        ContentType = contentType,
                                        CorrelationId = new AsyncApiCorrelationId
                                        {
                                            Location = correlationLocation,
                                            Description = correlationDescription,
                                            Extensions = new Dictionary<string, IAsyncApiExtension>
                                            {
                                                { extensionKey, new AsyncApiAny(extensionString) },
                                            },
                                        },
                                        Traits = new List<AsyncApiMessageTrait>
                                        {
                                            new AsyncApiMessageTrait
                                            {
                                                Name = traitName,
                                                Title = traitTitle,
                                                Headers = new AsyncApiJsonSchema
                                                {
                                                    Title = schemaTitle,
                                                    WriteOnly = true,
                                                    Description = schemaDescription,
                                                    Examples = new List<AsyncApiAny>
                                                    {
                                                        new AsyncApiAny(new ExtensionClass
                                                        {
                                                            Key = anyStringValue,
                                                            OtherKey = anyLongValue,
                                                        }),
                                                    },
                                                },
                                                Examples = new List<AsyncApiMessageExample>
                                                {
                                                    new AsyncApiMessageExample
                                                    {
                                                        Summary = exampleSummary,
                                                        Name = exampleName,
                                                        Payload = new AsyncApiAny(new ExtensionClass
                                                        {
                                                            Key = anyStringValue,
                                                            OtherKey = anyLongValue,
                                                        }),
                                                        Extensions = new Dictionary<string, IAsyncApiExtension>
                                                        {
                                                            { extensionKey, new AsyncApiAny(extensionString) },
                                                        },
                                                    },
                                                },
                                                Description = traitDescription,
                                                Summary = traitSummary,
                                                Tags = new List<AsyncApiTag>
                                                {
                                                    new AsyncApiTag
                                                    {
                                                        Name = tagName,
                                                        Description = tagDescription,
                                                    },
                                                },
                                                ExternalDocs = new AsyncApiExternalDocumentation
                                                {
                                                    Url = new Uri(externalDocsUri),
                                                    Description = externalDocsDescription,
                                                },
                                                Extensions = new Dictionary<string, IAsyncApiExtension>
                                                {
                                                    { extensionKey, new AsyncApiAny(extensionString) },
                                                },
                                            },
                                        },
                                        Extensions = new Dictionary<string, IAsyncApiExtension>
                                        {
                                            { extensionKey, new AsyncApiAny(extensionString) },
                                        },
                                    }
                                },
                            },
                        }
                    },
                },
                Operations = new Dictionary<string, AsyncApiOperation>()
                {
                    {
                        operationId, new AsyncApiOperation()
                        {
                            Description = operationDescription,
                            Summary = operationSummary,
                            Channel = new AsyncApiChannelReference($"#/channels/{channelKey}"),
                            ExternalDocs = new AsyncApiExternalDocumentation
                            {
                                Url = new Uri(externalDocsUri),
                                Description = externalDocsDescription,
                            },
                            Messages = new List<AsyncApiMessageReference>
                            {
                                {
                                    new($"#/channels/messages/{messageKeyOne}")
                                },
                                {
                                    new($"#/channels/messages/{messageKeyTwo}")
                                },
                            },
                            Extensions = new Dictionary<string, IAsyncApiExtension>
                            {
                                { extensionKey, new AsyncApiAny(extensionString) },
                            },
                            Tags = new List<AsyncApiTag>
                            {
                                new AsyncApiTag
                                {
                                    Name = tagName,
                                    Description = tagDescription,
                                },
                            },
                            Traits = new List<AsyncApiOperationTrait>
                            {
                                new AsyncApiOperationTrait
                                {
                                    Description = traitDescription,
                                    Summary = traitSummary,
                                    Tags = new List<AsyncApiTag>
                                    {
                                        new AsyncApiTag
                                        {
                                            Name = tagName,
                                            Description = tagDescription,
                                        },
                                    },
                                    ExternalDocs = new AsyncApiExternalDocumentation
                                    {
                                        Url = new Uri(externalDocsUri),
                                        Description = externalDocsDescription,
                                    },
                                    Extensions = new Dictionary<string, IAsyncApiExtension>
                                    {
                                        { extensionKey, new AsyncApiAny(extensionString) },
                                    },
                                },
                            },
                        }
                    },
                },
            };

            var outputString = new StringWriter();
            var writer = new AsyncApiYamlWriter(outputString, AsyncApiWriterSettings.Default);

            // Act
            document.SerializeV2(writer);
            var actual = outputString.GetStringBuilder().ToString();

            // Assert
            actual.Should()
                  .BePlatformAgnosticEquivalentTo(expected);
        }

        [Test]
        public void V2_Read_WithAvroSchemaPayload_NoErrors()
        {
            // Arrange
            var yaml =
                """
                asyncapi: '2.6.0'
                info:
                  title: schema-validation-test
                  version: '1.0.0'
                  description: Async API for schema validation tests
                  contact:
                    name: Test
                    url: https://test.test/

                channels:
                  schema-validation-topic:
                    description: A topic to publish messages for testing Pulsar schema validation
                    publish:
                      message:
                        $ref: '#/components/messages/schema-validation-message'

                components:
                  messages:
                    schema-validation-message:
                      name: schema-validation-message
                      title: Message for schema validation testing that is a json object
                      summary: A test message is used for testing Pulsar schema validation
                      payload:
                        type: record
                        name: UserSignedUp
                        namespace: esp
                        doc: ESP Schema validation test
                        fields:
                          - name: userId
                            type: int
                          - name: userEmail
                            type: string
                      schemaFormat: 'application/vnd.apache.avro;version=1.9.0'
                """;

            // Act
            var result = new AsyncApiStringReader().Read(yaml, out var diagnostics);

            // Assert
            diagnostics.Errors.Should().HaveCount(0);
            result.Operations.Values.FirstOrDefault(op => op.Action == AsyncApiAction.Send)!.Messages.First().Payload.As<AsyncApiAvroSchema>().TryGetAs<AvroRecord>(out var record).Should().BeTrue();
            record.Name.Should().Be("UserSignedUp");
        }

        [Test]
        public void V2_Read_WithJsonSchemaReference_NoErrors()
        {
            // Arrange
            var yaml =
                """
                asyncapi: '2.6.0'
                info:
                  title: schema-validation-test
                  version: '1.0.0'
                  description: Async API for schema validation tests
                  contact:
                    name: Test
                    url: https://test.test/

                channels:
                  schema-validation-topic:
                    description: A topic to publish messages for testing Pulsar schema validation
                    publish:
                      message:
                        $ref: '#/components/messages/schema-validation-message'
                    subscribe:
                      message:
                        $ref: '#/components/messages/schema-validation-message'

                components:
                  schemas:
                    schema-validation-message-payload:
                      type: object
                      properties:
                        content:
                          type: string
                          description: Content of the message
                  messages:
                    schema-validation-message:
                      name: schema-validation-message
                      title: Message for schema validation testing that is a json object
                      summary: A test message is used for testing Pulsar schema validation
                      payload:
                        $ref: '#/components/schemas/schema-validation-message-payload'
                      contentType: application/json
                """;

            // Act
            var result = new AsyncApiStringReader().Read(yaml, out var diagnostics);

            // Assert
            diagnostics.Errors.Should().HaveCount(0);

            var message = result.Operations.Values.FirstOrDefault(op => op.Action == AsyncApiAction.Send)!.Messages.First();
            message.Title.Should().Be("Message for schema validation testing that is a json object");
            message.Payload.As<AsyncApiJsonSchema>().Properties.Should().HaveCount(1);
        }

        [Test]
        public void V2_Serialize_WithBindingReferences_SerializesDeserializes()
        {
            var expected =
                """
                asyncapi: 2.6.0
                info:
                  description: test description
                servers:
                  production:
                    url: example.com
                    protocol: pulsar+ssl
                    description: test description
                    bindings:
                      $ref: '#/components/serverBindings/bindings'
                channels:
                  testChannel:
                    $ref: '#/components/channels/otherchannel'
                components:
                  channels:
                    otherchannel:
                      publish:
                        description: test
                      bindings:
                        $ref: '#/components/channelBindings/bindings'
                  serverBindings:
                    bindings:
                      pulsar:
                        tenant: staging
                  channelBindings:
                    bindings:
                      pulsar:
                        namespace: users
                        persistence: persistent
                """;
            var doc = new AsyncApiDocument();
            doc.Info = new AsyncApiInfo()
            {
                Description = "test description",
            };
            doc.Servers.Add("production", new AsyncApiServer
            {
                Description = "test description",
                Protocol = "pulsar+ssl",
                Host = "example.com",
                Bindings = new AsyncApiBindingsReference<IServerBinding>("#/components/serverBindings/bindings"),
            });
            doc.Components = new AsyncApiComponents()
            {
                Channels = new Dictionary<string, AsyncApiChannel>()
                {
                    {
                        "otherchannel", new AsyncApiChannel()
                        {
                            Bindings = new AsyncApiBindingsReference<IChannelBinding>("#/components/channelBindings/bindings"),
                        }
                    },
                },
                ServerBindings = new Dictionary<string, AsyncApiBindings<IServerBinding>>()
                {
                    {
                        "bindings", new AsyncApiBindings<IServerBinding>()
                        {
                            new PulsarServerBinding()
                            {
                                Tenant = "staging",
                            },
                        }
                    },
                },
                ChannelBindings = new Dictionary<string, AsyncApiBindings<IChannelBinding>>()
                {
                    {
                        "bindings", new AsyncApiBindings<IChannelBinding>()
                        {
                            new PulsarChannelBinding()
                            {
                                Namespace = "users",
                                Persistence = AsyncAPI.Models.Bindings.Pulsar.Persistence.Persistent,
                            },
                        }
                    },
                },
                Operations = new Dictionary<string, AsyncApiOperation>()
                {
                    {
                        "operation", new AsyncApiOperation()
                        {
                            Description = "test",
                            Channel = new AsyncApiChannelReference("#/components/channels/otherchannel"),
                        }
                    },
                },
            };
            doc.Channels.Add(
                "testChannel",
                new AsyncApiChannelReference("#/components/channels/otherchannel"));
            var actual = doc.Serialize(AsyncApiVersion.AsyncApi2_0, AsyncApiFormat.Yaml);
            actual.Should().BePlatformAgnosticEquivalentTo(expected);

            var settings = new AsyncApiReaderSettings
            {
                Bindings = BindingsCollection.Pulsar,
            };
            var reader = new AsyncApiStringReader(settings);
            var deserialized = reader.Read(actual, out var diagnostic);
            var serverBindings = deserialized.Servers.First().Value.Bindings;
            serverBindings.TryGetValue<PulsarServerBinding>(out var binding);
            binding.Tenant.Should().Be("staging");

            var reserialized = deserialized.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);
            reserialized.Should().BePlatformAgnosticEquivalentTo(expected);
        }

        [Test]
        public void V2_SerializeV2_WithBindings_Serializes()
        {
            var expected = """
                asyncapi: 2.6.0
                info:
                  description: test description
                servers:
                  production:
                    url: example.com
                    protocol: pulsar+ssl
                    description: test description
                channels:
                  testChannel:
                    publish:
                      message:
                        bindings:
                          http:
                            headers:
                              description: this mah binding
                          kafka:
                            key:
                              description: this mah other binding
                    bindings:
                      kafka:
                        partitions: 2
                        replicas: 1
                """;

            var doc = new AsyncApiDocument();
            doc.Info = new AsyncApiInfo()
            {
                Description = "test description",
            };
            doc.Servers.Add("production", new AsyncApiServer
            {
                Description = "test description",
                Protocol = "pulsar+ssl",
                Host = "example.com",
            });
            doc.Channels.Add(
                "testChannel",
                new AsyncApiChannel
                {
                    Bindings = new AsyncApiBindings<IChannelBinding>
                    {
                        {
                            new KafkaChannelBinding
                            {
                                Partitions = 2,
                                Replicas = 1,
                            }
                        },
                    },
                });
            doc.Operations.Add("firstOperation", new AsyncApiOperation()
            {
                Messages = new List<AsyncApiMessageReference>
                {
                    new("#/components/messages/firstMessage"),
                },
            });

            doc.Components.Messages.Add("firstMessage", new AsyncApiMessage
            {
                Bindings = new AsyncApiBindings<IMessageBinding>
                {
                    {
                        new HttpMessageBinding
                        {
                            Headers = new AsyncApiJsonSchema
                            {
                                Description = "this mah binding",
                            },
                        }
                    },
                    {
                        new KafkaMessageBinding
                        {
                            Key = new AsyncApiJsonSchema
                            {
                                Description = "this mah other binding",
                            },
                        }
                    },
                },
            });

            var actual = doc.Serialize(AsyncApiVersion.AsyncApi2_0, AsyncApiFormat.Yaml);

            var settings = new AsyncApiReaderSettings
            {
                Bindings = BindingsCollection.All,
            };
            var reader = new AsyncApiStringReader(settings);
            var deserialized = reader.Read(actual, out var diagnostic);

            // Assert
            actual.Should()
                  .BePlatformAgnosticEquivalentTo(expected);
            Assert.AreEqual(2, deserialized.Operations.First().Value.Messages.First().Bindings.Count);

            var binding = deserialized.Operations.First().Value.Messages.First().Bindings.First();
            Assert.AreEqual("http", binding.Key);
            var httpBinding = binding.Value as HttpMessageBinding;

            Assert.AreEqual("this mah binding", httpBinding.Headers.Description);
        }
    }
}