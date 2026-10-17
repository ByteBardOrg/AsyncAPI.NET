namespace ByteBard.AsyncAPI.Tests.Bindings
{
    using FluentAssertions;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers;
    using NUnit.Framework;
    using BindingsCollection = ByteBard.AsyncAPI.Bindings.BindingsCollection;

    internal class AwsPolicyParsing_Should : TestBase
    {
        [TestCase("action:\n  foo: bar", "StringOrStringList")]
        [TestCase("principal: someone", "Principal")]
        [TestCase("principal:\n  Unknown: arn", "Principal")]
        [TestCase("principal: {}", "Principal")]
        [TestCase("principal:\n  - '*'", "Principal")]
        [TestCase("condition: invalid", "Condition")]
        [TestCase("condition:\n  StringEquals: invalid", "Condition")]
        public void SqsChannelBinding_WithInvalidPolicyStatement_ReportsDiagnosticInsteadOfThrowing(string statementField, string expectedType)
        {
            var yaml =
                "bindings:\n" +
                "  sqs:\n" +
                "    queue:\n" +
                "      name: myQueue\n" +
                "      policy:\n" +
                "        statements:\n" +
                "          - effect: Allow\n" +
                Indent(statementField, 12);

            var settings = new AsyncApiReaderSettings { Bindings = BindingsCollection.Sqs };

            AsyncApiDiagnostic diagnostic = null;
            var act = () => new AsyncApiStringReader(settings).ReadFragment<AsyncApiChannel>(yaml, AsyncApiVersion.AsyncApi2_0, out diagnostic);

            act.Should().NotThrow();
            diagnostic.Errors.Should().ContainSingle().Which.Message.Should().Contain(expectedType);
        }

        [TestCase("action:\n  foo: bar", "StringOrStringList")]
        [TestCase("principal: someone", "Principal")]
        [TestCase("condition: invalid", "Condition")]
        public void SnsChannelBinding_WithInvalidPolicyStatement_ReportsDiagnosticInsteadOfThrowing(string statementField, string expectedType)
        {
            var yaml =
                "bindings:\n" +
                "  sns:\n" +
                "    name: myTopic\n" +
                "    policy:\n" +
                "      statements:\n" +
                "        - effect: Allow\n" +
                Indent(statementField, 10);

            var settings = new AsyncApiReaderSettings { Bindings = BindingsCollection.Sns };

            AsyncApiDiagnostic diagnostic = null;
            var act = () => new AsyncApiStringReader(settings).ReadFragment<AsyncApiChannel>(yaml, AsyncApiVersion.AsyncApi2_0, out diagnostic);

            act.Should().NotThrow();
            diagnostic.Errors.Should().ContainSingle().Which.Message.Should().Contain(expectedType);
        }

        private static string Indent(string value, int spaces)
        {
            var prefix = new string(' ', spaces);
            return prefix + value.Replace("\n", "\n" + prefix) + "\n";
        }
    }
}
