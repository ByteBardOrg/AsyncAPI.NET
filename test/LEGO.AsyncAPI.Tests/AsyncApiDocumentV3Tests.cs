// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Tests
{
    using FluentAssertions;
    using LEGO.AsyncAPI.Bindings;
    using LEGO.AsyncAPI.Models;
    using LEGO.AsyncAPI.Readers;
    using NUnit.Framework;

    public class AsyncApiDocumentV3Tests : TestBase
    {
        [Test]
        public void V3_WithComplexInput_CanReSerialize()
        {
            // Arrange
            var expected =
                """
                asyncapi: 3.0.0
                info:
                  title: Streetlights Kafka API
                  version: 1.0.0
                  description: a description
                  license:
                    name: Apache 2.0
                    url: https://www.apache.org/licenses/LICENSE-2.0
                defaultContentType: application/json
                servers:
                  scram-connections:
                    host: test.mykafkacluster.org:18092
                    protocol: kafka-secure
                    description: Test broker secured with scramSha256
                    security:
                      - $ref: '#/components/securitySchemes/saslScram'
                    tags:
                      - name: env:test-scram
                        description: This environment is meant for running internal tests through scramSha256
                      - name: kind:remote
                        description: This server is a remote server. Not exposed by the application
                      - name: visibility:private
                        description: This resource is private and only available to certain users
                  mtls-connections:
                    host: test.mykafkacluster.org:28092
                    protocol: kafka-secure
                    description: Test broker secured with X509
                    security:
                      - $ref: '#/components/securitySchemes/certs'
                    tags:
                      - name: env:test-mtls
                        description: This environment is meant for running internal tests through mtls
                      - name: kind:remote
                        description: This server is a remote server. Not exposed by the application
                      - name: visibility:private
                        description: This resource is private and only available to certain users
                channels:
                  lightingMeasured:
                    address: 'smartylighting.streetlights.1.0.event.{streetlightId}.lighting.measured'
                    messages:
                      lightMeasured:
                        $ref: '#/components/messages/lightMeasured'
                    description: The topic on which measured values may be produced and consumed.
                    parameters:
                      streetlightId:
                        $ref: '#/components/parameters/streetlightId'
                  lightTurnOn:
                    address: 'smartylighting.streetlights.1.0.action.{streetlightId}.turn.on'
                    messages:
                      turnOn:
                        $ref: '#/components/messages/turnOnOff'
                    parameters:
                      streetlightId:
                        $ref: '#/components/parameters/streetlightId'
                  lightTurnOff:
                    address: 'smartylighting.streetlights.1.0.action.{streetlightId}.turn.off'
                    messages:
                      turnOff:
                        $ref: '#/components/messages/turnOnOff'
                    parameters:
                      streetlightId:
                        $ref: '#/components/parameters/streetlightId'
                  lightsDim:
                    address: 'smartylighting.streetlights.1.0.action.{streetlightId}.dim'
                    messages:
                      dimLight:
                        $ref: '#/components/messages/dimLight'
                    parameters:
                      streetlightId:
                        $ref: '#/components/parameters/streetlightId'
                operations:
                  receiveLightMeasurement:
                    action: receive
                    channel:
                      $ref: '#/channels/lightingMeasured'
                    summary: Inform about environmental lighting conditions of a particular streetlight.
                    traits:
                      - $ref: '#/components/operationTraits/kafka'
                    messages:
                      - $ref: '#/channels/lightingMeasured/messages/lightMeasured'
                  turnOn:
                    action: send
                    channel:
                      $ref: '#/channels/lightTurnOn'
                    traits:
                      - $ref: '#/components/operationTraits/kafka'
                    messages:
                      - $ref: '#/channels/lightTurnOn/messages/turnOn'
                  turnOff:
                    action: send
                    channel:
                      $ref: '#/channels/lightTurnOff'
                    traits:
                      - $ref: '#/components/operationTraits/kafka'
                    messages:
                      - $ref: '#/channels/lightTurnOff/messages/turnOff'
                  dimLight:
                    action: send
                    channel:
                      $ref: '#/channels/lightsDim'
                    traits:
                      - $ref: '#/components/operationTraits/kafka'
                    messages:
                      - $ref: '#/channels/lightsDim/messages/dimLight'
                components:
                  schemas:
                    lightMeasuredPayload:
                      schemaFormat: application/vnd.aai.asyncapi+json;version=3.0.0
                      schema:
                        type: object
                        properties:
                          lumens:
                            type: integer
                            description: Light intensity measured in lumens.
                            minimum: 0
                          sentAt:
                            $ref: '#/components/schemas/sentAt'
                    turnOnOffPayload:
                      schemaFormat: application/vnd.aai.asyncapi+json;version=3.0.0
                      schema:
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
                      schemaFormat: application/vnd.aai.asyncapi+json;version=3.0.0
                      schema:
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
                      schemaFormat: application/vnd.aai.asyncapi+json;version=3.0.0
                      schema:
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
                        schemaFormat: application/vnd.aai.asyncapi+json;version=3.0.0
                        schema:
                          type: object
                          properties:
                            my-app-header:
                              type: integer
                              maximum: 100
                              minimum: 0
                """;

            var reader = new AsyncApiStringReader(new AsyncApiReaderSettings { Bindings = BindingsCollection.Kafka });

            // Act
            var document = reader.Read(expected, out var diagnostics);
            var reserialized = document.SerializeAsYaml(AsyncApiVersion.AsyncApi3_0);

            // Assert
            diagnostics.Errors.Should().BeEmpty();
            diagnostics.Warnings.Should().BeEmpty();
            reserialized.Should().BePlatformAgnosticEquivalentTo(expected);
        }
    }
}
