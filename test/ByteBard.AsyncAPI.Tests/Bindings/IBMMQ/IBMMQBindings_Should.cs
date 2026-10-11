namespace ByteBard.AsyncAPI.Tests.Bindings.IBMMQ
{
    using System.Collections.Generic;
    using FluentAssertions;
    using ByteBard.AsyncAPI.Bindings;
    using ByteBard.AsyncAPI.Bindings.IBMMQ;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers;
    using ByteBard.AsyncAPI.Readers.Interface;
    using NUnit.Framework;

    internal class IBMMQBindings_Should : TestBase
    {
        private static ICollection<IBindingParser<IBinding>> Parsers => new List<IBindingParser<IBinding>>
        {
            new IBMMQServerBinding(), new IBMMQChannelBinding(), new IBMMQMessageBinding(),
        };

        [TestCase(AsyncApiVersion.AsyncApi2_0)]
        [TestCase(AsyncApiVersion.AsyncApi3_0)]
        public void IBMMQBindings_RoundTripAndOmitUnsetValues(AsyncApiVersion version)
        {
            var server = new AsyncApiServer { Host = "mq.example", Protocol = "ibmmq" };
            server.Bindings.Add(new IBMMQServerBinding
            {
                GroupId = "group-a", CcdtQueueManagerName = "QM1", CipherSpec = "TLS_AES_256_GCM_SHA384",
                MultiEndpointServer = false, HeartBeatInterval = 0,
            });
            var serverYaml = server.SerializeAsYaml(version);
            serverYaml.Should().Contain("multiEndpointServer: false").And.Contain("heartBeatInterval: 0");
            new AsyncApiStringReader(new AsyncApiReaderSettings { Bindings = Parsers })
                .ReadFragment<AsyncApiServer>(serverYaml, version, out _).Should().BeEquivalentTo(server);

            var channel = new AsyncApiChannel();
            channel.Bindings.Add(new IBMMQChannelBinding
            {
                DestinationType = "queue", MaxMsgLength = 0,
                Queue = new Queue { ObjectName = "ORDERS", IsPartitioned = false, Exclusive = true },
            });
            var channelYaml = channel.SerializeAsYaml(version);
            channelYaml.Should().Contain("isPartitioned: false").And.Contain("maxMsgLength: 0").And.NotContain("topic:");
            new AsyncApiStringReader(new AsyncApiReaderSettings { Bindings = Parsers })
                .ReadFragment<AsyncApiChannel>(channelYaml, version, out _).Should().BeEquivalentTo(channel);

            var topicChannel = new AsyncApiChannel();
            topicChannel.Bindings.Add(new IBMMQChannelBinding
            {
                DestinationType = "topic",
                Topic = new Topic { StringValue = "orders/#", ObjectName = "ORDERS.TOPIC", DurablePermitted = false, LastMsgRetained = true },
            });
            var topicYaml = topicChannel.SerializeAsYaml(version);
            topicYaml.Should().Contain("string: orders/#").And.NotContain("queue:");
            new AsyncApiStringReader(new AsyncApiReaderSettings { Bindings = Parsers })
                .ReadFragment<AsyncApiChannel>(topicYaml, version, out _).Should().BeEquivalentTo(topicChannel);

            var message = new AsyncApiMessage();
            message.Bindings.Add(new IBMMQMessageBinding { Type = "binary", Headers = "STRING", Description = "event headers", Expiry = 0 });
            var messageYaml = message.SerializeAsYaml(version);
            messageYaml.Should().Contain("headers: STRING").And.Contain("expiry: 0");
            new AsyncApiStringReader(new AsyncApiReaderSettings { Bindings = Parsers })
                .ReadFragment<AsyncApiMessage>(messageYaml, version, out _).Should().BeEquivalentTo(message);

            var absent = new AsyncApiServer();
            absent.Bindings.Add(new IBMMQServerBinding());
            var absentYaml = absent.SerializeAsYaml(version);
            absentYaml.Should().NotContain("multiEndpointServer").And.NotContain("heartBeatInterval").And.NotContain("bindingVersion");
        }
    }
}
