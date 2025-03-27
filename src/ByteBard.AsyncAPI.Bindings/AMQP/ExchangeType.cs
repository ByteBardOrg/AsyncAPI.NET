namespace ByteBard.AsyncAPI.Bindings.AMQP
{
    using ByteBard.AsyncAPI.Attributes;

    public enum ExchangeType
    {
        [Display("default")]
        Default = 0,

        [Display("topic")]
        Topic,

        [Display("direct")]
        Direct,

        [Display("fanout")]
        Fanout,

        [Display("headers")]
        Headers,
    }
}