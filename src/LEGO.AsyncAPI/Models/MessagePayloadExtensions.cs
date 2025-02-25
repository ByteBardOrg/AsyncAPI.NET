// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Models
{
    using LEGO.AsyncAPI.Models.Interfaces;

    public static class MultiFormatSchemaExtensions
    {
        public static bool TryGetAs<T>(this IAsyncApiSchema payload, out T result)
            where T : class, IAsyncApiSchema
        {
            result = payload as T;
            return result != null;
        }

        public static T As<T>(this IAsyncApiSchema payload)
            where T : class, IAsyncApiSchema
        {
            return payload as T;
        }

        public static bool Is<T>(this IAsyncApiSchema payload)
            where T : class, IAsyncApiSchema
        {
            return payload is T;
        }
    }
}
