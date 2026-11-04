namespace ByteBard.AsyncAPI.Tests.Bindings.Registration
{
    using System.Linq;
    using FluentAssertions;
    using ByteBard.AsyncAPI.Bindings;
    using ByteBard.AsyncAPI.Bindings.NATS;
    using ByteBard.AsyncAPI.Bindings.ROS2;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers;
    using ByteBard.AsyncAPI.Readers.Interface;
    using NUnit.Framework;

    internal class BindingsCollection_Should : TestBase
    {
        [Test]
        public void All_contains_each_protocol_and_scope_once_and_matches_families()
        {
            var families = new[]
            {
                BindingsCollection.Pulsar, BindingsCollection.Kafka, BindingsCollection.Http, BindingsCollection.Websockets,
                BindingsCollection.Sqs, BindingsCollection.Sns, BindingsCollection.AMQP, BindingsCollection.MQTT,
                BindingsCollection.AnypointMQ, BindingsCollection.GooglePubSub, BindingsCollection.IBMMQ, BindingsCollection.JMS,
                BindingsCollection.MQTT5, BindingsCollection.NATS, BindingsCollection.ROS2, BindingsCollection.Solace,
            };
            var all = BindingsCollection.All.ToArray();

            families.Should().HaveCount(16);
            all.Should().HaveCount(35);
            all.Should().HaveCount(families.Sum(family => family.Count));
            all.Select(parser => parser.GetType()).Should().OnlyHaveUniqueItems();
            all.GroupBy(parser =>
            {
                var instance = (IBinding)System.Activator.CreateInstance(parser.GetType())!;
                var scope = instance is IServerBinding ? "server" : instance is IChannelBinding ? "channel" :
                    instance is IOperationBinding ? "operation" : "message";
                return (instance.BindingKey, scope);
            }).Should().OnlyContain(group => group.Count() == 1);
            families.SelectMany(family => family).Select(parser => parser.GetType())
                .Should().BeEquivalentTo(all.Select(parser => parser.GetType()));
        }

        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void Reader_uses_all_to_parse_field_and_new_server_bindings(AsyncApiVersion version)
        {
            var reader = new AsyncApiStringReader(new AsyncApiReaderSettings { Bindings = BindingsCollection.All });
            var serverAddress = version == AsyncApiVersion.AsyncApi2_0
                ? "url: broker.example\n"
                : "host: broker.example\n";
            var fieldBinding = reader.ReadFragment<AsyncApiServer>(serverAddress + "protocol: ros2\n" +
                "bindings:\n  ros2:\n    rmwImplementation: rmw_fastrtps_cpp\n    domainId: 7\n", version, out var ros2Diagnostics);
            ros2Diagnostics.Errors.Should().BeEmpty();
            ros2Diagnostics.Warnings.Should().BeEmpty();

            var ros2 = fieldBinding.Bindings["ros2"].Should().BeOfType<ROS2ServerBinding>().Subject;
            ros2.RmwImplementation.Should().Be("rmw_fastrtps_cpp");
            ros2.DomainId.Should().Be(7);

            var mqtt5Binding = reader.ReadFragment<AsyncApiServer>(serverAddress + "protocol: mqtt5\nbindings:\n  mqtt5: {}\n", version, out var mqtt5Diagnostics);
            mqtt5Diagnostics.Errors.Should().BeEmpty();
            mqtt5Diagnostics.Warnings.Should().BeEmpty();
            mqtt5Binding.Bindings.Should().ContainSingle(binding => binding.Key == "mqtt5");

            var operationYaml = version == AsyncApiVersion.AsyncApi2_0
                ? """
                  asyncapi: 2.6.0
                  info:
                    title: Registration test
                    version: 1.0.0
                  channels:
                    orders:
                      publish:
                        operationId: consumeOrders
                        bindings:
                          nats:
                            queue: workers
                  """
                : """
                  asyncapi: 3.0.0
                  info:
                    title: Registration test
                    version: 1.0.0
                  channels:
                    orders:
                      address: orders
                  operations:
                    consumeOrders:
                      action: send
                      channel:
                        $ref: '#/channels/orders'
                      bindings:
                        nats:
                          queue: workers
                  """;
            var operationDocument = reader.Read(operationYaml, out var operationDiagnostics);
            operationDiagnostics.Errors.Should().BeEmpty();
            operationDiagnostics.Warnings.Should().BeEmpty();
            operationDocument.Channels.Should().ContainKey("orders");
            operationDocument.Operations["consumeOrders"].Bindings["nats"].Should().BeOfType<NATSOperationBinding>()
                .Which.Queue.Should().Be("workers");
        }
    }
}
