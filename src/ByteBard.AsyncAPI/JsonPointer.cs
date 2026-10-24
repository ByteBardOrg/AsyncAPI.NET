namespace ByteBard.AsyncAPI
{
    using System;
    using System.Linq;

    /// <summary>
    /// JSON pointer.
    /// </summary>
    public class JsonPointer
    {
        /// <summary>
        /// Initializes the <see cref="JsonPointer"/> class.
        /// </summary>
        /// <param name="pointer">Pointer as string.</param>
        public JsonPointer(string pointer)
        {
            this.Tokens = string.IsNullOrEmpty(pointer) || pointer == "/"
                ? new string[0]
                : pointer.Split('/').Skip(1).Select(this.Decode).ToArray();
        }

        /// <summary>
        /// Initializes the <see cref="JsonPointer"/> class.
        /// </summary>
        /// <param name="tokens">Pointer as tokenized string.</param>
        private JsonPointer(string[] tokens)
        {
            this.Tokens = tokens;
        }

        /// <summary>
        /// Tokens.
        /// </summary>
        public string[] Tokens { get; }

        /// <summary>
        /// Gets the parent pointer.
        /// </summary>
        public JsonPointer ParentPointer
        {
            get
            {
                if (this.Tokens.Length == 0)
                {
                    return null;
                }

                return new JsonPointer(this.Tokens.Take(this.Tokens.Length - 1).ToArray());
            }
        }

        /// <summary>
        /// Escapes a single reference token, so "~" becomes "~0" and "/" becomes "~1".
        /// </summary>
        /// <param name="token">The unescaped token.</param>
        public static string EscapeToken(string token)
        {
            return token.Replace("~", "~0").Replace("/", "~1");
        }

        /// <summary>
        /// Unescapes a single reference token, so "~1" becomes "/" and "~0" becomes "~".
        /// </summary>
        /// <param name="token">The escaped token.</param>
        public static string UnescapeToken(string token)
        {
            return token.Replace("~1", "/").Replace("~0", "~");
        }

        /// <summary>
        /// Decode the string.
        /// </summary>
        private string Decode(string token)
        {
            return UnescapeToken(Uri.UnescapeDataString(token));
        }

        /// <summary>
        /// Gets the string representation of this JSON pointer.
        /// </summary>
        public override string ToString()
        {
            return "/" + string.Join("/", this.Tokens);
        }
    }
}