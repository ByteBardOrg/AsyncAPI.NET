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
        public void V2_StringOrStringList_IsInitialised_WhenPassedStringOrStringList()
        {
            // Arrange
            var stringValue = new StringOrStringList(new AsyncApiAny("AsyncApi"));
            var listValue = new StringOrStringList(
                new AsyncApiAny(new List<string>()
                {
                    "Async",
                    "Api",
                }));

            // Assert
            stringValue.Value.GetValue<string>().Should().Be("AsyncApi");
            listValue.Value.GetValue<List<string>>()
                .Should().BeEquivalentTo(new List<string>() { "Async", "Api" });
        }

        [Test]
        public void V2_StringOrStringList_ThrowsArgumentException_WhenIntialisedWithoutStringOrStringList()
        {
            // Assert
            var ex = Assert.Throws<ArgumentException>(() => new StringOrStringList(new AsyncApiAny(true)));

            // Assert
            ex.Message.Should().Be("StringOrStringList should be a string value or a string list.");
        }

        [Test]
        public void V2_StringOrStringList_ThrowsArgumentException_WhenIntialisedWithListOfNonStrings()
        {
            // Assert
            var ex = Assert.Throws<ArgumentException>(() => new StringOrStringList(
                new AsyncApiAny(new List<object>()
                {
                    "x",
                    1,
                    "y",
                })));

            // Assert
            ex.Message.Should().Be("StringOrStringList value should only contain string items.");
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
                TestProperty = new StringOrStringList(new AsyncApiAny("someValue")),
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
                TestProperty = new StringOrStringList(new AsyncApiAny(new List<string>
                {
                    "someValue01",
                    "someValue02",
                    "someValue03",
                })),
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
            writer.WriteRequiredObject("testProperty", this.TestProperty, (w, t) => t.Value.Write(w));
            writer.WriteEndObject();
        }

        protected override FixedFieldMap<StringOrStringListTestBinding> FixedFieldMap => new()
        {
            { "testProperty", (a, n) => { a.TestProperty = new StringOrStringList(n.CreateAny()); } },
        };
    }
}