namespace ByteBard.AsyncAPI.Tests.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using ByteBard.AsyncAPI.Extensions;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers;
    using ByteBard.AsyncAPI.Validations;
    using NUnit.Framework;

    public class AvroSchema_Should
    {
        [Test]
        public void V2_Serialize_WithDefaultNull_SetJsonNull()
        {
            var input = """
            type: record
            name: User
            namespace: Producer
            doc: ESP Schema validation test
            fields:
              - name: userId
                type: int
              - name: userEmail
                type:
                  - null
                  - string
                default: null
            """;

            var model = new AsyncApiStringReader().ReadFragment<AsyncApiAvroSchema>(input, AsyncApiVersion.AsyncApi2_0, out var diag);
            var reserialized = model.SerializeAsJson(AsyncApiVersion.AsyncApi2_0);
            reserialized.Should().Contain("default\": null");
        }

        [Test]
        public void V2_Deserialize_WithMetadata_CreatesMetadata()
        {
            var input =
                """
                {
                  "type": "record",
                  "name": "SomeEvent",
                  "namespace": "my.namspace.for.event",
                  "fields": [
                    {
                      "name": "countryCode",
                      "type": "string",
                      "doc": "Country of the partner, (e.g. DE)"
                    },
                    {
                      "name": "occurredOn",
                      "type": "string",
                      "doc": "Timestamp of when action occurred."
                    },
                    {
                      "name": "partnerId",
                      "type": "string",
                      "doc": "Id of the partner"
                    },
                    {
                      "name": "platformSource",
                      "type": "string",
                      "doc": "Platform source"
                    }
                  ],
                  "example": {
                    "occurredOn": "2023-11-03T09:56.582+00:00",
                    "partnerId": "1",
                    "platformSource": "Brecht",
                    "countryCode": "DE"
                  }
                }
                """;
            var model = new AsyncApiStringReader().ReadFragment<AsyncApiAvroSchema>(input, AsyncApiVersion.AsyncApi2_0, out var diag);
            model.Metadata.Should().HaveCount(1);
            var reserialized = model.SerializeAsJson(AsyncApiVersion.AsyncApi2_0);

            // Assert
            input.Should()
                  .BePlatformAgnosticEquivalentTo(reserialized);

        }

        [Test]
        public void V2_SerializeV2_SerializesCorrectly()
        {
            // Arrange
            var expected = """
            type: record
            name: User
            namespace: com.example
            fields:
              - name: username
                type: string
                doc: The username of the user.
                default: guest
                order: ascending
              - name: status
                type:
                  type: enum
                  name: Status
                  symbols:
                    - ACTIVE
                    - INACTIVE
                    - BANNED
                doc: The status of the user.
              - name: emails
                type:
                  type: array
                  items: string
                doc: A list of email addresses.
              - name: metadata
                type:
                  type: map
                  values: string
                doc: Metadata associated with the user.
              - name: address
                type:
                  type: record
                  name: Address
                  fields:
                    - name: street
                      type: string
                    - name: city
                      type: string
                    - name: zipcode
                      type: string
                doc: The address of the user.
              - name: profilePicture
                type:
                  type: fixed
                  name: ProfilePicture
                  size: 256
                doc: A fixed-size profile picture.
              - name: contact
                type:
                  - 'null'
                  - type: record
                    name: PhoneNumber
                    fields:
                      - name: countryCode
                        type: int
                      - name: number
                        type: string
                doc: 'The contact information of the user, which can be either null or a phone number.'
            """;

            var schema = new AvroRecord
            {
                Name = "User",
                Namespace = "com.example",
                Fields = new List<AvroField>
                {
                    new AvroField
                    {
                        Name = "username",
                        Type = AvroPrimitiveType.String,
                        Doc = "The username of the user.",
                        Default = new AsyncApiAny("guest"),
                        Order = AvroFieldOrder.Ascending,
                    },
                    new AvroField
                    {
                        Name = "status",
                        Type = new AvroEnum
                        {
                            Name = "Status",
                            Symbols = new List<string> { "ACTIVE", "INACTIVE", "BANNED" },
                        },
                        Doc = "The status of the user.",
                    },
                    new AvroField
                    {
                        Name = "emails",
                        Type = new AvroArray
                        {
                            Items = AvroPrimitiveType.String,
                        },
                        Doc = "A list of email addresses.",
                    },
                    new AvroField
                    {
                        Name = "metadata",
                        Type = new AvroMap
                        {
                            Values = AvroPrimitiveType.String,
                        },
                        Doc = "Metadata associated with the user.",
                    },
                    new AvroField
                    {
                        Name = "address",
                        Type = new AvroRecord
                        {
                            Name = "Address",
                            Fields = new List<AvroField>
                            {
                                new AvroField { Name = "street", Type = AvroPrimitiveType.String },
                                new AvroField { Name = "city", Type = AvroPrimitiveType.String },
                                new AvroField { Name = "zipcode", Type = AvroPrimitiveType.String },
                            },
                        },
                        Doc = "The address of the user.",
                    },
                    new AvroField
                    {
                        Name = "profilePicture",
                        Type = new AvroFixed
                        {
                            Name = "ProfilePicture",
                            Size = 256,
                        },
                        Doc = "A fixed-size profile picture.",
                    },
                    new AvroField
                    {
                        Name = "contact",
                        Type = new AvroUnion
                        {
                            Types = new List<AsyncApiAvroSchema>
                            {
                                AvroPrimitiveType.Null,
                                new AvroRecord
                                {
                                    Name = "PhoneNumber",
                                    Fields = new List<AvroField>
                                    {
                                        new AvroField { Name = "countryCode", Type = AvroPrimitiveType.Int },
                                        new AvroField { Name = "number", Type = AvroPrimitiveType.String },
                                    },
                                },
                            },
                        },
                        Doc = "The contact information of the user, which can be either null or a phone number.",
                    },
                },
            };

            // Act
            var actual = schema.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);

            // Assert
            actual.Should()
                  .BePlatformAgnosticEquivalentTo(expected);
        }

        [Test]
        public void V2_SerializeV2_WithLogicalTypes_SerializesCorrectly()
        {
            // Arrange
            var input = """
                        {
                          "type": "array",
                          "items": [
                            {
                              "type": "bytes",
                              "logicalType": "decimal",
                              "scale": 2,
                              "precision": 4
                            },
                            {
                              "type": "string",
                              "logicalType": "uuid"
                            },
                            {
                              "type": "int",
                              "logicalType": "date"
                            },
                            {
                              "type": "int",
                              "logicalType": "time-millis"
                            },
                            {
                              "type": "long",
                              "logicalType": "time-micros"
                            },
                            {
                              "type": "long",
                              "logicalType": "timestamp-millis"
                            },
                            {
                              "type": "long",
                              "logicalType": "timestamp-micros"
                            },
                            {
                              "type": "fixed",
                              "logicalType": "duration",
                              "name": "Duration",
                              "size": 12
                            }
                          ]
                        }
                        """;

            // Act
            var model = new AsyncApiStringReader().ReadFragment<AsyncApiAvroSchema>(input, AsyncApiVersion.AsyncApi2_0, out var diag);
            var serialized = model.SerializeAsJson(AsyncApiVersion.AsyncApi2_0);
            // Assert
            model.As<AvroArray>().Items.As<AvroUnion>().Types.Should().HaveCount(8);

            serialized.Should().BePlatformAgnosticEquivalentTo(input);
        }

        [Test]
        public void V2_ReadFragment_DeserializesCorrectly()
        {
            // Arrange
            var input = """
            type: record
            name: User
            namespace: com.example
            fields:
              - name: username
                type: string
                doc: The username of the user.
                default: guest
                order: ascending
              - name: status
                type:
                  type: enum
                  name: Status
                  symbols:
                    - ACTIVE
                    - INACTIVE
                    - BANNED
                doc: The status of the user.
              - name: emails
                type:
                  type: array
                  items: string
                doc: A list of email addresses.
              - name: metadata
                type:
                  type: map
                  values: string
                doc: Metadata associated with the user.
              - name: address
                type:
                  type: record
                  name: Address
                  fields:
                    - name: street
                      type: string
                    - name: city
                      type: string
                    - name: zipcode
                      type: string
                doc: The address of the user.
              - name: profilePicture
                type:
                  type: fixed
                  name: ProfilePicture
                  size: 256
                doc: A fixed-size profile picture.
              - name: contact
                type:
                  - 'null'
                  - type: record
                    name: PhoneNumber
                    fields:
                      - name: countryCode
                        type: int
                      - name: number
                        type: string
                doc: 'The contact information of the user, which can be either null or a phone number.'
            """;

            var expected = new AvroRecord
            {
                Name = "User",
                Namespace = "com.example",
                Fields = new List<AvroField>
                {
                    new AvroField
                    {
                        Name = "username",
                        Type = AvroPrimitiveType.String,
                        Doc = "The username of the user.",
                        Default = new AsyncApiAny("guest"),
                        Order = AvroFieldOrder.Ascending,
                    },
                    new AvroField
                    {
                        Name = "status",
                        Type = new AvroEnum
                        {
                            Name = "Status",
                            Symbols = new List<string> { "ACTIVE", "INACTIVE", "BANNED" },
                        },
                        Doc = "The status of the user.",
                    },
                    new AvroField
                    {
                        Name = "emails",
                        Type = new AvroArray
                        {
                            Items = AvroPrimitiveType.String,
                        },
                        Doc = "A list of email addresses.",
                    },
                    new AvroField
                    {
                        Name = "metadata",
                        Type = new AvroMap
                        {
                            Values = AvroPrimitiveType.String,
                        },
                        Doc = "Metadata associated with the user.",
                    },
                    new AvroField
                    {
                        Name = "address",
                        Type = new AvroRecord
                        {
                            Name = "Address",
                            Fields = new List<AvroField>
                            {
                                new AvroField { Name = "street", Type = AvroPrimitiveType.String },
                                new AvroField { Name = "city", Type = AvroPrimitiveType.String },
                                new AvroField { Name = "zipcode", Type = AvroPrimitiveType.String },
                            },
                        },
                        Doc = "The address of the user.",
                    },
                    new AvroField
                    {
                        Name = "profilePicture",
                        Type = new AvroFixed
                        {
                            Name = "ProfilePicture",
                            Size = 256,
                        },
                        Doc = "A fixed-size profile picture.",
                    },
                    new AvroField
                    {
                        Name = "contact",
                        Type = new AvroUnion
                        {
                            Types = new List<AsyncApiAvroSchema>
                            {
                                AvroPrimitiveType.Null,
                                new AvroRecord
                                {
                                    Name = "PhoneNumber",
                                    Fields = new List<AvroField>
                                    {
                                        new AvroField { Name = "countryCode", Type = AvroPrimitiveType.Int },
                                        new AvroField { Name = "number", Type = AvroPrimitiveType.String },
                                    },
                                },
                            },
                        },
                        Doc = "The contact information of the user, which can be either null or a phone number.",
                    },
                },
            };

            // Act
            var actual = new AsyncApiStringReader().ReadFragment<AsyncApiAvroSchema>(input, AsyncApiVersion.AsyncApi2_0, out var diagnostic);

            // Assert
            actual.Should()
                  .BeEquivalentTo(expected);
        }

        [Test]
        public void V2_ReadFragment_WithRecursiveNamedType_DeserializesCorrectly()
        {
            var input = """
            {
              "type": "record",
              "name": "LongList",
              "aliases": ["LinkedLongs"],
              "fields" : [
                {"name": "value", "type": "long"},
                {"name": "next", "type": ["null", "LongList"]}
              ]
            }
            """;

            var actual = new AsyncApiStringReader().ReadFragment<AsyncApiAvroSchema>(input, AsyncApiVersion.AsyncApi2_0, out var diagnostic);

            diagnostic.Errors.Should().BeEmpty();

            var record = actual.As<AvroRecord>();
            var union = record.Fields[1].Type.As<AvroUnion>();
            var namedType = union.Types[1].As<AvroNamedType>();

            namedType.Name.Should().Be("LongList");
            namedType.Target.Should().BeSameAs(record);

            var serialized = actual.SerializeAsJson(AsyncApiVersion.AsyncApi2_0);
            serialized.Should().Contain("\"LongList\"");
            serialized.Should().NotContain("$ref");

            actual.Validate(ValidationRuleSet.GetDefaultRuleSet())
                  .OfType<AsyncApiValidatorWarning>()
                  .Should()
                  .BeEmpty();
        }

        [Test]
        public void V2_Serialize_WithRecursiveNamedType_WritesNamedTypeAsString()
        {
            var expected = """
            type: record
            name: LongList
            fields:
              - name: value
                type: long
              - name: next
                type:
                  - 'null'
                  - LongList
            """;

            var record = new AvroRecord
            {
                Name = "LongList",
            };

            record.Fields = new List<AvroField>
            {
                new AvroField
                {
                    Name = "value",
                    Type = AvroPrimitiveType.Long,
                },
                new AvroField
                {
                    Name = "next",
                    Type = new AvroUnion
                    {
                        Types = new List<AsyncApiAvroSchema>
                        {
                            AvroPrimitiveType.Null,
                            new AvroNamedType("LongList", record),
                        },
                    },
                },
            };

            var actual = record.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);

            actual.Should().BePlatformAgnosticEquivalentTo(expected);
        }

        [Test]
        public void V2_ReadFragment_WithMapValuesNamedType_DeserializesCorrectly()
        {
            var input = """
            {
              "type": "record",
              "name": "Container",
              "namespace": "example",
              "fields" : [
                {
                  "name": "item",
                  "type": {
                    "type": "record",
                    "name": "Item",
                    "fields": [
                      {"name": "id", "type": "string"}
                    ]
                  }
                },
                {
                  "name": "itemsByKey",
                  "type": {
                    "type": "map",
                    "values": "Item"
                  }
                }
              ]
            }
            """;

            var actual = new AsyncApiStringReader().ReadFragment<AsyncApiAvroSchema>(input, AsyncApiVersion.AsyncApi2_0, out var diagnostic);

            diagnostic.Errors.Should().BeEmpty();

            var record = actual.As<AvroRecord>();
            var item = record.Fields[0].Type.As<AvroRecord>();
            var map = record.Fields[1].Type.As<AvroMap>();
            var namedType = map.Values.As<AvroNamedType>();

            namedType.Name.Should().Be("Item");
            namedType.Target.Should().BeSameAs(item);

            var serialized = actual.SerializeAsJson(AsyncApiVersion.AsyncApi2_0);
            serialized.Should().Contain("\"values\": \"Item\"");
            serialized.Should().NotContain("$ref");

            actual.Validate(ValidationRuleSet.GetDefaultRuleSet())
                  .OfType<AsyncApiValidatorWarning>()
                  .Should()
                  .BeEmpty();
        }

        [Test]
        public void V2_Validate_WithUnresolvedNamedType_CreatesWarning()
        {
            var input = """
            {
              "type": "record",
              "name": "Container",
              "fields" : [
                {"name": "missing", "type": "MissingType"}
              ]
            }
            """;

            var actual = new AsyncApiStringReader().ReadFragment<AsyncApiAvroSchema>(input, AsyncApiVersion.AsyncApi2_0, out var diagnostic);

            diagnostic.Errors.Should().BeEmpty();
            diagnostic.Warnings.Should()
                      .ContainSingle(w => w.Message == "Avro named type 'MissingType' is referenced but was not defined before use.");

            actual.Validate(ValidationRuleSet.GetDefaultRuleSet())
                  .OfType<AsyncApiValidatorWarning>()
                  .Should()
                  .ContainSingle(w => w.Message == "Avro named type 'MissingType' is referenced but was not defined before use.");
        }

        [Test]
        public void V2_Validate_WithUnresolvedMapValuesNamedType_CreatesWarning()
        {
            var input = """
            {
              "type": "record",
              "name": "Container",
              "fields" : [
                {
                  "name": "itemsByKey",
                  "type": {
                    "type": "map",
                    "values": "MissingType"
                  }
                }
              ]
            }
            """;

            var actual = new AsyncApiStringReader().ReadFragment<AsyncApiAvroSchema>(input, AsyncApiVersion.AsyncApi2_0, out var diagnostic);

            diagnostic.Errors.Should().BeEmpty();
            diagnostic.Warnings.Should()
                      .ContainSingle(w => w.Message == "Avro named type 'MissingType' is referenced but was not defined before use.");

            actual.Validate(ValidationRuleSet.GetDefaultRuleSet())
                  .OfType<AsyncApiValidatorWarning>()
                  .Should()
                  .ContainSingle(w => w.Message == "Avro named type 'MissingType' is referenced but was not defined before use.");
        }

        [Test]
        public void V2_ReadDocument_WithRecursiveNamedType_DeserializesAndValidates()
        {
            var input = """
            asyncapi: '2.6.0'
            info:
              title: Avro named type test
              version: '1.0.0'
            channels:
              list:
                publish:
                  message:
                    name: ListMessage
                    payload:
                      type: record
                      name: LongList
                      fields:
                        - name: value
                          type: long
                        - name: next
                          type:
                            - 'null'
                            - LongList
                    schemaFormat: application/vnd.apache.avro
            """;

            var document = new AsyncApiStringReader().Read(input, out var diagnostic);

            diagnostic.Errors.Should().BeEmpty();
            diagnostic.Warnings.Should().BeEmpty();

            var message = document.Operations.Values.First(operation => operation.Action == AsyncApiAction.Receive).Messages.First();
            var record = message.Payload.Schema.As<AvroRecord>();
            var union = record.Fields[1].Type.As<AvroUnion>();
            var namedType = union.Types[1].As<AvroNamedType>();

            namedType.Name.Should().Be("LongList");
            namedType.Target.Should().BeSameAs(record);
        }

        [Test]
        public void V2_ReadDocument_WithUnresolvedNamedType_CreatesWarning()
        {
            var input = """
            asyncapi: '2.6.0'
            info:
              title: Avro named type test
              version: '1.0.0'
            channels:
              list:
                publish:
                  message:
                    name: ListMessage
                    payload:
                      type: record
                      name: LongList
                      fields:
                        - name: value
                          type: long
                        - name: next
                          type:
                            - 'null'
                            - MissingType
                    schemaFormat: application/vnd.apache.avro
            """;

            new AsyncApiStringReader().Read(input, out var diagnostic);

            diagnostic.Errors.Should().BeEmpty();
            diagnostic.Warnings.Should()
                      .ContainSingle(w => w.Message == "Avro named type 'MissingType' is referenced but was not defined before use.");
        }

        [Test]
        public void V2_AvroSchema_WithPrimitiveSchema_ConvertsToPrimitiveType()
        {
            AsyncApiAvroSchema schema = AvroPrimitiveType.String;

            ((AvroPrimitiveType)schema).Should().Be(AvroPrimitiveType.String);
        }
    }
}
