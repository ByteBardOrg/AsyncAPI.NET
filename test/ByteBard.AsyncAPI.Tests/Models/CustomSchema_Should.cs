namespace ByteBard.AsyncAPI.Tests.Models;

using System.Collections.Generic;
using System.Linq;
using AsyncAPI.Models;
using AsyncAPI.Models.Interfaces;
using AsyncAPI.Writers;
using FluentAssertions;
using NUnit.Framework;
using Readers;
using Readers.ParseNodes;

public class CustomSchema_Should
{
    public enum MySchemaType
    {
        One,
        Two,
    }

    public class MySchema : IAsyncApiSchema
    {
        public MySchemaType Type { get; set; }

        public int Length { get; set; }

        public string Description { get; set; }

        public IEnumerable<MySchema> Children { get; set; }

        public void SerializeV2(IAsyncApiWriter writer)
        {
            // not being tested
            throw new System.NotImplementedException();
        }

        public void SerializeV3(IAsyncApiWriter writer)
        {
            // not being tested
            throw new System.NotImplementedException();
        }
    }

    public class MySchemaParser : IAsyncApiSchemaParser
    {
        private readonly static FixedFieldMap<MySchema> SchemaFixedFields = new()
        {
            {
                "type",
                (schema, node) => { schema.Type = node.GetScalarValue().GetEnumFromDisplayName<MySchemaType>(); }
            },
            {
                "length", (schema, node) => { schema.Length = node.GetIntegerValue(); }
            },
            {
                "description", (schema, node) => { schema.Description = node.GetScalarValueOrDefault("No description"); }
            },
            {
                "children", (schema, node) => { schema.Children = node.CreateList(Parse); }
            },
        };

        private static MySchema Parse(ParseNode node)
        {
            var mapNode = node.CheckMapNode("arbitrary string");

            var schema = new MySchema();

            // map each propery against the fixedFieldMap
            foreach (var property in mapNode)
            {
                property.ParseField(schema, SchemaFixedFields, null);
            }

            return schema;
        }

        public IAsyncApiSchema LoadSchema(ParseNode node)
        {
            return Parse(node);
        }

        public IEnumerable<string> SupportedFormats => new string[] { "application/myschema+json" };
    }

    [Test]
    public void Document_WithCustomSchema_Deserializes()
    {
        // Arrange
        var settings = new AsyncApiReaderSettings();
        settings.SchemaParserRegistry.RegisterParser(new MySchemaParser());

        var reader = new AsyncApiStringReader(settings);

        var input =
            """
            asyncapi: 3.0.0
            info:
              title: test
              version: 1.0.0
            
            channels:
              workspace:
                messages:
                  workspaceEvent:
                    $ref: '#/components/messages/WorkspaceEventPayload'
            components:
              messages:
                WorkspaceEventPayload:
                  contentType: text/plain
                  payload:
                    schemaFormat: application/myschema+json
                    schema:
                        type: one
                        description: a test description
                        length: 1
                        children:
                            - type: two
                              length: 2
            
            """;

        // Act
        var document = reader.Read(input, out var diagnostic);

        // Assert
        diagnostic.Errors.Should().HaveCount(0);

        var schema = document.Components.Messages.Values.First().Payload;
        schema.SchemaFormat.Should().Be("application/myschema+json");

        var myschema = schema.Schema.As<MySchema>();
        myschema.Description.Should().Be("a test description");
        myschema.Length.Should().Be(1);
        myschema.Type.Should().Be(MySchemaType.One);
        myschema.Children.Should().HaveCount(1);
        var child = myschema.Children.First();

        child.Type.Should().Be(MySchemaType.Two);
        child.Length.Should().Be(2);
    }
}