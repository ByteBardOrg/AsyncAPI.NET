namespace ByteBard.AsyncAPI.Tests.Bindings.GooglePubSub
{
    using System.Collections.Generic;
    using FluentAssertions;
    using ByteBard.AsyncAPI.Bindings;
    using ByteBard.AsyncAPI.Bindings.GooglePubSub;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers;
    using ByteBard.AsyncAPI.Readers.Interface;
    using NUnit.Framework;

    internal class GooglePubSubBindings_Should : TestBase
    {
        private static ICollection<IBindingParser<IBinding>> Parsers => new List<IBindingParser<IBinding>>
        {
            new GooglePubSubChannelBinding(), new GooglePubSubMessageBinding(),
        };

        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void GooglePubSubBindings_RoundTripShape(AsyncApiVersion version)
        {
            var channel = new AsyncApiChannel();
            channel.Bindings.Add(new GooglePubSubChannelBinding
            {
                Labels = new Dictionary<string, string> { ["team"] = "events" },
                MessageRetentionDuration = "86400s",
                MessageStoragePolicy = new MessageStoragePolicy { AllowedPersistenceRegions = new[] { "us-central1", "us-east1" } },
                SchemaSettings = new SchemaSettings { Encoding = "JSON", FirstRevisionId = "1", LastRevisionId = "3", Name = "orders" },
            });

            var channelYaml = channel.SerializeAsYaml(version);
            channelYaml.Should().Contain("messageRetentionDuration: 86400s")
                .And.Contain("allowedPersistenceRegions:")
                .And.Contain("firstRevisionId: '1'");
            var settings = new AsyncApiReaderSettings { Bindings = Parsers };
            var parsedChannel = new AsyncApiStringReader(settings).ReadFragment<AsyncApiChannel>(channelYaml, version, out var channelDiagnostics);
            channelDiagnostics.Errors.Should().BeEmpty();
            parsedChannel.Should().BeEquivalentTo(channel);

            var message = new AsyncApiMessage();
            message.Bindings.Add(new GooglePubSubMessageBinding
            {
                Attributes = new Dictionary<string, string> { ["source"] = "billing" },
                OrderingKey = "customer-7",
                Schema = new MessageSchema { Name = "projects/p/schemas/orders" },
            });
            var messageYaml = message.SerializeAsYaml(version);
            messageYaml.Should().Contain("orderingKey: customer-7").And.Contain("projects/p/schemas/orders");
            var parsedMessage = new AsyncApiStringReader(settings).ReadFragment<AsyncApiMessage>(messageYaml, version, out var messageDiagnostics);
            messageDiagnostics.Errors.Should().BeEmpty();
            parsedMessage.Should().BeEquivalentTo(message);
        }
    }
}
