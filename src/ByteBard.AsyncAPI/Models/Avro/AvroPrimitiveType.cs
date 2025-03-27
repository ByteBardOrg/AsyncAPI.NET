namespace ByteBard.AsyncAPI.Models
{
    using ByteBard.AsyncAPI.Attributes;

    public enum AvroPrimitiveType
    {
        [Display("null")]
        Null,

        [Display("boolean")]
        Boolean,

        [Display("int")]
        Int,

        [Display("long")]
        Long,

        [Display("float")]
        Float,

        [Display("double")]
        Double,

        [Display("bytes")]
        Bytes,

        [Display("string")]
        String,
    }
}