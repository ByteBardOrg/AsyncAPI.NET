namespace ByteBard.AsyncAPI.Tests.Bindings.Http
{
    using System.Linq;
    using FluentAssertions;
    using ByteBard.AsyncAPI.Bindings;
    using ByteBard.AsyncAPI.Bindings.Http;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers;
    using NUnit.Framework;

    internal class HttpBindings_Should : TestBase
    {
        [Test]
        public void V2_HttpMessageBinding_FilledObject_SerializesAndDeserializes()
        {
            // Arrange
            var expected =
                """
                bindings:
                  http:
                    headers:
                      description: this mah binding
                    bindingVersion: 0.2.0
                """;

            var message = new AsyncApiMessage();

            message.Bindings.Add(new HttpMessageBinding
            {
                Headers = new AsyncApiJsonSchema
                {
                    Description = "this mah binding",
                },
                BindingVersion = "0.2.0",
            });

            // Act
            var actual = message.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);
            var settings = new AsyncApiReaderSettings();
            settings.Bindings = BindingsCollection.Http;
            var binding = new AsyncApiStringReader(settings).ReadFragment<AsyncApiMessage>(actual, AsyncApiVersion.AsyncApi2_0, out _);

            // Assert
            actual.Should()
                  .BePlatformAgnosticEquivalentTo(expected);
            binding.Should().BeEquivalentTo(message);
        }

        [Test]
        public void V2_HttpOperationBinding_RoundTrip_PreservesTypeFromOperationAction()
        {
            // Arrange
            var input =
                """
                asyncapi: 2.6.0
                info:
                  title: Test
                  version: 1.0.0
                channels:
                  test:
                    subscribe:
                      bindings:
                        http:
                          type: request
                          method: POST
                          query:
                            description: query params
                          bindingVersion: 0.2.0
                """;

            // Act
            var settings = new AsyncApiReaderSettings();
            settings.Bindings = BindingsCollection.Http;
            var document = new AsyncApiStringReader(settings).Read(input, out _);
            var output = document.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);

            // Assert
            var httpBinding = document.Operations.Values.First().Bindings["http"] as HttpOperationBinding;
            httpBinding.Method.Should().Be("POST");
            httpBinding.Query.Description.Should().Be("query params");

            output.Should().Contain("type: request");
            output.Should().Contain("method: POST");
            output.Should().Contain("bindingVersion: 0.2.0");
        }

        [Test]
        public void V2_HttpOperationBinding_RoundTrip_InfersResponseTypeFromPublishAction()
        {
            // Arrange
            var input =
                """
                asyncapi: 2.6.0
                info:
                  title: Test
                  version: 1.0.0
                channels:
                  test:
                    publish:
                      bindings:
                        http:
                          type: response
                """;

            // Act
            var settings = new AsyncApiReaderSettings();
            settings.Bindings = BindingsCollection.Http;
            var document = new AsyncApiStringReader(settings).Read(input, out _);
            var output = document.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);

            // Assert
            var operation = document.Operations.Values.First();
            operation.Action.Should().Be(AsyncApiAction.Receive);

            output.Should().Contain("type: response");
            output.Should().Contain("bindingVersion: 0.2.0");
        }

        [Test]
        public void V3_HttpOperationBinding_OmitsTypeField()
        {
            // Arrange
            var operation = new AsyncApiOperation
            {
                Action = AsyncApiAction.Send,
            };

            operation.Bindings.Add(new HttpOperationBinding
            {
                Method = "POST",
                Query = new AsyncApiJsonSchema
                {
                    Description = "query params",
                },
            });

            // Act
            var actual = operation.SerializeAsYaml(AsyncApiVersion.AsyncApi3_0);

            // Assert
            actual.Should().NotContain("type:");
            actual.Should().Contain("method: POST");
            actual.Should().Contain("bindingVersion: 0.3.0");
        }

        [Test]
        public void V3_HttpMessageBinding_IncludesStatusCode()
        {
            // Arrange
            var message = new AsyncApiMessage();

            message.Bindings.Add(new HttpMessageBinding
            {
                Headers = new AsyncApiJsonSchema
                {
                    Description = "response headers",
                },
                StatusCode = 200,
            });

            // Act
            var actual = message.SerializeAsYaml(AsyncApiVersion.AsyncApi3_0);

            // Assert
            actual.Should().Contain("statusCode: 200");
            actual.Should().Contain("bindingVersion: 0.3.0");
        }

        [Test]
        public void V2_HttpMessageBinding_OmitsStatusCode()
        {
            // Arrange
            var message = new AsyncApiMessage();

            message.Bindings.Add(new HttpMessageBinding
            {
                Headers = new AsyncApiJsonSchema
                {
                    Description = "response headers",
                },
                StatusCode = 200,
            });

            // Act
            var actual = message.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);

            // Assert
            actual.Should().NotContain("statusCode");
            actual.Should().Contain("bindingVersion: 0.2.0");
        }

        [Test]
        public void V3_HttpMessageBinding_ReadsStatusCode()
        {
            var yaml =
                """
                bindings:
                  http:
                    statusCode: 404
                """;

            var settings = new AsyncApiReaderSettings { Bindings = BindingsCollection.Http };
            var message = new AsyncApiStringReader(settings).ReadFragment<AsyncApiMessage>(yaml, AsyncApiVersion.AsyncApi3_0, out var diagnostics);

            diagnostics.Errors.Should().BeEmpty();
            ((HttpMessageBinding)message.Bindings["http"]).StatusCode.Should().Be(404);
        }

        [TestCase("not-found")]
        [TestCase("200.5")]
        public void V3_HttpMessageBinding_WithInvalidStatusCode_ReportsError(string statusCode)
        {
            var yaml =
                "bindings:\n" +
                "  http:\n" +
                "    statusCode: " + statusCode + "\n";

            var settings = new AsyncApiReaderSettings { Bindings = BindingsCollection.Http };
            new AsyncApiStringReader(settings).ReadFragment<AsyncApiMessage>(yaml, AsyncApiVersion.AsyncApi3_0, out var diagnostics);

            diagnostics.Errors.Should().ContainSingle().Which.Message.Should().Contain("could not parse to integer");
        }

        [TestCase(100)]
        [TestCase(599)]
        [TestCase(425)] // Too Early (RFC 8470), not a System.Net.HttpStatusCode member
        [TestCase(499)] // nginx client closed request
        [TestCase(520)] // Cloudflare unknown error
        public void V3_HttpMessageBinding_WithValidStatusCode_ReportsNoErrorAndRoundTrips(int statusCode)
        {
            var yaml =
                "bindings:\n" +
                "  http:\n" +
                "    statusCode: " + statusCode + "\n";

            var settings = new AsyncApiReaderSettings { Bindings = BindingsCollection.Http };
            var message = new AsyncApiStringReader(settings).ReadFragment<AsyncApiMessage>(yaml, AsyncApiVersion.AsyncApi3_0, out var diagnostics);

            diagnostics.Errors.Should().BeEmpty();
            ((HttpMessageBinding)message.Bindings["http"]).StatusCode.Should().Be(statusCode);
            message.SerializeAsYaml(AsyncApiVersion.AsyncApi3_0).Should().Contain("statusCode: " + statusCode);
        }

        [TestCase(0)]
        [TestCase(42)]
        [TestCase(99)]
        [TestCase(600)]
        [TestCase(-200)]
        public void V3_HttpMessageBinding_WithOutOfRangeStatusCode_ReportsErrorAndKeepsValue(int statusCode)
        {
            var yaml =
                "bindings:\n" +
                "  http:\n" +
                "    statusCode: " + statusCode + "\n";

            var settings = new AsyncApiReaderSettings { Bindings = BindingsCollection.Http };
            var message = new AsyncApiStringReader(settings).ReadFragment<AsyncApiMessage>(yaml, AsyncApiVersion.AsyncApi3_0, out var diagnostics);

            var error = diagnostics.Errors.Should().ContainSingle().Subject;
            error.Message.Should().Contain("must be between 100 and 599");
            error.Pointer.Should().EndWith("statusCode");
            ((HttpMessageBinding)message.Bindings["http"]).StatusCode.Should().Be(statusCode);
            message.SerializeAsYaml(AsyncApiVersion.AsyncApi3_0).Should().Contain("statusCode: " + statusCode);
        }
    }
}
