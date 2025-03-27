namespace ByteBard.AsyncAPI.Bindings.AMQP
{
    using ByteBard.AsyncAPI.Attributes;

    public enum ChannelType
    {
        [Display("routingKey")]
        RoutingKey = 0,

        [Display("queue")]
        Queue,
    }
}