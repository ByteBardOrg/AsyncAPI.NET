namespace ByteBard.AsyncAPI.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers;
    using ByteBard.AsyncAPI.Writers;
    using FluentAssertions;
    using NUnit.Framework;

    public class AsyncApiReferenceEquality_Should : TestBase
    {
        private const string RecursiveSchemaDocument =
            """
            asyncapi: 3.0.0
            info:
              title: test
              version: 1.0.0
            channels:
              first:
                messages:
                  a:
                    payload:
                      $ref: '#/components/schemas/Node'
              second:
                messages:
                  b:
                    payload:
                      $ref: '#/components/schemas/Node'
            components:
              schemas:
                Node:
                  type: object
                  properties:
                    next:
                      $ref: '#/components/schemas/Node'
                    prev:
                      $ref: '#/components/schemas/Node'
            """;

        [Test]
        public void UnresolvedReferences_AreEqualOnlyWhenTheyPointAtTheSameComponent()
        {
            var first = new AsyncApiChannelReference("#/components/channels/first");
            var firstAgain = new AsyncApiChannelReference("#/components/channels/first");
            var second = new AsyncApiChannelReference("#/components/channels/second");

            first.Should().Be(firstAgain);
            (first == firstAgain).Should().BeTrue();
            first.GetHashCode().Should().Be(firstAgain.GetHashCode());

            first.Should().NotBe(second);
            (first == second).Should().BeFalse();
            first.Equals((AsyncApiChannel)null).Should().BeFalse();
        }

        [Test]
        public void ResolvedReferences_EqualTheirTargetAndShareItsHashCode()
        {
            var document = new AsyncApiStringReader().Read(RecursiveSchemaDocument, out var diagnostic);
            diagnostic.Errors.Should().BeEmpty();

            var target = (AsyncApiJsonSchema)document.Components.Schemas["Node"].Schema;
            var first = (AsyncApiJsonSchemaReference)target.Properties["next"];
            var second = (AsyncApiJsonSchemaReference)target.Properties["prev"];

            first.Should().NotBeSameAs(second);
            first.Should().Be(second);
            first.Equals(target).Should().BeTrue();
            first.GetHashCode().Should().Be(second.GetHashCode()).And.Be(target.GetHashCode());
            new[] { first, second }.Distinct().Should().ContainSingle();
        }

        [Test]
        public void SerializingWithInlinedReferences_DoesNotFailWhenTheSameLoopIsReachedTwice()
        {
            var yaml =
                """
                asyncapi: 2.6.0
                info:
                  title: test
                  version: 1.0.0
                channels:
                  first:
                    publish:
                      message:
                        payload:
                          $ref: '#/components/schemas/Node'
                components:
                  schemas:
                    Node:
                      type: object
                      properties:
                        next:
                          $ref: '#/components/schemas/Node'
                        prev:
                          $ref: '#/components/schemas/Node'
                """;
            var document = new AsyncApiStringReader().Read(yaml, out var diagnostic);
            diagnostic.Errors.Should().BeEmpty();

            var output = new StringWriter();
            var writer = new AsyncApiYamlWriter(output, new AsyncApiWriterSettings { ReferenceInline = ReferenceInlineSetting.InlineReferences });

            var act = () => document.SerializeV2(writer);

            act.Should().NotThrow();
        }

        [Test]
        public void SettingAPropertyOnAnUnresolvedReference_ThrowsWithTheReference()
        {
            var reference = new AsyncApiChannelReference("#/components/channels/missing");

            var act = () => reference.Address = "users";

            act.Should().Throw<InvalidOperationException>().WithMessage("*#/components/channels/missing*");
        }

        [Test]
        public void ReadingAFlagOnAnUnresolvedSchemaReference_ReturnsFalse()
        {
            var reference = new AsyncApiJsonSchemaReference("#/components/schemas/missing");

            reference.ReadOnly.Should().BeFalse();
            reference.Deprecated.Should().BeFalse();
        }
    }
}
