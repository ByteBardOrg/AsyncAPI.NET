namespace ByteBard.AsyncAPI.Tests.Bindings.ROS2
{
    using FluentAssertions;
    using ByteBard.AsyncAPI.Bindings.ROS2;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers;
    using NUnit.Framework;

    internal class ROS2Bindings_Should : TestBase
    {
        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void OperationBinding_WritesNestedQosAndRetainsZeroAndMinusOne(AsyncApiVersion version)
        {
            var binding = new ROS2OperationBinding
            {
                Role = "publisher",
                Node = "camera",
                QosPolicies = new ROS2QosPolicies
                {
                    Reliability = "reliable",
                    History = "keep_last",
                    Durability = "transient_local",
                    Lifespan = 0,
                    Deadline = -1,
                    Liveliness = "automatic",
                    LeaseDuration = 0,
                },
            };

            var operation = new AsyncApiOperation();
            operation.Channel = new AsyncApiChannelReference("#/channels/camera");
            operation.Messages.Add(new AsyncApiMessageReference("#/components/messages/camera"));
            operation.Bindings.Add(binding);
            var actual = operation.SerializeAsYaml(version);
            actual.Should().Contain("role: publisher")
                .And.Contain("qosPolicies:")
                .And.Contain("history: keep_last")
                .And.Contain("durability: transient_local")
                .And.Contain("lifespan: 0")
                .And.Contain("deadline: -1")
                .And.Contain("leaseDuration: 0")
                .And.NotContain("bindingVersion:");

            var settings = new AsyncApiReaderSettings { Bindings = new[] { new ROS2OperationBinding() } };
            var parsed = new AsyncApiStringReader(settings).ReadFragment<AsyncApiOperation>(actual, version, out var diagnostics);
            if (version == AsyncApiVersion.AsyncApi2_0)
            {
                diagnostics.Errors.Should().ContainSingle().Which.ToString().Should().Contain("field 'channel'");
            }
            else
            {
                diagnostics.Errors.Should().BeEmpty();
            }
            diagnostics.Warnings.Should().BeEmpty();
            parsed.Bindings["ros2"].Should().BeEquivalentTo(binding);
        }

        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void ServerBinding_OmitsUnsetFieldsAndWritesExplicitZero(AsyncApiVersion version)
        {
            var binding = new ROS2ServerBinding { DomainId = 0 };
            var server = new AsyncApiServer { Host = "localhost", Protocol = "ros2" };
            server.Bindings.Add(binding);
            var actual = server.SerializeAsYaml(version);
            actual.Should().Contain("domainId: 0")
                .And.NotContain("rmwImplementation:")
                .And.NotContain("bindingVersion:");

            var settings = new AsyncApiReaderSettings { Bindings = new[] { new ROS2ServerBinding() } };
            var parsed = new AsyncApiStringReader(settings).ReadFragment<AsyncApiServer>(actual, version, out var diagnostics);
            diagnostics.Errors.Should().BeEmpty();
            diagnostics.Warnings.Should().BeEmpty();
            parsed.Bindings["ros2"].Should().BeEquivalentTo(binding);
        }
    }
}
