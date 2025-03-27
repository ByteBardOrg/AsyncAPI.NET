namespace ByteBard.AsyncAPI.Models.Bindings.Pulsar
{
    using ByteBard.AsyncAPI.Attributes;

    public enum Persistence
    {
        [Display("persistent")]
        Persistent,

        [Display("non-persistent")]
        NonPersistent,
    }
}
