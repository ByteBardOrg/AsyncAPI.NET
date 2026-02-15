namespace ByteBard.AsyncAPI.Tests.Bindings.Http
{
    using System.Linq;
    using System.Net;
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
                StatusCode = HttpStatusCode.OK,
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
                StatusCode = HttpStatusCode.OK,
            });

            // Act
            var actual = message.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);

            // Assert
            actual.Should().NotContain("statusCode");
            actual.Should().Contain("bindingVersion: 0.2.0");
        }
    }
}
