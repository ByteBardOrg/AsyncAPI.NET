﻿namespace LEGO.AsyncAPI.Models.Any
{
    /// <summary>
    /// Type of an <see cref="IOpenApiAny"/>.
    /// </summary>
    public enum AnyType
    {
        /// <summary>
        /// Primitive.
        /// </summary>
        Primitive,

        /// <summary>
        /// Null.
        /// </summary>
        Null,

        /// <summary>
        /// Array.
        /// </summary>
        Array,

        /// <summary>
        /// Object.
        /// </summary>
        Object
    }
}