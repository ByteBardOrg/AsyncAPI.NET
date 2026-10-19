namespace ByteBard.AsyncAPI.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using FluentAssertions;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers;
    using ByteBard.AsyncAPI.Writers;
    using NUnit.Framework;

    public class AsyncApiSchema_Should : TestBase
    {
        public static AsyncApiJsonSchema BasicSchema = new AsyncApiJsonSchema();

        public static AsyncApiJsonSchema AdvancedSchemaNumber = new AsyncApiJsonSchema
        {
            Title = "title1",
            MultipleOf = 3,
            Maximum = 42,
            ExclusiveMinimum = 42,
            Minimum = 10,
            Default = new AsyncApiAny(15),
            Type = SchemaType.Integer,
            Nullable = true,
            ExternalDocs = new AsyncApiExternalDocumentation
            {
                Url = new Uri("http://example.com/externalDocs"),
            },
        };

        public static AsyncApiJsonSchema AdvancedSchemaBigNumbers = new AsyncApiJsonSchema
        {
            Title = "title1",
            MultipleOf = 3,
            Maximum = double.MaxValue,
            ExclusiveMinimum = double.MinValue,
            Minimum = double.MinValue,
            Default = new AsyncApiAny(15),
            Type = SchemaType.Integer,
            Nullable = true,
            ExternalDocs = new AsyncApiExternalDocumentation
            {
                Url = new Uri("http://example.com/externalDocs"),
            },
        };

        public static AsyncApiJsonSchema AdvancedSchemaObject = new AsyncApiJsonSchema
        {
            Title = "title1",
            Properties = new Dictionary<string, AsyncApiJsonSchema>
            {
                ["property1"] = new AsyncApiJsonSchema
                {
                    Properties = new Dictionary<string, AsyncApiJsonSchema>
                    {
                        ["property2"] = new AsyncApiJsonSchema
                        {
                            Type = SchemaType.Integer,
                        },
                        ["property3"] = new AsyncApiJsonSchema
                        {
                            Type = SchemaType.String,
                            MaxLength = 15,
                        },
                    },
                    AdditionalProperties = new FalseApiSchema(),
                    Items = new FalseApiSchema(),
                    AdditionalItems = new FalseApiSchema(),
                },
                ["property4"] = new AsyncApiJsonSchema
                {
                    Properties = new Dictionary<string, AsyncApiJsonSchema>
                    {
                        ["property5"] = new AsyncApiJsonSchema
                        {
                            Properties = new Dictionary<string, AsyncApiJsonSchema>
                            {
                                ["property6"] = new AsyncApiJsonSchema
                                {
                                    Type = SchemaType.Boolean,
                                },
                            },
                        },
                        ["property7"] = new AsyncApiJsonSchema
                        {
                            Type = SchemaType.String,
                            MinLength = 2,
                        },
                    },
                    PatternProperties = new Dictionary<string, AsyncApiJsonSchema>()
                    {
                        {
                            "^S_",
                            new AsyncApiJsonSchema()
                            {
                                Type = SchemaType.String,
                            }
                        },
                        {
                            "^I_", new AsyncApiJsonSchema()
                            {
                                Type = SchemaType.Integer,
                            }
                        },
                    },
                    PropertyNames = new AsyncApiJsonSchema()
                    {
                        Pattern = "^[A-Za-z_][A-Za-z0-9_]*$",
                    },
                    AdditionalProperties = new AsyncApiJsonSchema
                    {
                        Properties = new Dictionary<string, AsyncApiJsonSchema>
                        {
                            ["Property8"] = new AsyncApiJsonSchema
                            {
                                Type = SchemaType.String | SchemaType.Null,
                            },
                        },
                    },
                    Items = new AsyncApiJsonSchema
                    {
                        Properties = new Dictionary<string, AsyncApiJsonSchema>
                        {
                            ["Property9"] = new AsyncApiJsonSchema
                            {
                                Type = SchemaType.String | SchemaType.Null,
                            },
                        },
                    },
                    AdditionalItems = new AsyncApiJsonSchema
                    {
                        Properties = new Dictionary<string, AsyncApiJsonSchema>
                        {
                            ["Property10"] = new AsyncApiJsonSchema
                            {
                                Type = SchemaType.String | SchemaType.Null,
                            },
                        },
                    },
                },
                ["property11"] = new AsyncApiJsonSchema
                {
                    Const = new AsyncApiAny("aSpecialConstant"),
                },
            },
            Nullable = true,
            ExternalDocs = new AsyncApiExternalDocumentation
            {
                Url = new Uri("http://example.com/externalDocs"),
            },
        };

        public static AsyncApiJsonSchema AdvancedSchemaWithAllOf = new AsyncApiJsonSchema
        {
            Title = "title1",
            AllOf = new List<AsyncApiJsonSchema>
            {
                new AsyncApiJsonSchema
                {
                    Title = "title2",
                    Properties = new Dictionary<string, AsyncApiJsonSchema>
                    {
                        ["property1"] = new AsyncApiJsonSchema
                        {
                            Type = SchemaType.Integer,
                        },
                        ["property2"] = new AsyncApiJsonSchema
                        {
                            Type = SchemaType.String,
                            MaxLength = 15,
                        },
                    },
                },
                new AsyncApiJsonSchema
                {
                    Title = "title3",
                    Properties = new Dictionary<string, AsyncApiJsonSchema>
                    {
                        ["property3"] = new AsyncApiJsonSchema
                        {
                            Properties = new Dictionary<string, AsyncApiJsonSchema>
                            {
                                ["property4"] = new AsyncApiJsonSchema
                                {
                                    Type = SchemaType.Boolean ,
                                },
                            },
                        },
                        ["property5"] = new AsyncApiJsonSchema
                        {
                            Type = SchemaType.String,
                            MinLength = 2,
                        },
                    },
                    Nullable = true,
                },
            },
            Nullable = true,
            ExternalDocs = new AsyncApiExternalDocumentation
            {
                Url = new Uri("http://example.com/externalDocs"),
            },
        };

        public static AsyncApiJsonSchema ReferencedSchema = new AsyncApiJsonSchema
        {
            Title = "title1",
            MultipleOf = 3,
            Maximum = 42,
            ExclusiveMinimum = 42,
            Minimum = 10,
            Default = new AsyncApiAny(15),
            Type = SchemaType.Integer,

            Nullable = true,
            ExternalDocs = new AsyncApiExternalDocumentation
            {
                Url = new Uri("http://example.com/externalDocs"),
            },
        };

        public static AsyncApiJsonSchema AdvancedSchemaWithRequiredPropertiesObject = new AsyncApiJsonSchema
        {
            Title = "title1",
            Required = new HashSet<string>() { "property1" },
            Properties = new Dictionary<string, AsyncApiJsonSchema>
            {
                ["property1"] = new AsyncApiJsonSchema
                {
                    Required = new HashSet<string>() { "property3" },
                    Properties = new Dictionary<string, AsyncApiJsonSchema>
                    {
                        ["property2"] = new AsyncApiJsonSchema
                        {
                            Type = SchemaType.Integer,
                        },
                        ["property3"] = new AsyncApiJsonSchema
                        {
                            Type = SchemaType.String,
                            MaxLength = 15,
                            ReadOnly = true,
                        },
                    },
                    ReadOnly = true,
                },
                ["property4"] = new AsyncApiJsonSchema
                {
                    Properties = new Dictionary<string, AsyncApiJsonSchema>
                    {
                        ["property5"] = new AsyncApiJsonSchema
                        {
                            Properties = new Dictionary<string, AsyncApiJsonSchema>
                            {
                                ["property6"] = new AsyncApiJsonSchema
                                {
                                    Type = SchemaType.Boolean,
                                },
                            },
                        },
                        ["property7"] = new AsyncApiJsonSchema
                        {
                            Type = SchemaType.String,
                            MinLength = 2,
                        },
                    },
                    ReadOnly = true,
                },
            },
            Nullable = true,
            ExternalDocs = new AsyncApiExternalDocumentation
            {
                Url = new Uri("http://example.com/externalDocs"),
            },
        };

        [Test]
        public void V2_SerializeAsJson_WithBasicSchema_V2Works()
        {
            // Arrange
            var expected = @"{ }";

            // Act
            var actual = BasicSchema.SerializeAsJson(AsyncApiVersion.AsyncApi2_0);

            // Assert
            actual.Should()
                  .BePlatformAgnosticEquivalentTo(expected);
        }

        [Test]
        public void V2_SerializeAsJson_WithAdvancedSchemaNumber_V2Works()
        {
            // Arrange
            var expected = """
                {
                  "title": "title1",
                  "type": "integer",
                  "maximum": 42,
                  "minimum": 10,
                  "exclusiveMinimum": 42,
                  "multipleOf": 3,
                  "default": 15,
                  "nullable": true,
                  "externalDocs": {
                    "url": "http://example.com/externalDocs"
                  }
                }
                """;

            // Act
            var actual = AdvancedSchemaNumber.SerializeAsJson(AsyncApiVersion.AsyncApi2_0);

            // Assert
            actual.Should()
                  .BePlatformAgnosticEquivalentTo(expected);
        }

        [Test]
        public void V2_SerializeAsJson_WithAdvancedSchemaBigNumbers_V2Works()
        {
            // Arrange
            var expected = """
                {
                  "title": "title1",
                  "type": "integer",
                  "maximum": 1.7976931348623157E+308,
                  "minimum": -1.7976931348623157E+308,
                  "exclusiveMinimum": -1.7976931348623157E+308,
                  "multipleOf": 3,
                  "default": 15,
                  "nullable": true,
                  "externalDocs": {
                    "url": "http://example.com/externalDocs"
                  }
                }
                """;

            // Act
            var actual = AdvancedSchemaBigNumbers.SerializeAsJson(AsyncApiVersion.AsyncApi2_0);

            // Assert
            actual.Should()
                  .BePlatformAgnosticEquivalentTo(expected);
        }

        [Test]
        public void V2_SerializeAsJson_WithAdvancedSchemaObject_V2Works()
        {
            // Arrange
            string expected = this.GetTestData<string>(AsyncApiVersion.AsyncApi2_0, "SerializeAsJson_WithAdvancedSchemaObject_V2Works");

            // Act
            var actual = AdvancedSchemaObject.SerializeAsJson(AsyncApiVersion.AsyncApi2_0);

            // Assert
            actual.Should()
                  .BePlatformAgnosticEquivalentTo(expected);
        }

        [Test]
        public void V2_Deserialize_WithAdvancedSchema_Works()
        {
            // Arrange
            var json = this.GetTestData<string>(AsyncApiVersion.AsyncApi2_0, "Deserialize_WithAdvancedSchema_Works");
            var expected = AdvancedSchemaObject;

            // Act
            var actual = new AsyncApiStringReader().ReadFragment<AsyncApiJsonSchema>(json, AsyncApiVersion.AsyncApi2_0, out var _diagnostics);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void V2_SerializeAsJson_WithAdvancedSchemaWithAllOf_V2Works()
        {
            // Arrange
            var expected = this.GetTestData<string>(AsyncApiVersion.AsyncApi2_0, "SerializeAsJson_WithAdvancedSchemaWithAllOf_V2Works");

            // Act
            var actual = AdvancedSchemaWithAllOf.SerializeAsJson(AsyncApiVersion.AsyncApi2_0);

            // Assert
            actual.Should()
                  .BePlatformAgnosticEquivalentTo(expected);
        }

        [Theory]
        [TestCase(ReferenceInlineSetting.InlineReferences)]
        [TestCase(ReferenceInlineSetting.DoNotInlineReferences)]
        public void V2_Serialize_WithInliningOptions_ShouldInlineAccordingly(ReferenceInlineSetting shouldInline)
        {
            // arrange
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
            .WithChannel("mychannel", new AsyncApiChannel()
            {
                Address = "mychannel-{parameter}",
                Messages = new Dictionary<string, AsyncApiMessage>
                {
                    {
                        "whatever", new AsyncApiMessage
                        {
                            Payload = new AsyncApiJsonSchema
                            {
                                Type = SchemaType.Object,
                                Required = new HashSet<string> { "testB" },
                                Properties = new Dictionary<string, AsyncApiJsonSchema>
                                {
                                    { "testC", new AsyncApiJsonSchemaReference("#/components/schemas/testC") },
                                    { "testB", new AsyncApiJsonSchemaReference("#/components/schemas/testB") },
                                },
                            },
                        }
                    },
                },
            })
            .WithOperation("operationA", new AsyncApiOperation
            {
                Action = AsyncApiAction.Receive,
                Channel = new AsyncApiChannelReference("#/channels/mychannel"),
                Messages = new List<AsyncApiMessageReference> { new AsyncApiMessageReference("#/channels/mychannel/messages/whatever") }
            })
            .WithComponent("testD", new AsyncApiJsonSchema() { Type = SchemaType.String, Format = "uuid" })
            .WithComponent("testC", new AsyncApiJsonSchema()
            {
                Type = SchemaType.Object,
                Properties = new Dictionary<string, AsyncApiJsonSchema>
                {
                    { "testD", new AsyncApiJsonSchemaReference("#/components/schemas/testD") },
                },
            })
            .WithComponent("testB", new AsyncApiJsonSchema() { Description = "test", Type = SchemaType.Boolean })
            .Build();

            var outputString = new StringWriter();
            var writer = new AsyncApiYamlWriter(outputString, new AsyncApiWriterSettings { ReferenceInline = shouldInline });

            // Act
            asyncApiDocument.SerializeV2(writer);

            var actual = outputString.ToString();

            // Assert
            string expected = this.GetTestData<string>(
                AsyncApiVersion.AsyncApi2_0,
                shouldInline == ReferenceInlineSetting.InlineReferences
                ? "AsyncApiSchema_InlinedReferences"
                : "AsyncApiSchema_NoInlinedReferences.yml");

            actual.Should()
                  .BePlatformAgnosticEquivalentTo(expected);
        }

        [Theory]
        [TestCase(ReferenceInlineSetting.InlineReferences)]
        [TestCase(ReferenceInlineSetting.DoNotInlineReferences)]
        public void V3_Serialize_WithInliningOptions_ShouldInlineAccordingly(ReferenceInlineSetting shouldInline)
        {
            // arrange
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
            .WithChannel("mychannel", new AsyncApiChannel()
            {
                Messages = new Dictionary<string, AsyncApiMessage>
                    {
                {
                    "whatever", new AsyncApiMessage
                    {
                        Payload = new AsyncApiJsonSchema
                        {
                            Type = SchemaType.Object,
                            Required = new HashSet<string> { "testB" },
                            Properties = new Dictionary<string, AsyncApiJsonSchema>
                            {
                                { "testC", new AsyncApiJsonSchemaReference("#/components/schemas/testC") },
                                { "testB", new AsyncApiJsonSchemaReference("#/components/schemas/testB") },
                            },
                        },
                    }
                },
                    },
            })
            .WithComponent("testD", new AsyncApiJsonSchema() { Type = SchemaType.String, Format = "uuid" })
            .WithComponent("testC", new AsyncApiJsonSchema()
            {
                Type = SchemaType.Object,
                Properties = new Dictionary<string, AsyncApiJsonSchema>
                {
            { "testD", new AsyncApiJsonSchemaReference("#/components/schemas/testD") },
                },
            })
            .WithComponent("testB", new AsyncApiJsonSchema() { Description = "test", Type = SchemaType.Boolean })
            .Build();

            var outputString = new StringWriter();
            var writer = new AsyncApiYamlWriter(outputString, new AsyncApiWriterSettings { ReferenceInline = shouldInline });

            // Act
            asyncApiDocument.SerializeV3(writer);

            var actual = outputString.ToString();

            // Assert
            string expected = this.GetTestData<string>(AsyncApiVersion.AsyncApi3_0,
                shouldInline == ReferenceInlineSetting.InlineReferences
                ? "AsyncApiSchema_InlinedReferences"
                : "AsyncApiSchema_NoInlinedReferences.yml");

            actual.Should()
                  .BePlatformAgnosticEquivalentTo(expected);
        }

        [Test]
        public void V2_SerializeV2_WithNullWriter_Throws()
        {
            // Arrange
            var asyncApiLicense = new AsyncApiLicense();

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => { asyncApiLicense.SerializeV2(null); });
        }

        /// <summary>
        /// Regression test.
        /// Bug: Serializing properties multiple times - specifically Schema.OneOf was serialized into OneOf and Then.
        /// </summary>
        [Test]
        public void V2_Serialize_WithOneOf_DoesNotWriteThen()
        {
            var mainSchema = new AsyncApiJsonSchema();
            var subSchema = new AsyncApiJsonSchema();
            subSchema.Properties.Add("title", new AsyncApiJsonSchema() { Type = SchemaType.String });
            mainSchema.OneOf = new List<AsyncApiJsonSchema>() { subSchema };

            var yaml = mainSchema.Serialize(AsyncApiVersion.AsyncApi2_0, AsyncApiFormat.Yaml);

            Assert.True(!yaml.Contains("then:"), "then");
            Assert.True(yaml.Contains("oneOf:"), "oneOf");
        }

        /// <summary>
        /// Regression test.
        /// Bug: Serializing properties multiple times - specifically Schema.AnyOf was serialized into AnyOf and If.
        /// </summary>
        [Test]
        public void V2_Serialize_WithAnyOf_DoesNotWriteIf()
        {
            var mainSchema = new AsyncApiJsonSchema();
            var subSchema = new AsyncApiJsonSchema();
            subSchema.Properties.Add("title", new AsyncApiJsonSchema() { Type = SchemaType.String });
            mainSchema.AnyOf = new List<AsyncApiJsonSchema>() { subSchema };

            var yaml = mainSchema.Serialize(AsyncApiVersion.AsyncApi2_0, AsyncApiFormat.Yaml);

            Assert.True(!yaml.Contains("if:"));
        }

        [Test]
        public void V2_Deserialize_BasicExample()
        {
            var input =
                """
                title: title1
                type: integer
                maximum: 42
                minimum: 10
                exclusiveMinimum: 42.0
                multipleOf: 3
                default: 15
                nullable: true
                externalDocs:
                  url: http://example.com/externalDocs
                """;

            var schema = new AsyncApiStringReader().ReadFragment<AsyncApiJsonSchema>(input, AsyncApiVersion.AsyncApi2_0, out var diag);

            diag.Errors.Should().BeEmpty();
            schema.Should().BeEquivalentTo(AdvancedSchemaNumber);
        }

        [Test]
        public void V2_Deserialize_WithOutOfRangeIntegers_HasError()
        {
            var input =
                """
                title: title1
                type: string
                maxLength: 10000000000000
                minLength: 10000000000000
                maxProperties: 10000000000000
                minProperties: 10000000000000
                maxItems: 10000000000000
                minItems: 10000000000000
                default: this is a test
                nullable: true
                externalDocs:
                  url: http://example.com/externalDocs
                """;

            var schema = new AsyncApiStringReader().ReadFragment<AsyncApiJsonSchema>(input, AsyncApiVersion.AsyncApi2_0, out var diag);

            diag.Errors.Should().HaveCount(6);
        }

        [Test]
        public void V2_Deserialize_WithNegativeValues_HasError()
        {
            var input =
                """
                title: title1
                type: string
                maxLength: -22
                minLength: -22
                maxProperties: -22
                minProperties: -22
                maxItems: -22
                minItems: -22
                default: this is a test
                nullable: true
                externalDocs:
                  url: http://example.com/externalDocs
                """;

            var schema = new AsyncApiStringReader().ReadFragment<AsyncApiJsonSchema>(input, AsyncApiVersion.AsyncApi2_0, out var diag);

            diag.Errors.Should().HaveCount(6);
        }

        [Test]
        public void V3_Deserialize_WithOutOfRangeIntegers_HasError()
        {
            var input =
                """
                title: title1
                type: string
                maxLength: 10000000000000
                minLength: 10000000000000
                maxProperties: 10000000000000
                minProperties: 10000000000000
                maxItems: 10000000000000
                minItems: 10000000000000
                default: this is a test
                nullable: true
                externalDocs:
                  url: http://example.com/externalDocs
                """;

            var schema = new AsyncApiStringReader().ReadFragment<AsyncApiJsonSchema>(input, AsyncApiVersion.AsyncApi3_0, out var diag);

            diag.Errors.Should().HaveCount(6);
        }

        [Test]
        public void V3_Deserialize_WithNegativeValues_HasError()
        {
            var input =
                """
                title: title1
                type: string
                maxLength: -22
                minLength: -22
                maxProperties: -22
                minProperties: -22
                maxItems: -22
                minItems: -22
                default: this is a test
                nullable: true
                externalDocs:
                  url: http://example.com/externalDocs
                """;

            var schema = new AsyncApiStringReader().ReadFragment<AsyncApiJsonSchema>(input, AsyncApiVersion.AsyncApi3_0, out var diag);

            diag.Errors.Should().HaveCount(6);
        }
        /// <summary>
        /// Regression test.
        /// Bug: Serializing properties multiple times - specifically Schema.Not was serialized into Not and Else.
        /// </summary>
        [Test]
        public void V2_Serialize_WithNot_DoesNotWriteElse()
        {
            var mainSchema = new AsyncApiJsonSchema();
            var subSchema = new AsyncApiJsonSchema();
            subSchema.Properties.Add("title", new AsyncApiJsonSchema() { Type = SchemaType.String });
            mainSchema.Not = subSchema;

            var yaml = mainSchema.Serialize(AsyncApiVersion.AsyncApi2_0, AsyncApiFormat.Yaml);

            Assert.True(!yaml.Contains("else:"));
        }

        /// <summary>
        /// Regression test.
        /// Bug: Inlining a recursive schema that is reached by more than one reference threw
        /// "An item with the same key has already been added" when writing components.
        /// </summary>
        [Test]
        public void V2_Serialize_WithInlinedReferences_RecursiveSchemaReachedTwice_DoesNotThrow()
        {
            var yaml =
                """
                asyncapi: 2.6.0
                info:
                  title: test
                  version: 1.0.0
                channels:
                  first:
                    publish:
                      message:
                        payload:
                          $ref: '#/components/schemas/Node'
                components:
                  schemas:
                    Node:
                      type: object
                      properties:
                        next:
                          $ref: '#/components/schemas/Node'
                        prev:
                          $ref: '#/components/schemas/Node'
                """;
            var document = new AsyncApiStringReader().Read(yaml, out var diagnostic);
            diagnostic.Errors.Should().BeEmpty();

            var outputString = new StringWriter();
            var writer = new AsyncApiYamlWriter(outputString, new AsyncApiWriterSettings { ReferenceInline = ReferenceInlineSetting.InlineReferences });

            var act = () => document.SerializeV2(writer);

            act.Should().NotThrow();
            outputString.ToString().Replace("\r\n", "\n").Should().Contain("components:\n  schemas:\n    Node:");
        }

        [Test]
        public void V2_Serialize_WithInlinedReferences_RoundTripsYamlAndJson()
        {
            // Arrange
            var input =
                """
                asyncapi: 2.6.0
                info:
                  title: test
                  version: 1.0.0
                channels:
                  first:
                    publish:
                      message:
                        payload:
                          type: object
                          properties:
                            first:
                              $ref: '#/components/schemas/Leaf'
                            second:
                              $ref: '#/components/schemas/Leaf'
                            node:
                              $ref: '#/components/schemas/Node'
                components:
                  schemas:
                    Leaf:
                      type: string
                    Node:
                      type: object
                      properties:
                        value:
                          $ref: '#/components/schemas/Leaf'
                        next:
                          $ref: '#/components/schemas/Node'
                """;

            var expectedYaml =
                """
                asyncapi: 2.6.0
                info:
                  title: test
                  version: 1.0.0
                channels:
                  first:
                    publish:
                      message:
                        payload:
                          type: object
                          properties:
                            first:
                              type: string
                            second:
                              type: string
                            node:
                              $ref: '#/components/schemas/Node'
                components:
                  schemas:
                    Leaf:
                      type: string
                    Node:
                      type: object
                      properties:
                        value:
                          type: string
                        next:
                          $ref: '#/components/schemas/Node'
                """;

            var expectedJson =
                """
                {
                  "asyncapi": "2.6.0",
                  "info": {
                    "title": "test",
                    "version": "1.0.0"
                  },
                  "channels": {
                    "first": {
                      "publish": {
                        "message": {
                          "payload": {
                            "type": "object",
                            "properties": {
                              "first": {
                                "type": "string"
                              },
                              "second": {
                                "type": "string"
                              },
                              "node": {
                                "$ref": "#/components/schemas/Node"
                              }
                            }
                          }
                        }
                      }
                    }
                  },
                  "components": {
                    "schemas": {
                      "Leaf": {
                        "type": "string"
                      },
                      "Node": {
                        "type": "object",
                        "properties": {
                          "value": {
                            "type": "string"
                          },
                          "next": {
                            "$ref": "#/components/schemas/Node"
                          }
                        }
                      }
                    }
                  }
                }
                """;

            // Act & Assert
            AssertRoundTrip(AsyncApiVersion.AsyncApi2_0, ReferenceInlineSetting.InlineReferences, input, expectedYaml, expectedJson);
        }

        [Test]
        public void V3_Serialize_WithInlinedReferences_RoundTripsYamlAndJson()
        {
            // Arrange
            var input =
                """
                asyncapi: 3.0.0
                info:
                  title: test
                  version: 1.0.0
                channels:
                  first:
                    messages:
                      a:
                        payload:
                          type: object
                          properties:
                            first:
                              $ref: '#/components/schemas/Leaf'
                            second:
                              $ref: '#/components/schemas/Leaf'
                            node:
                              $ref: '#/components/schemas/Node'
                components:
                  schemas:
                    Leaf:
                      type: string
                    Node:
                      type: object
                      properties:
                        value:
                          $ref: '#/components/schemas/Leaf'
                        next:
                          $ref: '#/components/schemas/Node'
                """;

            var expectedYaml =
                """
                asyncapi: 3.1.0
                info:
                  title: test
                  version: 1.0.0
                channels:
                  first:
                    messages:
                      a:
                        payload:
                          schemaFormat: application/vnd.aai.asyncapi+json;version=3.0.0
                          schema:
                            type: object
                            properties:
                              first:
                                type: string
                              second:
                                type: string
                              node:
                                $ref: '#/components/schemas/Node'
                components:
                  schemas:
                    Leaf:
                      schemaFormat: application/vnd.aai.asyncapi+json;version=3.0.0
                      schema:
                        type: string
                    Node:
                      schemaFormat: application/vnd.aai.asyncapi+json;version=3.0.0
                      schema:
                        type: object
                        properties:
                          value:
                            type: string
                          next:
                            $ref: '#/components/schemas/Node'
                """;

            var expectedJson =
                """
                {
                  "asyncapi": "3.1.0",
                  "info": {
                    "title": "test",
                    "version": "1.0.0"
                  },
                  "channels": {
                    "first": {
                      "messages": {
                        "a": {
                          "payload": {
                            "schemaFormat": "application/vnd.aai.asyncapi+json;version=3.0.0",
                            "schema": {
                              "type": "object",
                              "properties": {
                                "first": {
                                  "type": "string"
                                },
                                "second": {
                                  "type": "string"
                                },
                                "node": {
                                  "$ref": "#/components/schemas/Node"
                                }
                              }
                            }
                          }
                        }
                      }
                    }
                  },
                  "components": {
                    "schemas": {
                      "Leaf": {
                        "schemaFormat": "application/vnd.aai.asyncapi+json;version=3.0.0",
                        "schema": {
                          "type": "string"
                        }
                      },
                      "Node": {
                        "schemaFormat": "application/vnd.aai.asyncapi+json;version=3.0.0",
                        "schema": {
                          "type": "object",
                          "properties": {
                            "value": {
                              "type": "string"
                            },
                            "next": {
                              "$ref": "#/components/schemas/Node"
                            }
                          }
                        }
                      }
                    }
                  }
                }
                """;

            // Act & Assert
            AssertRoundTrip(AsyncApiVersion.AsyncApi3_0, ReferenceInlineSetting.InlineReferences, input, expectedYaml, expectedJson);
        }

        [Test]
        public void V2_Serialize_WithoutInlinedReferences_RoundTripsYamlAndJson()
        {
            // Arrange
            var input =
                """
                asyncapi: 2.6.0
                info:
                  title: test
                  version: 1.0.0
                channels:
                  first:
                    publish:
                      message:
                        payload:
                          type: object
                          properties:
                            first:
                              $ref: '#/components/schemas/Leaf'
                            second:
                              $ref: '#/components/schemas/Leaf'
                            node:
                              $ref: '#/components/schemas/Node'
                components:
                  schemas:
                    Leaf:
                      type: string
                    Node:
                      type: object
                      properties:
                        value:
                          $ref: '#/components/schemas/Leaf'
                        next:
                          $ref: '#/components/schemas/Node'
                """;

            var expectedYaml =
                """
                asyncapi: 2.6.0
                info:
                  title: test
                  version: 1.0.0
                channels:
                  first:
                    publish:
                      message:
                        $ref: '#/components/messages/anonymous-message-1'
                components:
                  schemas:
                    Leaf:
                      type: string
                    Node:
                      type: object
                      properties:
                        value:
                          $ref: '#/components/schemas/Leaf'
                        next:
                          $ref: '#/components/schemas/Node'
                  messages:
                    anonymous-message-1:
                      payload:
                        type: object
                        properties:
                          first:
                            $ref: '#/components/schemas/Leaf'
                          second:
                            $ref: '#/components/schemas/Leaf'
                          node:
                            $ref: '#/components/schemas/Node'
                """;

            var expectedJson =
                """
                {
                  "asyncapi": "2.6.0",
                  "info": {
                    "title": "test",
                    "version": "1.0.0"
                  },
                  "channels": {
                    "first": {
                      "publish": {
                        "message": {
                          "$ref": "#/components/messages/anonymous-message-1"
                        }
                      }
                    }
                  },
                  "components": {
                    "schemas": {
                      "Leaf": {
                        "type": "string"
                      },
                      "Node": {
                        "type": "object",
                        "properties": {
                          "value": {
                            "$ref": "#/components/schemas/Leaf"
                          },
                          "next": {
                            "$ref": "#/components/schemas/Node"
                          }
                        }
                      }
                    },
                    "messages": {
                      "anonymous-message-1": {
                        "payload": {
                          "type": "object",
                          "properties": {
                            "first": {
                              "$ref": "#/components/schemas/Leaf"
                            },
                            "second": {
                              "$ref": "#/components/schemas/Leaf"
                            },
                            "node": {
                              "$ref": "#/components/schemas/Node"
                            }
                          }
                        }
                      }
                    }
                  }
                }
                """;

            // Act & Assert
            AssertRoundTrip(AsyncApiVersion.AsyncApi2_0, ReferenceInlineSetting.DoNotInlineReferences, input, expectedYaml, expectedJson);
        }

        [Test]
        public void V3_Serialize_WithoutInlinedReferences_RoundTripsYamlAndJson()
        {
            // Arrange
            var input =
                """
                asyncapi: 3.0.0
                info:
                  title: test
                  version: 1.0.0
                channels:
                  first:
                    messages:
                      a:
                        payload:
                          type: object
                          properties:
                            first:
                              $ref: '#/components/schemas/Leaf'
                            second:
                              $ref: '#/components/schemas/Leaf'
                            node:
                              $ref: '#/components/schemas/Node'
                components:
                  schemas:
                    Leaf:
                      type: string
                    Node:
                      type: object
                      properties:
                        value:
                          $ref: '#/components/schemas/Leaf'
                        next:
                          $ref: '#/components/schemas/Node'
                """;

            var expectedYaml =
                """
                asyncapi: 3.1.0
                info:
                  title: test
                  version: 1.0.0
                channels:
                  first:
                    messages:
                      a:
                        payload:
                          schemaFormat: application/vnd.aai.asyncapi+json;version=3.0.0
                          schema:
                            type: object
                            properties:
                              first:
                                $ref: '#/components/schemas/Leaf'
                              second:
                                $ref: '#/components/schemas/Leaf'
                              node:
                                $ref: '#/components/schemas/Node'
                components:
                  schemas:
                    Leaf:
                      schemaFormat: application/vnd.aai.asyncapi+json;version=3.0.0
                      schema:
                        type: string
                    Node:
                      schemaFormat: application/vnd.aai.asyncapi+json;version=3.0.0
                      schema:
                        type: object
                        properties:
                          value:
                            $ref: '#/components/schemas/Leaf'
                          next:
                            $ref: '#/components/schemas/Node'
                """;

            var expectedJson =
                """
                {
                  "asyncapi": "3.1.0",
                  "info": {
                    "title": "test",
                    "version": "1.0.0"
                  },
                  "channels": {
                    "first": {
                      "messages": {
                        "a": {
                          "payload": {
                            "schemaFormat": "application/vnd.aai.asyncapi+json;version=3.0.0",
                            "schema": {
                              "type": "object",
                              "properties": {
                                "first": {
                                  "$ref": "#/components/schemas/Leaf"
                                },
                                "second": {
                                  "$ref": "#/components/schemas/Leaf"
                                },
                                "node": {
                                  "$ref": "#/components/schemas/Node"
                                }
                              }
                            }
                          }
                        }
                      }
                    }
                  },
                  "components": {
                    "schemas": {
                      "Leaf": {
                        "schemaFormat": "application/vnd.aai.asyncapi+json;version=3.0.0",
                        "schema": {
                          "type": "string"
                        }
                      },
                      "Node": {
                        "schemaFormat": "application/vnd.aai.asyncapi+json;version=3.0.0",
                        "schema": {
                          "type": "object",
                          "properties": {
                            "value": {
                              "$ref": "#/components/schemas/Leaf"
                            },
                            "next": {
                              "$ref": "#/components/schemas/Node"
                            }
                          }
                        }
                      }
                    }
                  }
                }
                """;

            // Act & Assert
            AssertRoundTrip(AsyncApiVersion.AsyncApi3_0, ReferenceInlineSetting.DoNotInlineReferences, input, expectedYaml, expectedJson);
        }

        [Test]
        public void V2_Serialize_WithInlinedReferences_MissingPropertyReference_Throws()
        {
            var input =
                """
                asyncapi: 2.6.0
                info:
                  title: test
                  version: 1.0.0
                channels:
                  first:
                    publish:
                      message:
                        payload:
                          type: object
                          properties:
                            gone:
                              $ref: '#/components/schemas/Missing'
                """;

            AssertMissingReferenceThrowsWhenInlined(AsyncApiVersion.AsyncApi2_0, input);
        }

        [Test]
        public void V2_Serialize_WithInlinedReferences_MissingPayloadReference_Throws()
        {
            var input =
                """
                asyncapi: 2.6.0
                info:
                  title: test
                  version: 1.0.0
                channels:
                  first:
                    publish:
                      message:
                        payload:
                          $ref: '#/components/schemas/Missing'
                """;

            AssertMissingReferenceThrowsWhenInlined(AsyncApiVersion.AsyncApi2_0, input);
        }

        [Test]
        public void V3_Serialize_WithInlinedReferences_MissingPropertyReference_Throws()
        {
            var input =
                """
                asyncapi: 3.0.0
                info:
                  title: test
                  version: 1.0.0
                channels:
                  first:
                    messages:
                      a:
                        payload:
                          type: object
                          properties:
                            gone:
                              $ref: '#/components/schemas/Missing'
                """;

            AssertMissingReferenceThrowsWhenInlined(AsyncApiVersion.AsyncApi3_0, input);
        }

        [Test]
        public void V3_Serialize_WithInlinedReferences_MissingPayloadReference_Throws()
        {
            var input =
                """
                asyncapi: 3.0.0
                info:
                  title: test
                  version: 1.0.0
                channels:
                  first:
                    messages:
                      a:
                        payload:
                          $ref: '#/components/schemas/Missing'
                """;

            AssertMissingReferenceThrowsWhenInlined(AsyncApiVersion.AsyncApi3_0, input);
        }

        /// <summary>
        /// A reference that can't be resolved can't be inlined, so inlining it throws naming the reference,
        /// while writing it without inlining keeps the $ref as it was read.
        /// </summary>
        private static void AssertMissingReferenceThrowsWhenInlined(AsyncApiVersion version, string input)
        {
            var document = new AsyncApiStringReader().Read(input, out var diagnostic);
            diagnostic.Errors.Should().NotBeEmpty();

            foreach (var format in new[] { AsyncApiFormat.Yaml, AsyncApiFormat.Json })
            {
                var act = () => Serialize(document, version, format, ReferenceInlineSetting.InlineReferences);

                act.Should().Throw<AsyncApiWriterException>()
                    .WithMessage("Cannot inline '#/components/schemas/Missing': the reference could not be resolved.");
                Serialize(document, version, format, ReferenceInlineSetting.DoNotInlineReferences)
                    .Should().Contain("#/components/schemas/Missing");
            }
        }

        /// <summary>
        /// Reads each document, serializes it as both YAML and JSON,
        /// and expects the same output every time, so the output is stable across round trips.
        /// </summary>
        private static void AssertRoundTrip(AsyncApiVersion version, ReferenceInlineSetting referenceInline, string input, string expectedYaml, string expectedJson)
        {
            foreach (var source in new[] { input, expectedYaml, expectedJson })
            {
                var document = new AsyncApiStringReader().Read(source, out var diagnostic);
                diagnostic.Errors.Should().BeEmpty();

                Serialize(document, version, AsyncApiFormat.Yaml, referenceInline).Should().BePlatformAgnosticEquivalentTo(expectedYaml);
                Serialize(document, version, AsyncApiFormat.Json, referenceInline).Should().BePlatformAgnosticEquivalentTo(expectedJson);
            }
        }

        private static string Serialize(AsyncApiDocument document, AsyncApiVersion version, AsyncApiFormat format, ReferenceInlineSetting referenceInline)
        {
            var outputString = new StringWriter();
            var settings = new AsyncApiWriterSettings { ReferenceInline = referenceInline };
            IAsyncApiWriter writer = format == AsyncApiFormat.Json
                ? new AsyncApiJsonWriter(outputString, settings)
                : new AsyncApiYamlWriter(outputString, settings);

            if (version == AsyncApiVersion.AsyncApi2_0)
            {
                document.SerializeV2(writer);
            }
            else
            {
                document.SerializeV3(writer);
            }

            return outputString.ToString();
        }
    }
}
