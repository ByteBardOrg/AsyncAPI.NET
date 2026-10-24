namespace ByteBard.AsyncAPI.Tests
{
    using System.Linq;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers;
    using FluentAssertions;
    using NUnit.Framework;

    /// <summary>
    /// Keys containing "/" or "~" must be escaped as "~1" and "~0" in reference pointers (RFC 6901).
    /// Such keys fail the spec's key patterns, but the documents still read, write, upgrade and downgrade.
    /// </summary>
    public class EscapedReferenceKeys_Should
    {
        private const string V3Document =
            """
            asyncapi: 3.0.0
            info:
              title: test
              version: 1.0.0
            servers:
              prod~eu:
                host: example.com
                protocol: kafka
                security:
                  - $ref: '#/components/securitySchemes/basic~1auth'
            channels:
              user/signedup:
                address: user/signedup
                servers:
                  - $ref: '#/servers/prod~0eu'
                messages:
                  signed/up:
                    $ref: '#/components/messages/signed~1up'
            operations:
              op:
                action: send
                channel:
                  $ref: '#/channels/user~1signedup'
                messages:
                  - $ref: '#/channels/user~1signedup/messages/signed~1up'
            components:
              messages:
                signed/up:
                  payload:
                    type: string
              securitySchemes:
                basic/auth:
                  type: userPassword
            """;

        private const string V2Document =
            """
            asyncapi: 2.6.0
            info:
              title: test
              version: 1.0.0
            servers:
              prod~eu:
                url: example.com
                protocol: kafka
                security:
                  - basic/auth: []
            channels:
              user/signedup:
                servers:
                  - prod~eu
                publish:
                  message:
                    $ref: '#/components/messages/signed~1up'
            components:
              messages:
                signed/up:
                  payload:
                    type: string
              securitySchemes:
                basic/auth:
                  type: userPassword
            """;

        [Test]
        public void V3_Read_ResolvesEscapedReferencesToTheRootComponents()
        {
            var doc = Read(V3Document);

            var channel = doc.Channels["user/signedup"];
            doc.Operations["op"].Channel.Equals(channel).Should().BeTrue();
            channel.Servers.Single().Equals(doc.Servers["prod~eu"]).Should().BeTrue();
            channel.Messages["signed/up"].Equals(doc.Components.Messages["signed/up"]).Should().BeTrue();
            doc.Operations["op"].Messages.Single().Equals(channel.Messages["signed/up"]).Should().BeTrue();
            doc.Servers["prod~eu"].Security.Single().Type.Should().Be(SecuritySchemeType.UserPassword);
        }

        [Test]
        public void V3_RoundTrips()
        {
            var expected =
                """
                asyncapi: 3.1.0
                info:
                  title: test
                  version: 1.0.0
                servers:
                  prod~eu:
                    host: example.com
                    protocol: kafka
                    security:
                      - $ref: '#/components/securitySchemes/basic~1auth'
                channels:
                  user/signedup:
                    address: user/signedup
                    messages:
                      signed/up:
                        $ref: '#/components/messages/signed~1up'
                    servers:
                      - $ref: '#/servers/prod~0eu'
                operations:
                  op:
                    action: send
                    channel:
                      $ref: '#/channels/user~1signedup'
                    messages:
                      - $ref: '#/channels/user~1signedup/messages/signed~1up'
                components:
                  messages:
                    signed/up:
                      payload:
                        schemaFormat: application/vnd.aai.asyncapi+json;version=3.0.0
                        schema:
                          type: string
                  securitySchemes:
                    basic/auth:
                      type: userPassword
                """;

            var actual = Read(V3Document).SerializeAsYaml(AsyncApiVersion.AsyncApi3_0);

            actual.Should().BePlatformAgnosticEquivalentTo(expected);
            Read(actual).SerializeAsYaml(AsyncApiVersion.AsyncApi3_0).Should().BePlatformAgnosticEquivalentTo(expected);
        }

        [Test]
        public void V3_DowngradesToV2()
        {
            var expected =
                """
                asyncapi: 2.6.0
                info:
                  title: test
                  version: 1.0.0
                servers:
                  prod~eu:
                    url: example.com
                    protocol: kafka
                    security:
                      - basic/auth: []
                channels:
                  user/signedup:
                    servers:
                      - prod~eu
                    subscribe:
                      message:
                        $ref: '#/components/messages/signed~1up'
                components:
                  messages:
                    signed/up:
                      payload:
                        type: string
                      schemaFormat: application/vnd.aai.asyncapi+json;version=3.0.0
                  securitySchemes:
                    basic/auth:
                      type: userPassword
                """;

            var actual = Read(V3Document).SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);

            actual.Should().BePlatformAgnosticEquivalentTo(expected);
            Read(actual).SerializeAsYaml(AsyncApiVersion.AsyncApi2_0).Should().BePlatformAgnosticEquivalentTo(expected);
        }

        [Test]
        public void V2_Read_ResolvesEscapedReferencesToTheRootComponents()
        {
            var doc = Read(V2Document);

            // 2.x channel keys are addresses, which the reader normalizes into 3.x keys.
            var channel = doc.Channels["usersignedup"];
            doc.Operations.Values.Single().Channel.Equals(channel).Should().BeTrue();
            channel.Servers.Single().Equals(doc.Servers["prod~eu"]).Should().BeTrue();
            channel.Messages.Keys.Should().Equal("signed/up");
            channel.Messages["signed/up"].Equals(doc.Components.Messages["signed/up"]).Should().BeTrue();
            doc.Servers["prod~eu"].Security.Single().Type.Should().Be(SecuritySchemeType.UserPassword);
        }

        [Test]
        public void V2_RoundTrips()
        {
            var actual = Read(V2Document).SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);

            actual.Should().BePlatformAgnosticEquivalentTo(V2Document);
            Read(actual).SerializeAsYaml(AsyncApiVersion.AsyncApi2_0).Should().BePlatformAgnosticEquivalentTo(V2Document);
        }

        [Test]
        public void V2_UpgradesToV3()
        {
            var expected =
                """
                asyncapi: 3.1.0
                info:
                  title: test
                  version: 1.0.0
                servers:
                  prod~eu:
                    host: example.com
                    protocol: kafka
                    security:
                      - $ref: '#/components/securitySchemes/basic~1auth'
                channels:
                  usersignedup:
                    address: user/signedup
                    messages:
                      signed/up:
                        $ref: '#/components/messages/signed~1up'
                    servers:
                      - $ref: '#/servers/prod~0eu'
                operations:
                  op:
                    action: receive
                    channel:
                      $ref: '#/channels/usersignedup'
                    messages:
                      - $ref: '#/channels/usersignedup/messages/signed~1up'
                components:
                  messages:
                    signed/up:
                      payload:
                        type: string
                  securitySchemes:
                    basic/auth:
                      type: userPassword
                """;

            var actual = Read(V2Document.Replace("publish:", "publish:\n      operationId: op")).SerializeAsYaml(AsyncApiVersion.AsyncApi3_0);

            actual.Should().BePlatformAgnosticEquivalentTo(expected);

            var upgraded = Read(actual);
            upgraded.Operations["op"].Channel.Equals(upgraded.Channels["usersignedup"]).Should().BeTrue();
            upgraded.Operations["op"].Messages.Single().Equals(upgraded.Channels["usersignedup"].Messages["signed/up"]).Should().BeTrue();
        }

        [TestCase("user/signedup", "user~1signedup")]
        [TestCase("user~signedup", "user~0signedup")]
        public void V3_LocalChannelReference_WithEscapedKey_ResolvesTheRootChannel(string channelKey, string escapedKey)
        {
            var doc = Read(ChannelReferenceDocument(channelKey, escapedKey));

            var channel = doc.Operations["op"].Channel;
            channel.UnresolvedReference.Should().BeFalse();
            channel.Equals(doc.Channels[channelKey]).Should().BeTrue();
        }

        [Test]
        public void V3_LocalChannelReference_WithUnescapedKey_DoesNotResolve()
        {
            // "/" separates reference tokens, so this points at channels -> user -> signedup.
            var doc = new AsyncApiStringReader().Read(ChannelReferenceDocument("user/signedup", "user/signedup"), out var diagnostic);

            doc.Operations["op"].Channel.UnresolvedReference.Should().BeTrue();
            diagnostic.Errors.Should().Contain(e => e.Message == "Could not resolve reference fragment.");
        }

        private static AsyncApiDocument Read(string yaml)
        {
            var doc = new AsyncApiStringReader().Read(yaml, out var diagnostic);

            // The keys fail the spec's key patterns; anything else means a reference did not resolve.
            diagnostic.Errors.Should().OnlyContain(e => e.Message.Contains("MUST match the regular expression"));
            return doc;
        }

        private static string ChannelReferenceDocument(string channelKey, string referenceKey)
        {
            return
                $"""
                asyncapi: 3.0.0
                info:
                  title: test
                  version: 1.0.0
                channels:
                  '{channelKey}':
                    address: user/signedup
                operations:
                  op:
                    action: send
                    channel:
                      $ref: '#/channels/{referenceKey}'
                """;
        }
    }
}
