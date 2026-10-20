namespace ByteBard.AsyncAPI.Tests
{
    using System.Text.Json.Nodes;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using FluentAssertions;
    using NUnit.Framework;

    internal class ReaderErrorDetails_Should : TestBase
    {
        [TestCase("abc", "Value 'abc' could not parse to integer. Expected a whole number.")]
        [TestCase("1.5", "Value '1.5' could not parse to integer. Expected a whole number.")]
        [TestCase("+-1", "Value '+-1' could not parse to integer. Expected a whole number.")]
        [TestCase("--1", "Value '--1' could not parse to integer. Expected a whole number.")]
        [TestCase("99999999999","Value '99999999999' could not parse to integer. It must be between -2147483648 and 2147483647.")]
        public void SchemaIntegerField_ReportsTheValueAndWhyItIsInvalid(string value, string expectedMessage)
        {
            var yaml = $"type: string\nmaxLength: '{value}'";

            new AsyncApiStringReader().ReadFragment<AsyncApiJsonSchema>(yaml, AsyncApiVersion.AsyncApi3_0, out var diagnostics);

            var error = diagnostics.Errors.Should().ContainSingle().Subject;
            error.Message.Should().Be(expectedMessage);
            error.Pointer.Should().Be("#/maxLength");
        }

        [TestCase("uniqueItems")]
        [TestCase("readOnly")]
        [TestCase("writeOnly")]
        [TestCase("deprecated")]
        public void SchemaBooleanField_ReportsTheValueAndWhyItIsInvalid(string field)
        {
            var yaml = $"type: string\n{field}: 'yes please'";

            new AsyncApiStringReader().ReadFragment<AsyncApiJsonSchema>(yaml, AsyncApiVersion.AsyncApi3_0, out var diagnostics);

            var error = diagnostics.Errors.Should().ContainSingle().Subject;
            error.Message.Should().Be("Value 'yes please' could not parse to bool. Expected true or false.");
            error.Pointer.Should().Be($"#/{field}");
        }

        [Test]
        public void MissingLocalReference_IsReportedOnce()
        {
            var yaml =
                """
                asyncapi: 3.0.0
                info:
                  title: test
                  version: 1.0.0
                channels:
                  users:
                    messages:
                      created:
                        payload:
                          $ref: '#/components/schemas/Missing'
                """;

            new AsyncApiStringReader().Read(yaml, out var diagnostics);

            diagnostics.Errors.Should().ContainSingle()
                .Which.Message.Should().Be("Could not resolve reference fragment.");
        }

        [TestCase("/info/title/nested")]
        [TestCase("/tags/5")]
        [TestCase("/tags/name")]
        [TestCase("/tags/-0")]
        [TestCase("/tags/+0")]
        [TestCase("/tags/ 0")]
        [TestCase("/tags/00")]
        [TestCase("/tags/01")]
        [TestCase("/missing")]
        public void Find_ReturnsNull_WhenThePointerDoesNotLeadToANode(string pointer)
        {
            var json = JsonNode.Parse("""{ "info": { "title": "test" }, "tags": [ { "name": "a" } ] }""");

            new JsonPointer(pointer).Find(json).Should().BeNull();
        }

        [Test]
        public void Find_FollowsObjectsAndArrays()
        {
            var json = JsonNode.Parse("""{ "tags": [ { "name": "a" } ] }""");

            new JsonPointer("/tags/0/name").Find(json).GetValue<string>().Should().Be("a");
        }
    }
}
