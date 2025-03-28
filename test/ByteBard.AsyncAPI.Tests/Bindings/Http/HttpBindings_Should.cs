namespace ByteBard.AsyncAPI.Tests.Bindings.Http
{
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
                """;

            var message = new AsyncApiMessage();

            message.Bindings.Add(new HttpMessageBinding
            {
                Headers = new AsyncApiJsonSchema
                {
                    Description = "this mah binding",
                },
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
        public void V2_HttpOperationBinding_FilledObject_SerializesAndDeserializes()
        {
            // Arrange
            var expected =
                """
                bindings:
                  http:
                    type: request
                    method: POST
                    query:
                      description: this mah query
                """;

            var operation = new AsyncApiOperation();

            operation.Bindings.Add(new HttpOperationBinding
            {
                Type = HttpOperationBinding.HttpOperationType.Request,
                Method = "POST",
                Query = new AsyncApiJsonSchema
                {
                    Description = "this mah query",
                },
            });

            // Act
            var actual = operation.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);
            var settings = new AsyncApiReaderSettings();
            settings.Bindings = BindingsCollection.Http;
            var binding = new AsyncApiStringReader(settings).ReadFragment<AsyncApiOperation>(actual, AsyncApiVersion.AsyncApi2_0, out _);

            // Assert
            actual.Should()
                  .BePlatformAgnosticEquivalentTo(expected);
            binding.Should().BeEquivalentTo(operation);
        }
    }
}
