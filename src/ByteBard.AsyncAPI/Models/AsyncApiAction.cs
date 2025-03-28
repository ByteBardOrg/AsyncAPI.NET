namespace ByteBard.AsyncAPI.Models
{
    using ByteBard.AsyncAPI.Attributes;

    public enum AsyncApiAction
    {
        [Display("send")]
        Send,

        [Display("receive")]
        Receive,
    }
}