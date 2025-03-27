namespace ByteBard.AsyncAPI.Models
{
    using System;
    using System.Collections.ObjectModel;
    using System.Text.Json.Nodes;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Writers;

    [Obsolete("Please use AsyncApiAny instead")]
    public class AsyncApiArray : Collection<AsyncApiAny>, IAsyncApiExtension, IAsyncApiElement
    {
        public static explicit operator AsyncApiArray(AsyncApiAny any)
        {
            var a = new AsyncApiArray();
            if (any.GetNode() is JsonArray arr)
            {
                foreach (var item in arr)
                {
                    a.Add(new AsyncApiAny(item));
                }
            }

            return a;
        }

        public static implicit operator AsyncApiAny(AsyncApiArray arr)
        {
            var jArray = new JsonArray();
            foreach (var item in arr)
            {
                jArray.Add(item.GetNode());
            }

            return new AsyncApiAny(jArray);
        }

        /// <summary>
        /// Serialize AsyncApiObject to writer.
        /// </summary>
        public void Write(IAsyncApiWriter writer)
        {
            writer.WriteStartArray();

            foreach (var item in this)
            {
                writer.WriteAny(item);
            }

            writer.WriteEndArray();
        }
    }
}
