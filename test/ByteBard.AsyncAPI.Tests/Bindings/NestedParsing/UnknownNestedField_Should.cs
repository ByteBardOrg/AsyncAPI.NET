namespace ByteBard.AsyncAPI.Tests.Bindings.NestedParsing
{
    using ByteBard.AsyncAPI.Bindings;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers;
    using FluentAssertions;
    using NUnit.Framework;

    internal class UnknownNestedField_Should : TestBase
    {
        [TestCase(UnmappedMemberHandling.Error, 1)]
        [TestCase(UnmappedMemberHandling.Ignore, 0)]
        public void FollowConfiguredUnmappedMemberHandling(UnmappedMemberHandling handling, int expectedErrorCount)
        {
            var yaml = """
                bindings:
                  amqp:
                    is: routingKey
                    exchange:
                      name: events
                      unknownNestedField: ignored-value
                """;
            var settings = new AsyncApiReaderSettings
            {
                Bindings = BindingsCollection.AMQP,
                UnmappedMemberHandling = handling,
            };

            new AsyncApiStringReader(settings).ReadFragment<AsyncApiChannel>(yaml, AsyncApiVersion.AsyncApi2_0, out var diagnostic);

            diagnostic.Errors.Should().HaveCount(expectedErrorCount);
            if (expectedErrorCount > 0)
            {
                diagnostic.Errors[0].Message.Should().Contain("unknownNestedField");
            }
        }
    }
}
