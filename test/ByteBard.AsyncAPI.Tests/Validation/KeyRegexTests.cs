namespace ByteBard.AsyncAPI.Tests.Validation;

using System.Collections.Generic;
using ByteBard.AsyncAPI.Readers;
using FluentAssertions;
using NUnit.Framework;

public class KeyRegexTests
{
    [TestCase("chat.v1")]
    [TestCase("chat_01")]
    [TestCase("chat-1")]
    public void ChannelKey_RetainsExistingPattern(string key)
    {
        var input = $$"""
            asyncapi: 3.0.0
            info:
              title: Key validation
              version: '1.0'
            channels:
              '{{key}}': {}
            """;

        var document = new AsyncApiStringReader().Read(input, out var diagnostic);

        diagnostic.Errors.Should().BeEmpty();
        document.Channels.Should().ContainKey(key);
    }

    public static IEnumerable<TestCaseData> ServerKeys()
    {
        foreach (var version in new[] { "2.6.0", "3.0.0" })
        {
            foreach (var key in new[] { "production", "Server_01", "server-1", "0" })
            {
                yield return new TestCaseData(version, key, true);
            }

            foreach (var key in new[] { "server.one", "sérver", "server/name", "server name", "" })
            {
                yield return new TestCaseData(version, key, false);
            }
        }
    }

    public static IEnumerable<TestCaseData> ComponentKeys()
    {
        foreach (var version in new[] { "2.6.0", "3.0.0" })
        {
            foreach (var key in new[] { "Message", "message.v1", "message_01", "message-1", "0" })
            {
                yield return new TestCaseData(version, key, true);
            }

            foreach (var key in new[] { "méssage", "消息", "message\u0301", "message/name", "message name", "" })
            {
                yield return new TestCaseData(version, key, false);
            }
        }
    }

    [TestCaseSource(nameof(ServerKeys))]
    public void ServerKey_UsesSpecificationPattern(string version, string key, bool valid)
    {
        var addressField = version == "2.6.0" ? "url" : "host";
        var input = $$"""
            asyncapi: {{version}}
            info:
              title: Key validation
              version: '1.0'
            channels:
              chat: {}
            servers:
              '{{key}}':
                {{addressField}}: localhost
                protocol: kafka
            """;

        var document = new AsyncApiStringReader().Read(input, out var diagnostic);

        document.Servers.Should().ContainKey(key);
        if (valid)
        {
            diagnostic.Errors.Should().BeEmpty();
        }
        else
        {
            diagnostic.Errors.Should().ContainSingle(e => e.Message.Contains("in 'servers' MUST match"));
        }
    }

    [TestCaseSource(nameof(ComponentKeys))]
    public void ComponentKey_UsesAsciiSpecificationPattern(string version, string key, bool valid)
    {
        var input = $$"""
            asyncapi: {{version}}
            info:
              title: Key validation
              version: '1.0'
            channels:
              chat: {}
            components:
              messages:
                '{{key}}':
                  name: example
            """;

        var document = new AsyncApiStringReader().Read(input, out var diagnostic);

        document.Components.Messages.Should().ContainKey(key);
        if (valid)
        {
            diagnostic.Errors.Should().BeEmpty();
        }
        else
        {
            diagnostic.Errors.Should().ContainSingle(e => e.Message.Contains("in 'messages' MUST match"));
        }
    }
}
