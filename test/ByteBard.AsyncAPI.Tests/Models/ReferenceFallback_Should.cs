namespace ByteBard.AsyncAPI.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json.Nodes;
    using ByteBard.AsyncAPI.Bindings;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers;
    using FluentAssertions;
    using NUnit.Framework;

    /// <summary>
    /// Local references that do not point at a registered component (for example into another channel, operation
    /// or message) are resolved from the document itself. They must resolve, survive writing as either version,
    /// and read back the same.
    /// </summary>
    public class ReferenceFallback_Should
    {
        private const string Marker = "MARK";

        private const string V2Header =
            """
            asyncapi: 2.6.0
            info:
              title: test
              version: 1.0.0

            """;

        private const string V3Header =
            """
            asyncapi: 3.0.0
            info:
              title: test
              version: 1.0.0

            """;

        private static readonly Dictionary<string, FallbackCase> Cases = new()
        {
            ["v2 nested schema"] = new(V2Header + """
                channels:
                  a:
                    publish:
                      message:
                        payload:
                          $ref: '#/components/schemas/B'
                components:
                  schemas:
                    B:
                      type: object
                      properties:
                        y:
                          $ref: '#/components/schemas/C/properties/z'
                    C:
                      type: object
                      properties:
                        z:
                          type: integer
                          description: MARK
                """, d => (d.Components.Schemas["B"].Schema as AsyncApiJsonSchema)?.Properties["y"].Description),

            ["v2 json payload"] = new(V2Header + """
                channels:
                  a:
                    publish:
                      message:
                        payload:
                          $ref: '#/components/messages/m/payload'
                components:
                  messages:
                    m:
                      payload:
                        type: string
                        description: MARK
                """, d => (MessageOf(d, "a")?.Payload?.Schema as AsyncApiJsonSchema)?.Description),

            ["v2 avro payload"] = new(V2Header + """
                channels:
                  a:
                    publish:
                      message:
                        schemaFormat: application/vnd.apache.avro;version=1.9.0
                        payload:
                          $ref: '#/components/messages/m/payload'
                components:
                  messages:
                    m:
                      schemaFormat: application/vnd.apache.avro;version=1.9.0
                      payload:
                        type: record
                        name: MARK
                        fields:
                          - name: id
                            type: string
                """, d => (MessageOf(d, "a")?.Payload?.Schema as AvroRecord)?.Name),

            ["v2 headers"] = new(V2Header + """
                channels:
                  a:
                    publish:
                      message:
                        headers:
                          $ref: '#/components/messages/m/headers'
                components:
                  messages:
                    m:
                      headers:
                        type: object
                        description: MARK
                """, d => (MessageOf(d, "a")?.Headers?.Schema as AsyncApiJsonSchema)?.Description),

            ["v2 message in an operation"] = new(V2Header + """
                channels:
                  a:
                    publish:
                      message:
                        name: MARK
                        payload:
                          type: string
                  b:
                    subscribe:
                      message:
                        $ref: '#/channels/a/publish/message'
                """, d => MessageOf(d, "b")?.Name),

            ["v2 channel by address"] = new(V2Header + """
                channels:
                  user/signedup:
                    description: MARK
                    publish:
                      message:
                        payload:
                          type: string
                  other:
                    $ref: '#/channels/user~1signedup'
                """, d => d.Channels.TryGetValue("other", out var other) ? other.Description : null),

            ["v2 channel parameter"] = new(V2Header + """
                channels:
                  a/{id}:
                    parameters:
                      id:
                        description: MARK
                        schema:
                          type: string
                  b/{id}:
                    parameters:
                      id:
                        $ref: '#/channels/a~1{id}/parameters/id'
                """, d => d.Channels.Values.FirstOrDefault(c => c.Address == "b/{id}")?.Parameters["id"].Description),

            ["v2 server variable"] = new(V2Header + """
                servers:
                  prod:
                    url: '{host}'
                    protocol: kafka
                    variables:
                      host:
                        default: a.example.com
                        description: MARK
                  dev:
                    url: '{host}'
                    protocol: kafka
                    variables:
                      host:
                        $ref: '#/servers/prod/variables/host'
                channels:
                  a: {}
                """, d => d.Servers["dev"].Variables["host"].Description),

            ["v2 correlation id"] = new(V2Header + """
                channels:
                  a:
                    publish:
                      message:
                        correlationId:
                          $ref: '#/components/messages/m/correlationId'
                components:
                  messages:
                    m:
                      correlationId:
                        location: '$message.header#/id'
                        description: MARK
                """, d => MessageOf(d, "a")?.CorrelationId?.Description),

            ["v2 operation trait"] = new(V2Header + """
                channels:
                  a:
                    publish:
                      traits:
                        - description: MARK
                  b:
                    subscribe:
                      traits:
                        - $ref: '#/channels/a/publish/traits/0'
                """, d => OperationOf(d, "b")?.Traits.FirstOrDefault()?.Description),

            ["v2 message trait"] = new(V2Header + """
                channels:
                  a:
                    publish:
                      message:
                        traits:
                          - $ref: '#/components/messages/m/traits/0'
                components:
                  messages:
                    m:
                      traits:
                        - description: MARK
                """, d => MessageOf(d, "a")?.Traits.FirstOrDefault()?.Description),

            ["v2 channel bindings"] = new(V2Header + """
                channels:
                  a:
                    bindings:
                      ws:
                        method: GET
                  b:
                    bindings:
                      $ref: '#/channels/a/bindings'
                """, d => d.Channels["b"].Bindings?.Count == 1 ? Marker : null),

            ["v2 operation bindings"] = new(V2Header + """
                channels:
                  a:
                    publish:
                      bindings:
                        kafka:
                          clientId:
                            type: string
                  b:
                    subscribe:
                      bindings:
                        $ref: '#/channels/a/publish/bindings'
                """, d => OperationOf(d, "b")?.Bindings?.Count == 1 ? Marker : null),

            ["v2 message bindings"] = new(V2Header + """
                channels:
                  a:
                    publish:
                      message:
                        bindings:
                          $ref: '#/components/messages/m/bindings'
                components:
                  messages:
                    m:
                      bindings:
                        kafka:
                          key:
                            type: string
                """, d => MessageOf(d, "a")?.Bindings?.Count == 1 ? Marker : null),

            ["v2 server bindings"] = new(V2Header + """
                servers:
                  prod:
                    url: a.example.com
                    protocol: kafka
                    bindings:
                      kafka:
                        schemaRegistryUrl: 'https://registry.example.com'
                  dev:
                    url: b.example.com
                    protocol: kafka
                    bindings:
                      $ref: '#/servers/prod/bindings'
                channels:
                  a: {}
                """, d => d.Servers["dev"].Bindings?.Count == 1 ? Marker : null),

            ["v3 nested schema"] = new(V3Header + """
                channels:
                  a:
                    address: a
                components:
                  schemas:
                    B:
                      type: object
                      properties:
                        y:
                          $ref: '#/components/schemas/C/properties/z'
                    C:
                      type: object
                      properties:
                        z:
                          type: integer
                          description: MARK
                """, d => (d.Components.Schemas["B"].Schema as AsyncApiJsonSchema)?.Properties["y"].Description),

            ["v3 json payload"] = new(V3Header + MessageOnChannelA("""
                        payload:
                          $ref: '#/components/messages/m/payload'
                """, """
                      payload:
                        type: string
                        description: MARK
                """), d => (MessageOf(d, "a")?.Payload?.Schema as AsyncApiJsonSchema)?.Description),

            ["v3 avro payload"] = new(V3Header + MessageOnChannelA("""
                        payload:
                          $ref: '#/components/messages/m/payload'
                """, """
                      payload:
                        schemaFormat: application/vnd.apache.avro;version=1.9.0
                        schema:
                          type: record
                          name: MARK
                          fields:
                            - name: id
                              type: string
                """), d => (MessageOf(d, "a")?.Payload?.Schema as AvroRecord)?.Name),

            ["v3 headers"] = new(V3Header + MessageOnChannelA("""
                        headers:
                          $ref: '#/components/messages/m/headers'
                """, """
                      headers:
                        type: object
                        description: MARK
                """), d => (MessageOf(d, "a")?.Headers?.Schema as AsyncApiJsonSchema)?.Description),

            ["v3 correlation id"] = new(V3Header + MessageOnChannelA("""
                        correlationId:
                          $ref: '#/components/messages/m/correlationId'
                """, """
                      correlationId:
                        location: '$message.header#/id'
                        description: MARK
                """), d => MessageOf(d, "a")?.CorrelationId?.Description),

            ["v3 message trait"] = new(V3Header + MessageOnChannelA("""
                        traits:
                          - $ref: '#/components/messages/m/traits/0'
                """, """
                      traits:
                        - description: MARK
                """), d => MessageOf(d, "a")?.Traits.FirstOrDefault()?.Description),

            ["v3 message bindings"] = new(V3Header + MessageOnChannelA("""
                        bindings:
                          $ref: '#/components/messages/m/bindings'
                """, """
                      bindings:
                        kafka:
                          key:
                            type: string
                """), d => MessageOf(d, "a")?.Bindings?.Count == 1 ? Marker : null),

            ["v3 channel by key"] = new(V3Header + """
                channels:
                  signup:
                    address: user/signedup
                    description: MARK
                  other:
                    $ref: '#/channels/signup'
                """, d => d.Channels.TryGetValue("other", out var other) ? other.Description : null),

            ["v3 channel bindings"] = new(V3Header + """
                channels:
                  a:
                    address: a
                    bindings:
                      ws:
                        method: GET
                  b:
                    address: b
                    bindings:
                      $ref: '#/channels/a/bindings'
                """, d => d.Channels["b"].Bindings?.Count == 1 ? Marker : null),

            ["v3 operation bindings"] = new(V3Header + TwoOperations("""
                    bindings:
                      kafka:
                        clientId:
                          type: string
                """, """
                    bindings:
                      $ref: '#/operations/op1/bindings'
                """), d => OperationOf(d, "b")?.Bindings?.Count == 1 ? Marker : null),

            ["v3 server bindings"] = new(V3Header + """
                servers:
                  prod:
                    host: a.example.com
                    protocol: kafka
                    bindings:
                      kafka:
                        schemaRegistryUrl: 'https://registry.example.com'
                  dev:
                    host: b.example.com
                    protocol: kafka
                    bindings:
                      $ref: '#/servers/prod/bindings'
                channels:
                  a:
                    address: a
                """, d => d.Servers["dev"].Bindings?.Count == 1 ? Marker : null),

            ["v3 operation trait"] = new(V3Header + TwoOperations("""
                    traits:
                      - description: MARK
                """, """
                    traits:
                      - $ref: '#/operations/op1/traits/0'
                """), d => OperationOf(d, "b")?.Traits.FirstOrDefault()?.Description),

            ["v3 server variable"] = new(V3Header + """
                servers:
                  prod:
                    host: '{h}'
                    protocol: kafka
                    variables:
                      h:
                        default: a.example.com
                        description: MARK
                  dev:
                    host: '{h}'
                    protocol: kafka
                    variables:
                      h:
                        $ref: '#/servers/prod/variables/h'
                channels:
                  a:
                    address: a
                """, d => d.Servers["dev"].Variables["h"].Description),

            ["v3 tag"] = new(V3Header + TwoOperations("""
                    tags:
                      - name: t
                        description: MARK
                """, """
                    tags:
                      - $ref: '#/operations/op1/tags/0'
                """), d => OperationOf(d, "b")?.Tags.FirstOrDefault()?.Description),

            ["v3 external docs"] = new(V3Header + TwoOperations("""
                    externalDocs:
                      url: https://example.com
                      description: MARK
                """, """
                    externalDocs:
                      $ref: '#/operations/op1/externalDocs'
                """), d => OperationOf(d, "b")?.ExternalDocs?.Description),

            ["v3 reply"] = new(V3Header + TwoOperations("""
                    reply:
                      address:
                        location: '$message.header#/replyTo'
                        description: MARK
                """, """
                    reply:
                      $ref: '#/operations/op1/reply'
                """), d => OperationOf(d, "b")?.Reply?.Address?.Description, V3Only: true),

            ["v3 reply address"] = new(V3Header + TwoOperations("""
                    reply:
                      address:
                        location: '$message.header#/replyTo'
                        description: MARK
                """, """
                    reply:
                      address:
                        $ref: '#/operations/op1/reply/address'
                """), d => OperationOf(d, "b")?.Reply?.Address?.Description, V3Only: true),
        };

        /// <summary>
        /// The references each case writes as 2.x and as 3.x: those that resolve in that version are kept, the others are inlined.
        /// </summary>
        private static readonly Dictionary<string, (string[] V2, string[] V3)> WrittenReferences = new()
        {
            ["v2 nested schema"] = (
                new[] { "#/components/messages/anonymous-message-1", "#/components/schemas/C/properties/z", "#/components/schemas/B" },
                new[] { "#/components/messages/anonymous-message-1", "#/channels/a", "#/channels/a/messages/anonymous-message-1", "#/components/schemas/C/properties/z", "#/components/schemas/B" }),
            ["v2 json payload"] = (
                new[] { "#/components/messages/anonymous-message-1", "#/components/messages/m/payload" },
                new[] { "#/components/messages/anonymous-message-1", "#/channels/a", "#/channels/a/messages/anonymous-message-1", "#/components/messages/m/payload" }),
            ["v2 avro payload"] = (
                new[] { "#/components/messages/anonymous-message-1", "#/components/messages/m/payload" },
                new[] { "#/components/messages/anonymous-message-1", "#/channels/a", "#/channels/a/messages/anonymous-message-1", "#/components/messages/m/payload" }),
            ["v2 headers"] = (
                new[] { "#/components/messages/anonymous-message-1", "#/components/messages/m/headers" },
                new[] { "#/components/messages/anonymous-message-1", "#/channels/a", "#/channels/a/messages/anonymous-message-1", "#/components/messages/m/headers" }),
            ["v2 message in an operation"] = (
                new[] { "#/components/messages/anonymous-message-1", "#/channels/a/publish/message" },
                new[] { "#/components/messages/anonymous-message-1", "#/channels/a", "#/channels/a/messages/anonymous-message-1", "#/channels/b", "#/channels/b/messages/message" }),
            ["v2 channel by address"] = (
                new[] { "#/components/messages/anonymous-message-1", "#/channels/user~1signedup" },
                new[] { "#/components/messages/anonymous-message-1", "#/channels/usersignedup", "#/channels/usersignedup", "#/channels/usersignedup/messages/anonymous-message-1" }),
            ["v2 channel parameter"] = (
                new[] { "#/channels/a~1{id}/parameters/id" },
                Array.Empty<string>()),
            ["v2 server variable"] = (
                new[] { "#/servers/prod/variables/host" },
                new[] { "#/servers/prod/variables/host" }),
            ["v2 correlation id"] = (
                new[] { "#/components/messages/anonymous-message-1", "#/components/messages/m/correlationId" },
                new[] { "#/components/messages/anonymous-message-1", "#/channels/a", "#/channels/a/messages/anonymous-message-1", "#/components/messages/m/correlationId" }),
            ["v2 operation trait"] = (
                new[] { "#/channels/a/publish/traits/0" },
                new[] { "#/channels/a", "#/channels/b" }),
            ["v2 message trait"] = (
                new[] { "#/components/messages/anonymous-message-1", "#/components/messages/m/traits/0" },
                new[] { "#/components/messages/anonymous-message-1", "#/channels/a", "#/channels/a/messages/anonymous-message-1", "#/components/messages/m/traits/0" }),
            ["v2 channel bindings"] = (
                new[] { "#/channels/a/bindings" },
                new[] { "#/channels/a/bindings" }),
            ["v2 operation bindings"] = (
                new[] { "#/channels/a/publish/bindings" },
                new[] { "#/channels/a", "#/channels/b" }),
            ["v2 message bindings"] = (
                new[] { "#/components/messages/anonymous-message-1", "#/components/messages/m/bindings" },
                new[] { "#/components/messages/anonymous-message-1", "#/channels/a", "#/channels/a/messages/anonymous-message-1", "#/components/messages/m/bindings" }),
            ["v2 server bindings"] = (
                new[] { "#/servers/prod/bindings" },
                new[] { "#/servers/prod/bindings" }),
            // The 3.x writer wraps component schemas in a Multi Format Schema Object, so the pointer into C no longer exists and is inlined.
            ["v3 nested schema"] = (
                new[] { "#/components/schemas/C/properties/z" },
                Array.Empty<string>()),
            ["v3 json payload"] = (
                new[] { "#/components/messages/m/payload" },
                new[] { "#/components/messages/m/payload", "#/channels/a", "#/channels/a/messages/msg" }),
            ["v3 avro payload"] = (
                new[] { "#/components/messages/m/payload" },
                new[] { "#/components/messages/m/payload", "#/channels/a", "#/channels/a/messages/msg" }),
            // The 2.x writer writes headers as their schema, which inlines the referenced headers.
            ["v3 headers"] = (
                Array.Empty<string>(),
                new[] { "#/components/messages/m/headers", "#/channels/a", "#/channels/a/messages/msg" }),
            ["v3 correlation id"] = (
                new[] { "#/components/messages/m/correlationId" },
                new[] { "#/components/messages/m/correlationId", "#/channels/a", "#/channels/a/messages/msg" }),
            ["v3 message trait"] = (
                new[] { "#/components/messages/m/traits/0" },
                new[] { "#/components/messages/m/traits/0", "#/channels/a", "#/channels/a/messages/msg" }),
            ["v3 message bindings"] = (
                new[] { "#/components/messages/m/bindings" },
                new[] { "#/components/messages/m/bindings", "#/channels/a", "#/channels/a/messages/msg" }),
            ["v3 channel by key"] = (
                new[] { "#/channels/user~1signedup" },
                new[] { "#/channels/signup" }),
            ["v3 channel bindings"] = (
                new[] { "#/channels/a/bindings" },
                new[] { "#/channels/a/bindings" }),
            ["v3 operation bindings"] = (
                Array.Empty<string>(),
                new[] { "#/channels/a", "#/channels/b", "#/operations/op1/bindings" }),
            ["v3 server bindings"] = (
                new[] { "#/servers/prod/bindings" },
                new[] { "#/servers/prod/bindings" }),
            ["v3 operation trait"] = (
                Array.Empty<string>(),
                new[] { "#/channels/a", "#/channels/b", "#/operations/op1/traits/0" }),
            ["v3 server variable"] = (
                new[] { "#/servers/prod/variables/h" },
                new[] { "#/servers/prod/variables/h" }),
            ["v3 tag"] = (
                Array.Empty<string>(),
                new[] { "#/channels/a", "#/channels/b", "#/operations/op1/tags/0" }),
            ["v3 external docs"] = (
                Array.Empty<string>(),
                new[] { "#/channels/a", "#/channels/b", "#/operations/op1/externalDocs" }),
            ["v3 reply"] = (
                Array.Empty<string>(),
                new[] { "#/channels/a", "#/channels/b", "#/operations/op1/reply" }),
            ["v3 reply address"] = (
                Array.Empty<string>(),
                new[] { "#/channels/a", "#/channels/b", "#/operations/op1/reply/address" }),
        };

        public static IEnumerable<string> CaseNames => Cases.Keys;

        [TestCaseSource(nameof(CaseNames))]
        public void Read_ResolvesTheReference(string name)
        {
            var doc = Read(Cases[name].Yaml);

            Cases[name].Inspect(doc).Should().Be(Marker);
        }

        [TestCaseSource(nameof(CaseNames))]
        public void WriteAsV2_ReadsBackTheSame(string name)
        {
            if (Cases[name].V3Only)
            {
                Assert.Ignore("AsyncAPI 2.x has no equivalent of this object.");
            }

            this.AssertRoundTrip(name, AsyncApiVersion.AsyncApi2_0);
        }

        [TestCaseSource(nameof(CaseNames))]
        public void WriteAsV3_ReadsBackTheSame(string name)
        {
            this.AssertRoundTrip(name, AsyncApiVersion.AsyncApi3_0);
        }

        private void AssertRoundTrip(string name, AsyncApiVersion version)
        {
            var document = Read(Cases[name].Yaml);
            var written = document.SerializeAsYaml(version);

            var expected = version == AsyncApiVersion.AsyncApi2_0 ? WrittenReferences[name].V2 : WrittenReferences[name].V3;
            References(document.SerializeAsJson(version)).Should().Equal(expected, "only references that resolve should be kept:\n{0}", written);

            var reread = Read(written);

            Cases[name].Inspect(reread).Should().Be(Marker, "the reference should survive:\n{0}", written);
        }

        private static AsyncApiDocument Read(string yaml)
        {
            var settings = new AsyncApiReaderSettings { Bindings = BindingsCollection.All };
            var doc = new AsyncApiStringReader(settings).Read(yaml, out var diagnostic);

            diagnostic.Errors.Should().BeEmpty("reading should not report errors:\n{0}", yaml);
            return doc;
        }

        /// <summary>
        /// The "$ref" values in a written document, in the order they are written.
        /// </summary>
        internal static List<string> References(string json)
        {
            var references = new List<string>();
            Collect(JsonNode.Parse(json));
            return references;

            void Collect(JsonNode node)
            {
                switch (node)
                {
                    case JsonArray array:
                        foreach (var item in array)
                        {
                            Collect(item);
                        }

                        break;
                    case JsonObject map:
                        foreach (var property in map)
                        {
                            if (property.Key == "$ref")
                            {
                                references.Add(property.Value.GetValue<string>());
                            }
                            else
                            {
                                Collect(property.Value);
                            }
                        }

                        break;
                }
            }
        }

        private static AsyncApiMessage MessageOf(AsyncApiDocument doc, string channel)
        {
            return doc.Channels.TryGetValue(channel, out var c) ? c.Messages?.Values.FirstOrDefault() : null;
        }

        private static AsyncApiOperation OperationOf(AsyncApiDocument doc, string channelAddress)
        {
            return doc.Operations.Values.FirstOrDefault(o => o.Channel?.Address == channelAddress);
        }

        /// <summary>
        /// A message on channel "a" and a message "m" in the components for it to reference. The message needs an
        /// operation, since 2.x only has messages in operations.
        /// </summary>
        private static string MessageOnChannelA(string message, string componentMessage)
        {
            return
                $"""
                channels:
                  a:
                    address: a
                    messages:
                      msg:
                {message}
                operations:
                  op:
                    action: send
                    channel:
                      $ref: '#/channels/a'
                    messages:
                      - $ref: '#/channels/a/messages/msg'
                components:
                  messages:
                    m:
                {componentMessage}
                """;
        }

        /// <summary>
        /// Two operations on separate channels, since 2.x has one operation per action and channel.
        /// </summary>
        private static string TwoOperations(string op1, string op2)
        {
            return
                $"""
                channels:
                  a:
                    address: a
                  b:
                    address: b
                operations:
                  op1:
                    action: send
                    channel:
                      $ref: '#/channels/a'
                {op1}
                  op2:
                    action: send
                    channel:
                      $ref: '#/channels/b'
                {op2}
                """;
        }

        private sealed record FallbackCase(string Yaml, Func<AsyncApiDocument, string> Inspect, bool V3Only = false);
    }
}
