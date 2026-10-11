namespace ByteBard.AsyncAPI.Bindings
{
    using System;
    using System.Collections.Generic;
    using ByteBard.AsyncAPI.Bindings.AMQP;
    using ByteBard.AsyncAPI.Bindings.AnypointMQ;
    using ByteBard.AsyncAPI.Bindings.GooglePubSub;
    using ByteBard.AsyncAPI.Bindings.Http;
    using ByteBard.AsyncAPI.Bindings.IBMMQ;
    using ByteBard.AsyncAPI.Bindings.JMS;
    using ByteBard.AsyncAPI.Bindings.Kafka;
    using ByteBard.AsyncAPI.Bindings.MQTT;
    using ByteBard.AsyncAPI.Bindings.MQTT5;
    using ByteBard.AsyncAPI.Bindings.NATS;
    using ByteBard.AsyncAPI.Bindings.Pulsar;
    using ByteBard.AsyncAPI.Bindings.ROS2;
    using ByteBard.AsyncAPI.Bindings.Sns;
    using ByteBard.AsyncAPI.Bindings.Solace;
    using ByteBard.AsyncAPI.Bindings.Sqs;
    using ByteBard.AsyncAPI.Bindings.WebSockets;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers.Interface;

    public static class BindingsCollection
    {
        /// <summary>
        /// Adds the source items to the destination collection.
        /// </summary>
        /// <returns>The same destination collection, after adding the items.</returns>
        /// <remarks>Enumeration and collection errors propagate; items already added are not rolled back.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="destination"/> or <paramref name="source"/> is null.</exception>
        /// <exception cref="NotSupportedException">The destination collection does not support adding items.</exception>
        public static TCollection Add<TCollection, TItem>(this TCollection destination, IEnumerable<TItem> source)
            where TCollection : ICollection<TItem>
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (destination is List<TItem> list)
            {
                list.AddRange(source);
                return destination;
            }

            foreach (var item in source) destination.Add(item);
            return destination;
        }

        public static ICollection<IBindingParser<IBinding>> All => new List<IBindingParser<IBinding>>
        {
            Pulsar, Kafka, Http, Websockets, Sqs, Sns, AMQP, MQTT, AnypointMQ, GooglePubSub,
            IBMMQ, JMS, MQTT5, NATS, ROS2, Solace,
        };

        public static ICollection<IBindingParser<IBinding>> Http => new List<IBindingParser<IBinding>>
        {
            new HttpOperationBinding(), new HttpMessageBinding(),
        };

        public static ICollection<IBindingParser<IBinding>> Websockets => new List<IBindingParser<IBinding>>
        {
            new WebSocketsChannelBinding(),
        };

        public static ICollection<IBindingParser<IBinding>> Kafka => new List<IBindingParser<IBinding>>
        {
            new KafkaServerBinding(), new KafkaChannelBinding(), new KafkaOperationBinding(), new KafkaMessageBinding(),
        };

        public static ICollection<IBindingParser<IBinding>> Pulsar => new List<IBindingParser<IBinding>>
        {
            new PulsarServerBinding(), new PulsarChannelBinding(),
        };

        public static ICollection<IBindingParser<IBinding>> Sqs => new List<IBindingParser<IBinding>>
        {
            new SqsChannelBinding(), new SqsOperationBinding(),
        };

        public static ICollection<IBindingParser<IBinding>> Sns => new List<IBindingParser<IBinding>>
        {
            new SnsChannelBinding(), new SnsOperationBinding(),
        };

        public static ICollection<IBindingParser<IBinding>> AMQP => new List<IBindingParser<IBinding>>
        {
            new AMQPChannelBinding(), new AMQPOperationBinding(), new AMQPMessageBinding(),
        };

        public static ICollection<IBindingParser<IBinding>> AnypointMQ => new List<IBindingParser<IBinding>>
        {
            new AnypointMQChannelBinding(), new AnypointMQMessageBinding(),
        };

        public static ICollection<IBindingParser<IBinding>> GooglePubSub => new List<IBindingParser<IBinding>>
        {
            new GooglePubSubChannelBinding(), new GooglePubSubMessageBinding(),
        };

        public static ICollection<IBindingParser<IBinding>> IBMMQ => new List<IBindingParser<IBinding>>
        {
            new IBMMQServerBinding(), new IBMMQChannelBinding(), new IBMMQMessageBinding(),
        };

        public static ICollection<IBindingParser<IBinding>> JMS => new List<IBindingParser<IBinding>>
        {
            new JMSServerBinding(), new JMSChannelBinding(), new JMSMessageBinding(),
        };

        public static ICollection<IBindingParser<IBinding>> MQTT => new List<IBindingParser<IBinding>>
        {
            new MQTTServerBinding(), new MQTTOperationBinding(), new MQTTMessageBinding(),
        };

        public static ICollection<IBindingParser<IBinding>> MQTT5 => new List<IBindingParser<IBinding>>
        {
            new MQTT5ServerBinding(),
        };

        public static ICollection<IBindingParser<IBinding>> NATS => new List<IBindingParser<IBinding>>
        {
            new NATSOperationBinding(),
        };

        public static ICollection<IBindingParser<IBinding>> ROS2 => new List<IBindingParser<IBinding>>
        {
            new ROS2ServerBinding(), new ROS2OperationBinding(),
        };

        public static ICollection<IBindingParser<IBinding>> Solace => new List<IBindingParser<IBinding>>
        {
            new SolaceServerBinding(), new SolaceOperationBinding(),
        };
    }
}
