namespace ByteBard.AsyncAPI.Tests.Models;

using ByteBard.AsyncAPI.Extensions;
using ByteBard.AsyncAPI.Models;
using ByteBard.AsyncAPI.Readers;
using FluentAssertions;
using NUnit.Framework;

public class ChannelAddressExpressionTests
{
    [TestCase("id")]
    [TestCase("id0")]
    [TestCase("user_id")]
    [TestCase("user-id")]
    public void NamedExpression_PreservesParametersWhenSerialized(string name)
    {
        var channel = new AsyncApiChannel { Address = "orders/{" + name + "}" };
        channel.Parameters.Add(name, new AsyncApiParameter { Description = "Order identifier" });

        channel.Address.IsChannelAddressExpression().Should().BeTrue();
        var yaml = channel.SerializeAsYaml(AsyncApiVersion.AsyncApi3_0);

        yaml.Should().Contain("parameters:");
        yaml.Should().Contain("description: Order identifier");

        var roundTripped = new AsyncApiStringReader().ReadFragment<AsyncApiChannel>(
            yaml, AsyncApiVersion.AsyncApi3_0, out var diagnostic);

        diagnostic.Errors.Should().BeEmpty();
        roundTripped.Parameters.Should().ContainKey(name);
        roundTripped.Address.Should().Be(channel.Address);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("orders")]
    [TestCase("orders/{}")]
    [TestCase("orders/{user.id}")]
    [TestCase("orders/{用户}")]
    [TestCase("orders/{user id}")]
    [TestCase("orders/{user/id}")]
    [TestCase("orders/{user%20id}")]
    [TestCase("orders/{id")]
    [TestCase("orders/id}")]
    public void AddressWithoutNamedExpression_IsNotRecognized(string? address)
    {
        address.IsChannelAddressExpression().Should().BeFalse();
    }
}
