namespace ByteBard.AsyncAPI.Tests.Bindings
{
    using System.Linq;
    using ByteBard.AsyncAPI.Bindings;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers;
    using FluentAssertions;
    using NUnit.Framework;

    public class BindingsDiagnostics_Should : TestBase
    {
        private const string BindingsWithExtension =
            """
            kafka:
              bindingVersion: 0.4.0
            x-myextension: someValue
            """;

        private const string BindingsWithUnknownBinding =
            """
            kafka:
              bindingVersion: 0.4.0
            unknown:
              bindingVersion: 0.1.0
            """;

        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void ServerBindings_WithExtension_ReadsExtensionWithoutDiagnostics(AsyncApiVersion version)
        {
            AssertExtensionIsRead<IServerBinding>(version);
        }

        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void ChannelBindings_WithExtension_ReadsExtensionWithoutDiagnostics(AsyncApiVersion version)
        {
            AssertExtensionIsRead<IChannelBinding>(version);
        }

        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void OperationBindings_WithExtension_ReadsExtensionWithoutDiagnostics(AsyncApiVersion version)
        {
            AssertExtensionIsRead<IOperationBinding>(version);
        }

        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void MessageBindings_WithExtension_ReadsExtensionWithoutDiagnostics(AsyncApiVersion version)
        {
            AssertExtensionIsRead<IMessageBinding>(version);
        }

        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void ServerBindings_WithUnregisteredBinding_AddsError(AsyncApiVersion version)
        {
            AssertUnregisteredBindingErrors<IServerBinding>(version, "ServerBinding");
        }

        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void ChannelBindings_WithUnregisteredBinding_AddsError(AsyncApiVersion version)
        {
            AssertUnregisteredBindingErrors<IChannelBinding>(version, "ChannelBinding");
        }

        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void OperationBindings_WithUnregisteredBinding_AddsError(AsyncApiVersion version)
        {
            AssertUnregisteredBindingErrors<IOperationBinding>(version, "OperationBinding");
        }

        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void MessageBindings_WithUnregisteredBinding_AddsError(AsyncApiVersion version)
        {
            AssertUnregisteredBindingErrors<IMessageBinding>(version, "MessageBinding");
        }

        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void ServerBindings_WithRegisteredAndUnregisteredBinding_ReserializesOnlyRegisteredBinding(AsyncApiVersion version)
        {
            AssertUnregisteredBindingIsNotReserialized<IServerBinding>(version);
        }

        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void ChannelBindings_WithRegisteredAndUnregisteredBinding_ReserializesOnlyRegisteredBinding(AsyncApiVersion version)
        {
            AssertUnregisteredBindingIsNotReserialized<IChannelBinding>(version);
        }

        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void OperationBindings_WithRegisteredAndUnregisteredBinding_ReserializesOnlyRegisteredBinding(AsyncApiVersion version)
        {
            AssertUnregisteredBindingIsNotReserialized<IOperationBinding>(version);
        }

        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void MessageBindings_WithRegisteredAndUnregisteredBinding_ReserializesOnlyRegisteredBinding(AsyncApiVersion version)
        {
            AssertUnregisteredBindingIsNotReserialized<IMessageBinding>(version);
        }

        private static AsyncApiBindings<TBinding> Read<TBinding>(string input, AsyncApiVersion version, out AsyncApiDiagnostic diagnostic)
            where TBinding : IBinding
        {
            var settings = new AsyncApiReaderSettings();
            settings.Bindings = BindingsCollection.Kafka;
            return new AsyncApiStringReader(settings).ReadFragment<AsyncApiBindings<TBinding>>(input, version, out diagnostic);
        }

        private static void AssertExtensionIsRead<TBinding>(AsyncApiVersion version)
            where TBinding : IBinding
        {
            // Act
            var bindings = Read<TBinding>(BindingsWithExtension, version, out var diagnostic);

            // Assert
            diagnostic.Errors.Should().BeEmpty();
            diagnostic.Warnings.Should().BeEmpty();
            bindings.Keys.Should().BeEquivalentTo("kafka");
            bindings.Extensions.Keys.Should().BeEquivalentTo("x-myextension");
        }

        private static void AssertUnregisteredBindingErrors<TBinding>(AsyncApiVersion version, string bindingType)
            where TBinding : IBinding
        {
            // Act
            var bindings = Read<TBinding>(BindingsWithUnknownBinding, version, out var diagnostic);

            // Assert
            diagnostic.Errors.Select(e => e.Message).Should().BeEquivalentTo($"{bindingType} unknown is not found");
            diagnostic.Warnings.Should().BeEmpty();
            bindings.Keys.Should().BeEquivalentTo("kafka");
        }

        private static void AssertUnregisteredBindingIsNotReserialized<TBinding>(AsyncApiVersion version)
            where TBinding : IBinding
        {
            // Arrange
            var bindings = Read<TBinding>(BindingsWithUnknownBinding, version, out var diagnostic);

            // Act
            var reserialized = bindings.SerializeAsYaml(version);

            // Assert
            // The model has no way to hold a binding without a parser, so it is lost on write.
            // That is why reading it must stay an error.
            diagnostic.Errors.Should().ContainSingle();
            reserialized.Should().Contain("kafka");
            reserialized.Should().NotContain("unknown");
        }
    }
}
