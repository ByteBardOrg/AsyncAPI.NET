namespace ByteBard.AsyncAPI.Tests
{
    using System.Linq;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers;
    using FluentAssertions;
    using NUnit.Framework;

    /// <summary>
    /// Reference pointers are URI fragments, so keys are percent-encoded in them (RFC 6901 section 6):
    /// "prod eu" is referenced as "prod%20eu", and a key that is literally "prod%20eu" as "prod%2520eu".
    /// Such keys fail the spec's key patterns, but the documents still read, write, upgrade and downgrade.
    /// </summary>
    public class PercentEncodedReferenceKeys_Should
    {
        private const string V3Document =
            """
            asyncapi: 3.0.0
            info:
              title: test
              version: 1.0.0
            servers:
              prod eu:
                host: example.com
                protocol: kafka
                security:
                  - $ref: '#/components/securitySchemes/basic%20auth'
              prod%20eu:
                host: example.org
                protocol: kafka
            channels:
              user signedup:
                address: user/signedup
                servers:
                  - $ref: '#/servers/prod%20eu'
                  - $ref: '#/servers/prod%2520eu'
                messages:
                  signed up:
                    $ref: '#/components/messages/signed%20up'
            operations:
              op:
                action: send
                channel:
                  $ref: '#/channels/user%20signedup'
                messages:
                  - $ref: '#/channels/user%20signedup/messages/signed%20up'
            components:
              messages:
                signed up:
                  payload:
                    type: string
              securitySchemes:
                basic auth:
                  type: userPassword
            """;

        private const string V2Document =
            """
            asyncapi: 2.6.0
            info:
              title: test
              version: 1.0.0
            servers:
              prod eu:
                url: example.com
                protocol: kafka
                security:
                  - basic auth: []
              prod%20eu:
                url: example.org
                protocol: kafka
            channels:
              user/signedup:
                servers:
                  - prod eu
                  - prod%20eu
                publish:
                  message:
                    $ref: '#/components/messages/signed%20up'
            components:
              messages:
                signed up:
                  payload:
                    type: string
              securitySchemes:
                basic auth:
                  type: userPassword
            """;

        [Test]
        public void V3_Read_ResolvesEncodedReferencesToTheRootComponents()
        {
            var doc = Read(V3Document);

            var channel = doc.Channels["user signedup"];
            doc.Operations["op"].Channel.Equals(channel).Should().BeTrue();
            channel.Servers[0].Equals(doc.Servers["prod eu"]).Should().BeTrue();
            channel.Servers[1].Equals(doc.Servers["prod%20eu"]).Should().BeTrue();
            channel.Messages["signed up"].Equals(doc.Components.Messages["signed up"]).Should().BeTrue();
            doc.Operations["op"].Messages.Single().Equals(channel.Messages["signed up"]).Should().BeTrue();
            doc.Servers["prod eu"].Security.Single().Type.Should().Be(SecuritySchemeType.UserPassword);
        }

        [TestCase("prod%20eu")]
        [TestCase("prod eu")]
        public void V3_LocalServerReference_ResolvesTheRootServer_WhetherOrNotItIsEncoded(string referenceKey)
        {
            var doc = Read(
                $"""
                asyncapi: 3.0.0
                info:
                  title: test
                  version: 1.0.0
                servers:
                  prod eu:
                    host: example.com
                    protocol: kafka
                channels:
                  test:
                    servers:
                      - $ref: '#/servers/{referenceKey}'
                """);

            // Changes made through the root server must show through the reference, so it must not resolve to a copy.
            doc.Servers["prod eu"].Host = "changed.example.com";
            doc.Channels["test"].Servers.Single().Host.Should().Be("changed.example.com");
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
                  prod eu:
                    host: example.com
                    protocol: kafka
                    security:
                      - $ref: '#/components/securitySchemes/basic%20auth'
                  prod%20eu:
                    host: example.org
                    protocol: kafka
                channels:
                  user signedup:
                    address: user/signedup
                    messages:
                      signed up:
                        $ref: '#/components/messages/signed%20up'
                    servers:
                      - $ref: '#/servers/prod%20eu'
                      - $ref: '#/servers/prod%2520eu'
                operations:
                  op:
                    action: send
                    channel:
                      $ref: '#/channels/user%20signedup'
                    messages:
                      - $ref: '#/channels/user%20signedup/messages/signed%20up'
                components:
                  messages:
                    signed up:
                      payload:
                        schemaFormat: application/vnd.aai.asyncapi+json;version=3.0.0
                        schema:
                          type: string
                  securitySchemes:
                    basic auth:
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
                  prod eu:
                    url: example.com
                    protocol: kafka
                    security:
                      - basic auth: []
                  prod%20eu:
                    url: example.org
                    protocol: kafka
                channels:
                  user/signedup:
                    servers:
                      - prod eu
                      - prod%20eu
                    subscribe:
                      message:
                        $ref: '#/components/messages/signed%20up'
                components:
                  messages:
                    signed up:
                      payload:
                        type: string
                      schemaFormat: application/vnd.aai.asyncapi+json;version=3.0.0
                  securitySchemes:
                    basic auth:
                      type: userPassword
                """;

            var actual = Read(V3Document).SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);

            actual.Should().BePlatformAgnosticEquivalentTo(expected);
            Read(actual).SerializeAsYaml(AsyncApiVersion.AsyncApi2_0).Should().BePlatformAgnosticEquivalentTo(expected);
        }

        [Test]
        public void V2_Read_ResolvesServerNamesToTheRootServers()
        {
            var doc = Read(V2Document);

            // 2.x channel keys are addresses, which the reader normalizes into 3.x keys.
            var channel = doc.Channels["usersignedup"];
            channel.Servers[0].Equals(doc.Servers["prod eu"]).Should().BeTrue();
            channel.Servers[1].Equals(doc.Servers["prod%20eu"]).Should().BeTrue();
            channel.Messages.Keys.Should().Equal("signed up");
            channel.Messages["signed up"].Equals(doc.Components.Messages["signed up"]).Should().BeTrue();
            doc.Servers["prod eu"].Security.Single().Type.Should().Be(SecuritySchemeType.UserPassword);
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
                  prod eu:
                    host: example.com
                    protocol: kafka
                    security:
                      - $ref: '#/components/securitySchemes/basic%20auth'
                  prod%20eu:
                    host: example.org
                    protocol: kafka
                channels:
                  usersignedup:
                    address: user/signedup
                    messages:
                      signed up:
                        $ref: '#/components/messages/signed%20up'
                    servers:
                      - $ref: '#/servers/prod%20eu'
                      - $ref: '#/servers/prod%2520eu'
                operations:
                  op:
                    action: receive
                    channel:
                      $ref: '#/channels/usersignedup'
                    messages:
                      - $ref: '#/channels/usersignedup/messages/signed%20up'
                components:
                  messages:
                    signed up:
                      payload:
                        type: string
                  securitySchemes:
                    basic auth:
                      type: userPassword
                """;

            var actual = Read(V2Document.Replace("publish:", "publish:\n      operationId: op")).SerializeAsYaml(AsyncApiVersion.AsyncApi3_0);

            actual.Should().BePlatformAgnosticEquivalentTo(expected);

            var upgraded = Read(actual);
            upgraded.Channels["usersignedup"].Servers[0].Equals(upgraded.Servers["prod eu"]).Should().BeTrue();
            upgraded.Channels["usersignedup"].Servers[1].Equals(upgraded.Servers["prod%20eu"]).Should().BeTrue();
            upgraded.Operations["op"].Messages.Single().Equals(upgraded.Channels["usersignedup"].Messages["signed up"]).Should().BeTrue();
        }

        private static AsyncApiDocument Read(string yaml)
        {
            var doc = new AsyncApiStringReader().Read(yaml, out var diagnostic);

            // The keys fail the spec's key patterns; anything else means a reference did not resolve.
            diagnostic.Errors.Should().OnlyContain(e => e.Message.Contains("MUST match the regular expression"));
            return doc;
        }
    }
}
