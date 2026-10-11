namespace ByteBard.AsyncAPI.Tests.Bindings.NATS
{
    using System.Collections.Generic;
    using ByteBard.AsyncAPI.Bindings;
    using ByteBard.AsyncAPI.Bindings.NATS;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers;
    using ByteBard.AsyncAPI.Readers.Interface;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using NUnit.Framework;
    using FluentAssertions;

    public class NatsBindings_Should
    {
        [Test]
        public void OperationBinding_PreservesQueueAndOmittedVersion()
        {
            var operation = new AsyncApiOperation();
            operation.Bindings.Add(new NATSOperationBinding { Queue = "workers" });

            var yaml = operation.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);
            yaml.Should().Contain("queue: workers");
            yaml.Should().NotContain("bindingVersion");

            var settings = new AsyncApiReaderSettings();
            settings.Bindings = new List<IBindingParser<IBinding>> { new NATSOperationBinding() };
            var parsed = new AsyncApiStringReader(settings).ReadFragment<AsyncApiOperation>(yaml, AsyncApiVersion.AsyncApi2_0, out _);
            parsed.Should().BeEquivalentTo(operation);
        }
    }
}
