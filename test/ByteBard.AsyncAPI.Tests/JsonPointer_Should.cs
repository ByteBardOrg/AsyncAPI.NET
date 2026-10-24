namespace ByteBard.AsyncAPI.Tests
{
    using System;
    using System.Text.Json.Nodes;
    using ByteBard.AsyncAPI.Exceptions;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using FluentAssertions;
    using NUnit.Framework;

    internal class JsonPointer_Should
    {
        // The example document from RFC 6901 section 5.
        private const string RfcDocument =
            """
            {
              "foo": ["bar", "baz"],
              "": 0,
              "a/b": 1,
              "c%d": 2,
              "e^f": 3,
              "g|h": 4,
              "i\\j": 5,
              "k\"l": 6,
              " ": 7,
              "m~n": 8
            }
            """;

        [TestCase("/foo/0", "\"bar\"")]
        [TestCase("/", "0")]
        [TestCase("/a~1b", "1")]
        [TestCase("/c%d", "2")]
        [TestCase("/e^f", "3")]
        [TestCase("/g|h", "4")]
        [TestCase("/i\\j", "5")]
        [TestCase("/k\"l", "6")]
        [TestCase("/ ", "7")]
        [TestCase("/m~0n", "8")]
        public void Find_ResolvesTheRfcStringRepresentationExamples(string pointer, string expected)
        {
            var json = JsonNode.Parse(RfcDocument);

            new JsonPointer(pointer).Find(json).ToJsonString().Should().Be(expected);
        }

        [TestCase("/foo/0", "\"bar\"")]
        [TestCase("/", "0")]
        [TestCase("/a~1b", "1")]
        [TestCase("/c%25d", "2")]
        [TestCase("/e%5Ef", "3")]
        [TestCase("/g%7Ch", "4")]
        [TestCase("/i%5Cj", "5")]
        [TestCase("/k%22l", "6")]
        [TestCase("/%20", "7")]
        [TestCase("/m~0n", "8")]
        public void Find_ResolvesTheRfcUriFragmentExamples(string fragment, string expected)
        {
            var json = JsonNode.Parse(RfcDocument);

            JsonPointer.FromUriFragment(fragment).Find(json).ToJsonString().Should().Be(expected);
        }

        [Test]
        public void EmptyPointer_ReferencesTheWholeDocument()
        {
            var json = JsonNode.Parse(RfcDocument);

            new JsonPointer(string.Empty).Find(json).Should().BeSameAs(json);
            JsonPointer.FromUriFragment(string.Empty).Find(json).Should().BeSameAs(json);
        }

        [Test]
        public void StringRepresentation_IsNotPercentDecoded()
        {
            var json = JsonNode.Parse("""{ "a b": 1 }""");

            new JsonPointer("/a%20b").Find(json).Should().BeNull();
        }

        [TestCase("plain", "plain")]
        [TestCase("a/b", "a~1b")]
        [TestCase("a~b", "a~0b")]
        [TestCase("~1", "~01")]
        public void EscapeToken_EscapesTildeBeforeSlash(string token, string expected)
        {
            JsonPointer.EscapeToken(token).Should().Be(expected);
            new JsonPointer("/" + expected).Tokens.Should().Equal(token);
        }

        [Test]
        public void Unescape_DecodesTilde1BeforeTilde0()
        {
            new JsonPointer("/~01").Tokens.Should().Equal("~1");
        }

        [TestCase("foo")]
        [TestCase("#/foo")]
        [TestCase("/foo~")]
        [TestCase("/foo~2")]
        [TestCase("/~a")]
        public void Constructor_Throws_WhenThePointerIsMalformed(string pointer)
        {
            var act = () => new JsonPointer(pointer);

            act.Should().Throw<AsyncApiException>();
        }

        [TestCase("foo")]
        [TestCase("/foo~2")]
        [TestCase("/foo%7E2")]
        public void FromUriFragment_Throws_WhenTheDecodedPointerIsMalformed(string fragment)
        {
            var act = () => JsonPointer.FromUriFragment(fragment);

            act.Should().Throw<AsyncApiException>();
        }

        [Test]
        public void Constructor_Throws_WhenThePointerIsNull()
        {
            var act = () => new JsonPointer(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [TestCase("")]
        [TestCase("/")]
        [TestCase("/foo/0")]
        [TestCase("/a~1b/m~0n")]
        [TestCase("//")]
        public void ToString_RoundTripsTheStringRepresentation(string pointer)
        {
            new JsonPointer(pointer).ToString().Should().Be(pointer);
        }

        [Test]
        public void ParentPointer_OfASingleTokenPointer_IsTheWholeDocument()
        {
            new JsonPointer("/foo").ParentPointer.ToString().Should().BeEmpty();
            new JsonPointer(string.Empty).ParentPointer.Should().BeNull();
        }
    }
}
