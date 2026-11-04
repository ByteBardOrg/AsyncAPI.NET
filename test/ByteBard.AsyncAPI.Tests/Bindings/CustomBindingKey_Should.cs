namespace ByteBard.AsyncAPI.Tests.Bindings
{
    using System.Linq;
    using ByteBard.AsyncAPI.Bindings;
    using ByteBard.AsyncAPI.Bindings.Kafka;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers;
    using ByteBard.AsyncAPI.Readers.Interface;
    using FluentAssertions;
    using NUnit.Framework;

    public class CustomBindingKey_Should : TestBase
    {
        [Test]
        public void V3_CustomBinding_SerializesUnderExtensionKey()
        {
            // Arrange
            var channel = new AsyncApiChannel();
            channel.Bindings.Add(new MyBinding { Custom = "someValue", Any = new AsyncApiAny("anyValue") });

            // Act
            var actual = channel.SerializeAsYaml(AsyncApiVersion.AsyncApi3_0);

            // Assert
            actual.Should().BePlatformAgnosticEquivalentTo(
                """
                bindings:
                  x-my:
                    custom: someValue
                    any: anyValue
                """);
        }

        [Test]
        public void V3_SpecificationBinding_SerializesUnderItsOwnKey()
        {
            // Arrange
            var channel = new AsyncApiChannel();
            channel.Bindings.Add(new KafkaChannelBinding { Topic = "orders" });

            // Act
            var actual = channel.SerializeAsYaml(AsyncApiVersion.AsyncApi3_0);

            // Assert
            actual.Should().Contain("kafka:");
            actual.Should().NotContain("x-kafka");
        }

        [Test]
        public void V2_CustomBinding_SerializesUnderItsOwnKey()
        {
            // Arrange
            var channel = new AsyncApiChannel();
            channel.Bindings.Add(new MyBinding { Custom = "someValue", Any = new AsyncApiAny("anyValue") });

            // Act
            var actual = channel.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);

            // Assert
            actual.Should().Contain("  my:");
            actual.Should().NotContain("x-my");
        }

        [Test]
        public void V3_CustomBindingUnderExtensionKey_ReadsAsBindingWithoutDiagnostics()
        {
            // Arrange
            var input =
                """
                bindings:
                  x-my:
                    custom: someValue
                    any: anyValue
                """;

            // Act
            var channel = ReadChannel(input, AsyncApiVersion.AsyncApi3_0, out var diagnostic);

            // Assert
            diagnostic.Errors.Should().BeEmpty();
            diagnostic.Warnings.Should().BeEmpty();
            channel.Bindings.Keys.Should().BeEquivalentTo("my");
            channel.Bindings.Extensions.Should().BeEmpty();
            ((MyBinding)channel.Bindings["my"]).Custom.Should().Be("someValue");
            channel.SerializeAsYaml(AsyncApiVersion.AsyncApi3_0).Should().BePlatformAgnosticEquivalentTo(input);
        }

        [Test]
        public void V3_CustomBindingWithoutExtensionPrefix_AddsErrorAndReadsBinding()
        {
            // Arrange
            var input =
                """
                bindings:
                  my:
                    custom: someValue
                    any: anyValue
                """;

            // Act
            var channel = ReadChannel(input, AsyncApiVersion.AsyncApi3_0, out var diagnostic);

            // Assert
            diagnostic.Errors.Select(e => e.Message).Should().BeEquivalentTo(
                "ChannelBinding my is not an AsyncAPI 3 binding name; write a custom binding as x-my");
            channel.Bindings.Keys.Should().BeEquivalentTo("my");
        }

        [Test]
        public void V2_CustomBindingWithoutExtensionPrefix_ReadsWithoutDiagnostics()
        {
            // Arrange
            var input =
                """
                bindings:
                  my:
                    custom: someValue
                    any: anyValue
                """;

            // Act
            var channel = ReadChannel(input, AsyncApiVersion.AsyncApi2_0, out var diagnostic);

            // Assert
            diagnostic.Errors.Should().BeEmpty();
            channel.Bindings.Keys.Should().BeEquivalentTo("my");
        }

        [Test]
        public void V3_ExtensionWithoutRegisteredBinding_ReadsAsExtension()
        {
            // Arrange
            var input =
                """
                bindings:
                  x-other:
                    custom: someValue
                """;

            // Act
            var channel = ReadChannel(input, AsyncApiVersion.AsyncApi3_0, out var diagnostic);

            // Assert
            diagnostic.Errors.Should().BeEmpty();
            channel.Bindings.Keys.Should().BeEmpty();
            channel.Bindings.Extensions.Keys.Should().BeEquivalentTo("x-other");
        }

        [TestCase("kafka", true)]
        [TestCase("ws", true)]
        [TestCase("ros2", true)]
        [TestCase("mqtt5", false)]
        [TestCase("signalr", false)]
        [TestCase("x-signalr", false)]
        public void IsSpecificationKey_ReturnsWhetherAsyncApi3DefinesTheBinding(string key, bool expected)
        {
            AsyncApiBindingKeys.IsSpecificationKey(key).Should().Be(expected);
        }

        [TestCase("kafka", "kafka")]
        [TestCase("signalr", "x-signalr")]
        [TestCase("x-signalr", "x-signalr")]
        [TestCase("mqtt5", "x-mqtt5")]
        public void ToV3Key_PrefixesKeysAsyncApi3DoesNotDefine(string key, string expected)
        {
            AsyncApiBindingKeys.ToV3Key(key).Should().Be(expected);
        }

        private static AsyncApiChannel ReadChannel(string input, AsyncApiVersion version, out AsyncApiDiagnostic diagnostic)
        {
            var settings = new AsyncApiReaderSettings();
            settings.Bindings = BindingsCollection.Kafka;
            settings.Bindings.Add(new MyBinding());
            return new AsyncApiStringReader(settings).ReadFragment<AsyncApiChannel>(input, version, out diagnostic);
        }
    }
}
