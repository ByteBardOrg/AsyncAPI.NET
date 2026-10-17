namespace ByteBard.AsyncAPI.Tests.Bindings
{
    using FluentAssertions;
    using ByteBard.AsyncAPI.Bindings;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers;
    using NUnit.Framework;

    internal class BindingInvalidScalars_Should : TestBase
    {
        [TestCase("mqtt", "cleanSession: yes-please", "bool")]
        [TestCase("mqtt", "keepAlive: abc", "integer")]
        [TestCase("mqtt", "lastWill:\n  topic: /wills\n  qos: high", "integer")]
        [TestCase("ros2", "domainId: abc", "integer")]
        public void ServerBinding_WithInvalidScalar_ReportsError(string bindingKey, string field, string expectedType)
        {
            var yaml =
                "host: localhost\n" +
                "protocol: " + bindingKey + "\n" +
                "bindings:\n" +
                "  " + bindingKey + ":\n" +
                Indent(field, 4);

            new AsyncApiStringReader(Settings()).ReadFragment<AsyncApiServer>(yaml, AsyncApiVersion.AsyncApi3_0, out var diagnostics);

            diagnostics.Errors.Should().ContainSingle().Which.Message.Should().Contain($"could not parse to {expectedType}");
        }

        [Test]
        public void MessageBinding_WithInvalidPayloadFormatIndicator_ReportsError()
        {
            var yaml =
                "bindings:\n" +
                "  mqtt:\n" +
                "    payloadFormatIndicator: abc\n";

            new AsyncApiStringReader(Settings()).ReadFragment<AsyncApiMessage>(yaml, AsyncApiVersion.AsyncApi3_0, out var diagnostics);

            diagnostics.Errors.Should().ContainSingle().Which.Message.Should().Contain("could not parse to integer");
        }

        [TestCase("lifespan")]
        [TestCase("deadline")]
        [TestCase("leaseDuration")]
        public void ROS2OperationBinding_WithInvalidQosDuration_ReportsError(string field)
        {
            var yaml =
                "action: send\n" +
                "channel:\n" +
                "  $ref: '#/channels/camera'\n" +
                "bindings:\n" +
                "  ros2:\n" +
                "    qosPolicies:\n" +
                "      " + field + ": forever\n";

            new AsyncApiStringReader(Settings()).ReadFragment<AsyncApiOperation>(yaml, AsyncApiVersion.AsyncApi3_0, out var diagnostics);

            diagnostics.Errors.Should().ContainSingle().Which.Message.Should().Contain("could not parse to integer");
        }

        private static AsyncApiReaderSettings Settings() => new() { Bindings = BindingsCollection.All };

        private static string Indent(string value, int spaces)
        {
            var prefix = new string(' ', spaces);
            return prefix + value.Replace("\n", "\n" + prefix) + "\n";
        }
    }
}
