namespace ByteBard.AsyncAPI.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers;
    using ByteBard.AsyncAPI.Readers.Interface;
    using ByteBard.AsyncAPI.Readers.V2;
    using FluentAssertions;
    using NUnit.Framework;

    public class AsyncApiReference_Should : TestBase
    {
        [Test]
        public void V2_ReferencePointers()
        {
            var diag = new AsyncApiDiagnostic();
            var versionService = new AsyncApiV2VersionService(diag);
            var externalFragment = versionService.ConvertToAsyncApiReference("https://github.com/test/test#whatever", ReferenceType.None);
            var internalFragment = versionService.ConvertToAsyncApiReference("#/components/servers/server1", ReferenceType.None);
            var localFile = versionService.ConvertToAsyncApiReference("./local/some/folder/whatever.yaml", ReferenceType.None);
            var externalFile = versionService.ConvertToAsyncApiReference("https://github.com/test/whatever.yaml", ReferenceType.None);

            externalFragment.ExternalResource.Should().Be("https://github.com/test/test");
            externalFragment.FragmentId.Should().Be("whatever");
            externalFragment.Reference.Should().Be("https://github.com/test/test#whatever");
            externalFragment.IsFragment.Should().BeTrue();
            externalFragment.IsExternal.Should().BeTrue();

            internalFragment.ExternalResource.Should().BeNull();
            internalFragment.FragmentId.Should().Be("/components/servers/server1");
            internalFragment.Reference.Should().Be("#/components/servers/server1");
            internalFragment.IsFragment.Should().BeTrue();
            internalFragment.IsExternal.Should().BeFalse();

            localFile.ExternalResource.Should().Be("./local/some/folder/whatever.yaml");
            localFile.Reference.Should().Be("./local/some/folder/whatever.yaml");
            localFile.FragmentId.Should().Be(null);
            localFile.IsFragment.Should().BeFalse();

            externalFile.ExternalResource.Should().Be("https://github.com/test/whatever.yaml");
            externalFile.Reference.Should().Be("https://github.com/test/whatever.yaml");
            externalFile.FragmentId.Should().Be(null);
            externalFile.IsFragment.Should().BeFalse();
        }

        [Test]
        public void V2_Reference()
        {
            var json =
                """
        {
          "asyncapi": "2.6.0",
          "info": { },
          "servers": {
            "production": {
              "$ref": "https://github.com/test/test#whatever"
            }
          }
        }
        """;

            var doc = new AsyncApiStringReader().Read(json, out var diag);
            var reference = doc.Servers.First().Value as AsyncApiServerReference;
            reference.Reference.ExternalResource.Should().Be("https://github.com/test/test");
            reference.Reference.FragmentId.Should().Be("whatever");
            reference.Reference.IsFragment.Should().BeTrue();
        }

        [Test]
        public void V2_ExternalFragmentReference_ResolvesFragtment()
        {
            var externalJson =
                """
                {
                    "servers": [
                        {
                            "url": "production.gigantic-server.com:443",
                            "protocol": "wss",
                            "protocolVersion": "1.0.0",
                            "description": "The production API server",
                            "variables": {
                                "username": {
                                    "default": "demo",
                                    "description": "This value is assigned by the service provider"
                                },
                                "password": {
                                    "default": "demo",
                                    "description": "This value is assigned by the service provider"
                                }
                            }
                        }
                    ]
                }
                """;

            var json =
                """
                 {
                   "asyncapi": "2.6.0",
                   "info": { },
                   "servers": {
                     "production": {
                       "$ref": "whatever/whatever.json#/servers/0"
                     }
                   }
                 }
                 """;

            var doc = new AsyncApiStringReader(new AsyncApiReaderSettings { ReferenceResolution = ReferenceResolutionSetting.ResolveAllReferences, ExternalReferenceLoader = new MockStringLoader(externalJson) }).Read(json, out var diag);
            var reference = doc.Servers.First().Value as AsyncApiServerReference;
            reference.Reference.FragmentId.Should().Be("/servers/0");
            reference.Reference.IsFragment.Should().BeTrue();
            reference.Host.Should().Be("production.gigantic-server.com:443");
            reference.Protocol.Should().Be("wss");
        }

        [Test]
        public void V2_ServerReference_WithComponentReference_ResolvesReference()
        {
            var json =
                """
                {
                  "asyncapi": "2.6.0",
                  "info": { },
                  "servers": {
                    "production": {
                      "$ref": "#/components/servers/whatever"
                    }
                  },
                  "components": {
                    "servers": {
                        "whatever": {
                          "url": "production.gigantic-server.com:443",
                          "protocol": "wss",
                          "protocolVersion": "1.0.0",
                          "description": "The production API server",
                          "variables": {
                            "username": {
                              "default": "demo",
                              "description": "This value is assigned by the service provider"
                            },
                            "password": {
                              "default": "demo",
                              "description": "This value is assigned by the service provider"
                            }
                          }
                        }
                    }
                  }
                }
                """;

            var doc = new AsyncApiStringReader().Read(json, out var diag);
            var reference = doc.Servers.First().Value as AsyncApiServerReference;
            reference.Reference.ExternalResource.Should().BeNull();
            reference.Reference.FragmentId.Should().Be("/components/servers/whatever");
            reference.Reference.IsFragment.Should().BeTrue();
            reference.Host.Should().Be("production.gigantic-server.com:443");
            reference.Protocol.Should().Be("wss");

        }

        [Test]
        public void V2_AsyncApiReference_WithExternalFragmentUriReference_AllowReference()
        {
            // Arrange
            var actual = """
                payload:
                  $ref: http://example.com/some-resource#/path/to/external/fragment
                """;
            var reader = new AsyncApiStringReader();

            // Act
            var deserialized = reader.ReadFragment<AsyncApiMessage>(actual, AsyncApiVersion.AsyncApi2_0, out var diagnostic);

            // Assert
            diagnostic.Errors.Should().BeEmpty();
            var payload = deserialized.Payload.Schema.As<AsyncApiJsonSchemaReference>();
            payload.UnresolvedReference.Should().BeTrue();

            var reference = payload.Reference;
            reference.ExternalResource.Should().Be("http://example.com/some-resource");
            reference.FragmentId.Should().Be("/path/to/external/fragment");
            reference.IsFragment.Should().BeTrue();
            reference.IsExternal.Should().BeTrue();
            reference.Type.Should().Be(ReferenceType.Schema);
            var expected = deserialized.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);
            actual.Should()
                  .BePlatformAgnosticEquivalentTo(expected);
        }

        [Test]
        public void V2_AsyncApiReference_WithFragmentReference_AllowReference()
        {
            // Arrange
            var actual = """
                payload:
                  $ref: /fragments/myFragment
                """;
            var reader = new AsyncApiStringReader();

            // Act
            var deserialized = reader.ReadFragment<AsyncApiMessage>(actual, AsyncApiVersion.AsyncApi2_0, out var diagnostic);

            // Assert
            diagnostic.Errors.Should().BeEmpty();
            var payload = deserialized.Payload.Schema.As<AsyncApiJsonSchemaReference>();
            payload.UnresolvedReference.Should().BeTrue();

            var reference = payload.Reference;
            reference.Type.Should().Be(ReferenceType.Schema);
            reference.ExternalResource.Should().Be("/fragments/myFragment");
            reference.FragmentId.Should().BeNull();
            reference.IsFragment.Should().BeFalse();
            reference.IsExternal.Should().BeTrue();
            var expected = deserialized.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);
            actual.Should()
                  .BePlatformAgnosticEquivalentTo(expected);
        }

        [Test]
        public void V2_AsyncApiReference_WithExternalFragmentReference_AllowReference()
        {
            // Arrange
            var actual = """
                payload:
                  $ref: ./myjsonfile.json#/fragment
                """;
            var reader = new AsyncApiStringReader();

            // Act
            var deserialized = reader.ReadFragment<AsyncApiMessage>(actual, AsyncApiVersion.AsyncApi2_0, out var diagnostic);

            // Assert
            diagnostic.Errors.Should().BeEmpty();
            var payload = deserialized.Payload.Schema.As<AsyncApiJsonSchemaReference>();
            var reference = payload.Reference;
            reference.ExternalResource.Should().Be("./myjsonfile.json");
            reference.FragmentId.Should().Be("/fragment");
            reference.IsFragment.Should().BeTrue();
            reference.IsExternal.Should().BeTrue();

            var expected = deserialized.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);
            actual.Should()
                  .BePlatformAgnosticEquivalentTo(expected);
        }

        [Test]
        public void V2_AsyncApiReference_WithExternalComponentReference_AllowReference()
        {
            // Arrange
            var actual = """
                payload:
                  $ref: ./someotherdocument.json#/components/schemas/test
                """;
            var reader = new AsyncApiStringReader();

            // Act
            var deserialized = reader.ReadFragment<AsyncApiMessage>(actual, AsyncApiVersion.AsyncApi2_0, out var diagnostic);

            // Assert
            diagnostic.Errors.Should().BeEmpty();
            var payload = deserialized.Payload.Schema.As<AsyncApiJsonSchemaReference>();
            var reference = payload.Reference;
            reference.ExternalResource.Should().Be("./someotherdocument.json");
            reference.Type.Should().Be(ReferenceType.Schema);
            reference.FragmentId.Should().Be("/components/schemas/test");
            reference.IsFragment.Should().BeTrue();
            reference.IsExternal.Should().BeTrue();

            var expected = deserialized.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);
            actual.Should()
                  .BePlatformAgnosticEquivalentTo(expected);
        }

        [Test]
        public void V2_AsyncApiDocument_WithInternalComponentReference_ResolvesReference()
        {
            // Arrange
            var actual = """
                asyncapi: 2.6.0
                info:
                  title: My AsyncAPI Document
                  version: 1.0.0
                channels:
                  myChannel:
                    $ref: '#/components/channels/myChannel' 
                components:
                  channels: 
                    myChannel:
                      description: customDescription
                """;

            var settings = new AsyncApiReaderSettings()
            {
                ReferenceResolution = ReferenceResolutionSetting.ResolveInternalReferences,
            };
            var reader = new AsyncApiStringReader(settings);

            // Act
            var deserialized = reader.Read(actual, out var diagnostic);

            // Assert
            diagnostic.Errors.Should().BeEmpty();
            var channel = deserialized.Channels.First().Value as AsyncApiChannelReference;

            channel.UnresolvedReference.Should().BeFalse();
            channel.Description.Should().Be("customDescription");
            channel.Reference.ExternalResource.Should().BeNull();
            channel.Reference.FragmentId.Should().Be("/components/channels/myChannel");
            channel.Reference.IsExternal.Should().BeFalse();
            channel.Reference.Type.Should().Be(ReferenceType.Channel);
        }

        [Test]
        public void V2_AsyncApiDocument_WithExternalReferenceOnlySetToResolveInternalReferences_DoesNotResolve()
        {
            // Arrange
            var actual = """
                         asyncapi: 2.6.0
                         info:
                           title: My AsyncAPI Document
                           version: 1.0.0
                         channels:
                           myChannel:
                             $ref: http://example.com/channel.json
                         """;

            var settings = new AsyncApiReaderSettings()
            {
                ReferenceResolution = ReferenceResolutionSetting.ResolveInternalReferences,
            };
            var reader = new AsyncApiStringReader(settings);

            // Act
            var deserialized = reader.Read(actual, out var diagnostic);

            // Assert
            diagnostic.Errors.Should().BeEmpty();
            var channel = deserialized.Channels.First().Value as AsyncApiChannelReference;

            channel.UnresolvedReference.Should().BeTrue();
            channel.Description.Should().BeNull();
            channel.Reference.ExternalResource.Should().Be("http://example.com/channel.json");
            channel.Reference.Type.Should().Be(ReferenceType.Channel);
            channel.Reference.FragmentId.Should().BeNull();
            channel.Reference.IsExternal.Should().BeTrue();
            channel.Reference.IsFragment.Should().BeFalse();
        }

        [Test]
        public void V2_AsyncApiReference_WithExternalReference_AllowsReferenceDoesNotResolve()
        {
            // Arrange
            var actual = """
                payload:
                  $ref: http://example.com/json.json
                """;
            var reader = new AsyncApiStringReader();

            // Act
            var deserialized = reader.ReadFragment<AsyncApiMessage>(actual, AsyncApiVersion.AsyncApi2_0, out var diagnostic);

            // Assert
            diagnostic.Errors.Should().BeEmpty();
            var payload = deserialized.Payload.Schema.As<AsyncApiJsonSchemaReference>();
            var reference = payload.Reference;
            reference.ExternalResource.Should().Be("http://example.com/json.json");
            reference.FragmentId.Should().BeNull();
            reference.IsExternal.Should().BeTrue();
            reference.IsFragment.Should().BeFalse();
            diagnostic.Errors.Should().BeEmpty();

            var expected = deserialized.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);

            expected
                .Should()
                .BePlatformAgnosticEquivalentTo(actual);
        }

        [Test]
        public void V2_AsyncApiReference_WithExternalResourcesInterface_DeserializesCorrectly()
        {
            var yaml = """
                       asyncapi: 2.3.0
                       info:
                         title: test
                         version: 1.0.0
                       channels:
                         workspace:
                           publish:
                             message:
                               $ref: "./some/path/to/external/message.yaml"
                       """;
            var settings = new AsyncApiReaderSettings
            {
                ReferenceResolution = ReferenceResolutionSetting.ResolveAllReferences,
                ExternalReferenceLoader = new MockJsonSchemaLoader(),
            };
            var reader = new AsyncApiStringReader(settings);
            var doc = reader.Read(yaml, out var diagnostic);
            var message = doc.Channels["workspace"].Messages.Values.First();
            message.Name.Should().Be("Test");
            var payload = message.Payload.Schema.As<AsyncApiJsonSchema>();
            payload.Properties.Count.Should().Be(1);
        }

        [Test]
        public void V2_AsyncApiReference_DocumentLevelReferencePointer_DeserializesCorrectly()
        {
            var yaml = """
               asyncapi: 2.3.0
               info:
                 title: test
                 version: 1.0.0
               channels:
                 workspace:
                   publish:
                     message:
                       title: test message
                 other:
                   $ref: "#/channels/workspace"
               """;

            var reader = new AsyncApiStringReader();
            var doc = reader.Read(yaml, out var diagnostic);
            doc.Channels.Should().HaveCount(2);
            doc.Channels["other"].Messages.Values.First().Title.Should().Be("test message");
        }

        [Test]
        public void V2_AsyncApiReference_WithExternalAvroResource_DeserializesCorrectlyAndUpgrades()
        {
            var expected =
                """
                asyncapi: 2.6.0
                info:
                  title: test
                  version: 1.0.0
                channels:
                  workspace:
                    publish:
                      message:
                        $ref: '#/components/messages/anonymous-message-1'
                components:
                  messages:
                    anonymous-message-1:
                      payload:
                        $ref: ./some/path/to/external/payload.json
                      schemaFormat: application/vnd.apache.avro
                      name: Test
                      title: Test message
                      summary: Test.
                """;

            var avroPayload =
                """
                {
                  "type": "record",
                  "name": "thecodebuzz_schema",
                  "namespace": "thecodebuzz.avro",
                  "fields": [
                    {
                      "name": "username",
                      "type": "string",
                      "doc": "Name of the user account on Thecodebuzz.com"
                    },
                    {
                      "name": "email",
                      "type": "string",
                      "doc": "The email of the user logging message on the blog"
                    },
                    {
                      "name": "timestamp",
                      "type": "long",
                      "doc": "time in seconds"
                    }
                  ],
                  "doc:": "A basic schema for storing thecodebuzz blogs messages"
                }
                """;

            var yaml = """
                asyncapi: 2.6.0
                info:
                  title: test
                  version: 1.0.0
                channels:
                  workspace:
                    publish:
                      message:
                        payload:
                          $ref: ./some/path/to/external/payload.json
                        schemaFormat: application/vnd.apache.avro
                        name: Test
                        title: Test message
                        summary: Test.
                """;
            var settings = new AsyncApiReaderSettings
            {
                ReferenceResolution = ReferenceResolutionSetting.ResolveAllReferences,
                ExternalReferenceLoader = new MockStringLoader(avroPayload),
            };
            var reader = new AsyncApiStringReader(settings);
            var doc = reader.Read(yaml, out var diagnostic);
            var message = doc.Channels["workspace"].Messages.Values.First();
            var payload = message.Payload.Schema.As<AsyncApiAvroSchema>();
            payload.As<AvroRecord>().Name.Should().Be("thecodebuzz_schema");

            doc.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0).Should()
                .BePlatformAgnosticEquivalentTo(expected);

        }

        private static readonly TestCaseData[] UnresolvedReferences =
        {
            new TestCaseData(new AsyncApiChannelReference("#/components/channels/a"), new AsyncApiChannelReference("#/components/channels/a"), new AsyncApiChannelReference("#/components/channels/b")).SetArgDisplayNames("Channel"),
            new TestCaseData(new AsyncApiCorrelationIdReference("#/components/correlationIds/a"), new AsyncApiCorrelationIdReference("#/components/correlationIds/a"), new AsyncApiCorrelationIdReference("#/components/correlationIds/b")).SetArgDisplayNames("CorrelationId"),
            new TestCaseData(new AsyncApiExternalDocumentationReference("#/components/externalDocs/a"), new AsyncApiExternalDocumentationReference("#/components/externalDocs/a"), new AsyncApiExternalDocumentationReference("#/components/externalDocs/b")).SetArgDisplayNames("ExternalDocumentation"),
            new TestCaseData(new AsyncApiJsonSchemaReference("#/components/schemas/a"), new AsyncApiJsonSchemaReference("#/components/schemas/a"), new AsyncApiJsonSchemaReference("#/components/schemas/b")).SetArgDisplayNames("JsonSchema"),
            new TestCaseData(new AsyncApiMessageReference("#/components/messages/a"), new AsyncApiMessageReference("#/components/messages/a"), new AsyncApiMessageReference("#/components/messages/b")).SetArgDisplayNames("Message"),
            new TestCaseData(new AsyncApiMessageTraitReference("#/components/messageTraits/a"), new AsyncApiMessageTraitReference("#/components/messageTraits/a"), new AsyncApiMessageTraitReference("#/components/messageTraits/b")).SetArgDisplayNames("MessageTrait"),
            new TestCaseData(new AsyncApiOperationReference("#/components/operations/a"), new AsyncApiOperationReference("#/components/operations/a"), new AsyncApiOperationReference("#/components/operations/b")).SetArgDisplayNames("Operation"),
            new TestCaseData(new AsyncApiOperationReplyAddressReference("#/components/replyAddresses/a"), new AsyncApiOperationReplyAddressReference("#/components/replyAddresses/a"), new AsyncApiOperationReplyAddressReference("#/components/replyAddresses/b")).SetArgDisplayNames("OperationReplyAddress"),
            new TestCaseData(new AsyncApiOperationReplyReference("#/components/replies/a"), new AsyncApiOperationReplyReference("#/components/replies/a"), new AsyncApiOperationReplyReference("#/components/replies/b")).SetArgDisplayNames("OperationReply"),
            new TestCaseData(new AsyncApiOperationTraitReference("#/components/operationTraits/a"), new AsyncApiOperationTraitReference("#/components/operationTraits/a"), new AsyncApiOperationTraitReference("#/components/operationTraits/b")).SetArgDisplayNames("OperationTrait"),
            new TestCaseData(new AsyncApiParameterReference("#/components/parameters/a"), new AsyncApiParameterReference("#/components/parameters/a"), new AsyncApiParameterReference("#/components/parameters/b")).SetArgDisplayNames("Parameter"),
            new TestCaseData(new AsyncApiServerReference("#/components/servers/a"), new AsyncApiServerReference("#/components/servers/a"), new AsyncApiServerReference("#/components/servers/b")).SetArgDisplayNames("Server"),
            new TestCaseData(new AsyncApiServerVariableReference("#/components/serverVariables/a"), new AsyncApiServerVariableReference("#/components/serverVariables/a"), new AsyncApiServerVariableReference("#/components/serverVariables/b")).SetArgDisplayNames("ServerVariable"),
            new TestCaseData(new AsyncApiTagReference("#/components/tags/a"), new AsyncApiTagReference("#/components/tags/a"), new AsyncApiTagReference("#/components/tags/b")).SetArgDisplayNames("Tag"),
        };

        private static readonly TestCaseData[] UnresolvedReferencesComparedToNull =
        {
            new TestCaseData((Func<bool>)(() => new AsyncApiChannelReference("#/components/channels/a").Equals((AsyncApiChannel)null))).SetArgDisplayNames("Channel"),
            new TestCaseData((Func<bool>)(() => new AsyncApiCorrelationIdReference("#/components/correlationIds/a").Equals((AsyncApiCorrelationId)null))).SetArgDisplayNames("CorrelationId"),
            new TestCaseData((Func<bool>)(() => new AsyncApiExternalDocumentationReference("#/components/externalDocs/a").Equals((AsyncApiExternalDocumentation)null))).SetArgDisplayNames("ExternalDocumentation"),
            new TestCaseData((Func<bool>)(() => new AsyncApiJsonSchemaReference("#/components/schemas/a").Equals((AsyncApiJsonSchema)null))).SetArgDisplayNames("JsonSchema"),
            new TestCaseData((Func<bool>)(() => new AsyncApiMessageReference("#/components/messages/a").Equals((AsyncApiMessage)null))).SetArgDisplayNames("Message"),
            new TestCaseData((Func<bool>)(() => new AsyncApiMessageTraitReference("#/components/messageTraits/a").Equals((AsyncApiMessageTrait)null))).SetArgDisplayNames("MessageTrait"),
            new TestCaseData((Func<bool>)(() => new AsyncApiOperationReference("#/components/operations/a").Equals((AsyncApiOperation)null))).SetArgDisplayNames("Operation"),
            new TestCaseData((Func<bool>)(() => new AsyncApiOperationReplyAddressReference("#/components/replyAddresses/a").Equals((AsyncApiOperationReplyAddress)null))).SetArgDisplayNames("OperationReplyAddress"),
            new TestCaseData((Func<bool>)(() => new AsyncApiOperationReplyReference("#/components/replies/a").Equals((AsyncApiOperationReply)null))).SetArgDisplayNames("OperationReply"),
            new TestCaseData((Func<bool>)(() => new AsyncApiOperationTraitReference("#/components/operationTraits/a").Equals((AsyncApiOperationTrait)null))).SetArgDisplayNames("OperationTrait"),
            new TestCaseData((Func<bool>)(() => new AsyncApiParameterReference("#/components/parameters/a").Equals((AsyncApiParameter)null))).SetArgDisplayNames("Parameter"),
            new TestCaseData((Func<bool>)(() => new AsyncApiServerReference("#/components/servers/a").Equals((AsyncApiServer)null))).SetArgDisplayNames("Server"),
            new TestCaseData((Func<bool>)(() => new AsyncApiServerVariableReference("#/components/serverVariables/a").Equals((AsyncApiServerVariable)null))).SetArgDisplayNames("ServerVariable"),
            new TestCaseData((Func<bool>)(() => new AsyncApiTagReference("#/components/tags/a").Equals((AsyncApiTag)null))).SetArgDisplayNames("Tag"),
        };

        /// <summary>
        /// Regression test.
        /// Bug: Unresolved references compared their (null) targets, so every unresolved reference equalled every other.
        /// </summary>
        [TestCaseSource(nameof(UnresolvedReferences))]
        public void UnresolvedReferences_AreEqualOnlyWhenTheyHaveTheSameReference(object reference, object sameReference, object otherReference)
        {
            reference.Equals(sameReference).Should().BeTrue();
            reference.Equals(otherReference).Should().BeFalse();
        }

        /// <summary>
        /// Regression test.
        /// Bug: An unresolved reference compared its (null) target to the element, so it equalled null.
        /// </summary>
        [TestCaseSource(nameof(UnresolvedReferencesComparedToNull))]
        public void UnresolvedReference_IsNotEqualToNull(Func<bool> equalsNull)
        {
            equalsNull().Should().BeFalse();
        }

        [Test]
        public void ResolvedReferences_AreEqualWhenTheyPointAtTheSameComponent()
        {
            var workspace = new AsyncApiWorkspace();
            var messageA = new AsyncApiMessage { Name = "a" };
            workspace.RegisterComponent("#/components/messages/a", messageA);
            workspace.RegisterComponent("#/components/messages/b", new AsyncApiMessage { Name = "b" });

            var reference = new AsyncApiMessageReference("#/components/messages/a");
            var sameReference = new AsyncApiMessageReference("#/components/messages/a");
            var otherReference = new AsyncApiMessageReference("#/components/messages/b");
            reference.Reference.Workspace = workspace;
            sameReference.Reference.Workspace = workspace;
            otherReference.Reference.Workspace = workspace;

            (reference == sameReference).Should().BeTrue();
            (reference == otherReference).Should().BeFalse();
            reference.Equals(messageA).Should().BeTrue();
        }

        [Test]
        public void AsyncApiReference_WithTheSameReference_HasTheSameHashCode()
        {
            var reference = new AsyncApiReference("#/components/schemas/a", ReferenceType.Schema);
            var sameReference = new AsyncApiReference("#/components/schemas/a", ReferenceType.Schema);

            reference.GetHashCode().Should().Be(sameReference.GetHashCode());
            new[] { reference, sameReference }.Distinct().Should().ContainSingle();
        }

        /// <summary>
        /// Regression test.
        /// Bug: Extensions read the backing field, so it threw until another member had resolved the reference.
        /// </summary>
        [Test]
        public void BindingsReference_ReadingExtensionsFirst_ResolvesTheReference()
        {
            var workspace = new AsyncApiWorkspace();
            var bindings = new AsyncApiBindings<IChannelBinding>();
            workspace.RegisterComponent("#/components/channelBindings/a", bindings);

            var reference = new AsyncApiBindingsReference<IChannelBinding>("#/components/channelBindings/a");
            reference.Reference.Workspace = workspace;

            reference.Extensions.Should().BeSameAs(bindings.Extensions);
        }
    }

    public class MockStringLoader : IStreamLoader
    {
        public MockStringLoader(string input)
        {
            this.input = input;
        }

        private readonly string input;

        public Stream Load(Uri baseUri, Uri uri)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(this.input);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public Task<Stream> LoadAsync(Uri baseUri, Uri uri)
        {
            return Task.FromResult(this.Load(baseUri, uri));
        }
    }

    public class MockJsonSchemaLoader : IStreamLoader
    {
        const string Message =
            """
            name: Test
            title: Test message
            summary: Test.
            schemaFormat: application/schema+yaml;version=draft-07
            contentType: application/cloudevents+json
            payload:
              $ref: "./some/path/to/schema.yaml"
            """;

        const string Schema =
            """
            type: object
            properties:
              lumens:
                type: integer
                minimum: 0
                description: Light intensity measured in lumens.
            """;

        public Stream Load(Uri baseUri, Uri uri)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            if (uri.ToString() == "./some/path/to/external/message.yaml")
            {
                writer.Write(Message);
            }
            else
            {
                writer.Write(Schema);
            }
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public Task<Stream> LoadAsync(Uri baseUri, Uri uri)
        {
            return Task.FromResult(this.Load(baseUri, uri));
        }
    }
}