namespace ByteBard.AsyncAPI.Tests.Bindings
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using ByteBard.AsyncAPI.Bindings;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;
    using NUnit.Framework;

    public class StringOrStringList_Should : TestBase
    {
        [Test]
        public void StringOrStringList_ConvertsImplicitlyToAndFromItsAlternatives()
        {
            StringOrStringList stringValue = "AsyncApi";
            StringOrStringList listValue = new List<string> { "Async", "Api" };
            StringOrStringList arrayValue = new[] { "Async", "Api" };

            string single = stringValue.Should().BeOfType<SingleString>().Subject;
            string[] list = listValue.Should().BeOfType<StringList>().Subject;

            single.Should().Be("AsyncApi");
            list.Should().Equal("Async", "Api");
            arrayValue.Should().BeOfType<StringList>().Which.Values.Should().Equal("Async", "Api");
            ((StringOrStringList)(string)null).Should().BeNull();
        }

        [Test]
        public void StringList_RejectsNullItems()
        {
            var act = () => new StringList(new[] { "x", null });

            act.Should().Throw<ArgumentException>().WithMessage("StringList items cannot be null.*");
        }

        [Test]
        public void StringOrStringList_ReportsAMapAsAReaderError()
        {
            var yaml = """
                bindings:
                  testBinding:
                    testProperty:
                      key: value
                """;
            var settings = new AsyncApiReaderSettings { Bindings = new[] { new StringOrStringListTestBinding() } };

            new AsyncApiStringReader(settings).ReadFragment<AsyncApiChannel>(yaml, AsyncApiVersion.AsyncApi2_0, out var diagnostics);

            diagnostics.Errors.Should().ContainSingle()
                .Which.Message.Should().Contain("should contain a string value or a list of strings");
        }

        [Test]
        public void V2_StringOrStringList_WhenValueIsString_SerializesDeserializes()
        {
            // Arrange
            var expected = """
                bindings:
                  testBinding:
                    testProperty: someValue
                """;

            var channel = new AsyncApiChannel();
            channel.Bindings.Add(new StringOrStringListTestBinding
            {
                TestProperty = "someValue",
            });

            // Act
            var actual = channel.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);

            // Assert
            var settings = new AsyncApiReaderSettings();
            settings.Bindings = new[] { new StringOrStringListTestBinding() };
            var binding = new AsyncApiStringReader(settings).ReadFragment<AsyncApiChannel>(actual, AsyncApiVersion.AsyncApi2_0, out _);

            // Assert
            actual.Should()
                  .BePlatformAgnosticEquivalentTo(expected);
            binding.Should().BeEquivalentTo(channel);
        }

        [Test]
        public void V2_StringOrStringList_WhenValueIsStringList_SerializesDeserializes()
        {
            // Arrange
            var expected = """
                bindings:
                  testBinding:
                    testProperty:
                      - someValue01
                      - someValue02
                      - someValue03
                """;

            var channel = new AsyncApiChannel();
            channel.Bindings.Add(new StringOrStringListTestBinding
            {
                TestProperty = new List<string>
                {
                    "someValue01",
                    "someValue02",
                    "someValue03",
                },
            });

            // Act
            var actual = channel.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);

            // Assert
            var settings = new AsyncApiReaderSettings();
            settings.Bindings = new[] { new StringOrStringListTestBinding() };
            var binding = new AsyncApiStringReader(settings).ReadFragment<AsyncApiChannel>(actual, AsyncApiVersion.AsyncApi2_0, out _);

            // Assert
            actual.Should()
                  .BePlatformAgnosticEquivalentTo(expected);
            binding.Should().BeEquivalentTo(channel);
        }
    }

    public class StringOrStringListTestBinding : ChannelBinding<StringOrStringListTestBinding>
    {
        public StringOrStringList TestProperty { get; set; }

        public override string BindingKey => "testBinding";

        public override void SerializeV2(IAsyncApiWriter writer)
        {
            this.SerializeV3(writer);
        }

        public override void SerializeV3(IAsyncApiWriter writer)
        {
            this.SerializeProperties(writer);
        }

        public override void SerializeProperties(IAsyncApiWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteRequiredObject("testProperty", this.TestProperty, (w, t) => t.Write(w));
            writer.WriteEndObject();
        }

        protected override FixedFieldMap<StringOrStringListTestBinding> FixedFieldMap => new()
        {
            { "testProperty", (a, n) => { a.TestProperty = StringOrStringList.Parse(n); } },
        };
    }
}