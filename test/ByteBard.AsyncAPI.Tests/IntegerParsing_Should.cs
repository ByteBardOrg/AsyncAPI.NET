namespace ByteBard.AsyncAPI.Tests
{
    using System.Globalization;
    using FluentAssertions;
    using ByteBard.AsyncAPI.Bindings;
    using ByteBard.AsyncAPI.Bindings.IBMMQ;
    using ByteBard.AsyncAPI.Bindings.ROS2;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers;
    using NUnit.Framework;

    [NonParallelizable]
    internal class IntegerParsing_Should : TestBase
    {
        private CultureInfo originalCulture;

        [SetUp]
        public void SetCulture()
        {
            this.originalCulture = CultureInfo.CurrentCulture;
        }

        [TearDown]
        public void RestoreCulture()
        {
            CultureInfo.CurrentCulture = this.originalCulture;
        }

        // These cultures use a negative sign other than a plain '-' (e.g. U+061C U+002D for ar-SA).
        [TestCase("ar-SA")]
        [TestCase("he-IL")]
        [TestCase("fa-IR")]
        public void NegativeIntegers_AreParsed_RegardlessOfCurrentCulture(string culture)
        {
            CultureInfo.CurrentCulture = new CultureInfo(culture);
            var yaml =
                """
                action: send
                channel:
                  $ref: '#/channels/camera'
                bindings:
                  ros2:
                    qosPolicies:
                      lifespan: -1
                      deadline: -1
                      leaseDuration: -1
                """;

            var settings = new AsyncApiReaderSettings { Bindings = BindingsCollection.ROS2 };
            var operation = new AsyncApiStringReader(settings).ReadFragment<AsyncApiOperation>(yaml, AsyncApiVersion.AsyncApi3_0, out var diagnostics);

            diagnostics.Errors.Should().BeEmpty();
            var qos = ((ROS2OperationBinding)operation.Bindings["ros2"]).QosPolicies;
            qos.Lifespan.Should().Be(-1);
            qos.Deadline.Should().Be(-1);
            qos.LeaseDuration.Should().Be(-1);
        }

        [TestCase("ar-SA")]
        [TestCase("he-IL")]
        public void NegativeLongs_AreParsed_RegardlessOfCurrentCulture(string culture)
        {
            CultureInfo.CurrentCulture = new CultureInfo(culture);
            var json = """{ "bindings": { "ibmmq": { "expiry": -5 } } }""";

            var settings = new AsyncApiReaderSettings { Bindings = BindingsCollection.IBMMQ };
            var message = new AsyncApiStringReader(settings).ReadFragment<AsyncApiMessage>(json, AsyncApiVersion.AsyncApi3_0, out var diagnostics);

            diagnostics.Errors.Should().BeEmpty();
            ((IBMMQMessageBinding)message.Bindings["ibmmq"]).Expiry.Should().Be(-5);
        }

        [TestCase("precision")]
        [TestCase("scale")]
        public void AvroDecimal_WithNonIntegerSize_ReportsErrorInsteadOfThrowing(string field)
        {
            var input = $$"""
                {
                  "type": "bytes",
                  "logicalType": "decimal",
                  "{{field}}": "abc"
                }
                """;

            AsyncApiDiagnostic diagnostics = null;
            var act = () => new AsyncApiStringReader().ReadFragment<AsyncApiAvroSchema>(input, AsyncApiVersion.AsyncApi2_0, out diagnostics);

            act.Should().NotThrow();
            diagnostics.Errors.Should().ContainSingle().Which.Message.Should().Contain("could not parse to integer");
        }
    }
}
