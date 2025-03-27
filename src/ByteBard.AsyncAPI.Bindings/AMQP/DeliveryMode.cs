namespace ByteBard.AsyncAPI.Bindings.AMQP
{
    using ByteBard.AsyncAPI.Attributes;

    public enum DeliveryMode
    {
        [Display("transient")]
        Transient = 1,

        [Display("persistent")]
        Persistent = 2,
    }
}