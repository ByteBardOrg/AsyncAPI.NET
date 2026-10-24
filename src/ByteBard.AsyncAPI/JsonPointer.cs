namespace ByteBard.AsyncAPI
{
    using System;
    using System.Linq;
    using System.Text;
    using ByteBard.AsyncAPI.Exceptions;

    /// <summary>
    /// JSON pointer as defined by RFC 6901.
    /// </summary>
    public class JsonPointer
    {
        /// <summary>
        /// Initializes the <see cref="JsonPointer"/> class.
        /// </summary>
        /// <param name="pointer">Pointer in its JSON string representation (RFC 6901 section 5), e.g. <c>/components/schemas/a~1b</c>.
        /// An empty string points to the whole document; <c>/</c> points to the member with an empty key.</param>
        /// <exception cref="AsyncApiException">The pointer is malformed.</exception>
        public JsonPointer(string pointer)
        {
            if (pointer == null)
            {
                throw new ArgumentNullException(nameof(pointer));
            }

            if (pointer.Length == 0)
            {
                this.Tokens = new string[0];
                return;
            }

            if (pointer[0] != '/')
            {
                throw new AsyncApiException($"The JSON pointer '{pointer}' is malformed: it must be empty or start with '/'.");
            }

            this.Tokens = pointer.Substring(1).Split('/').Select(token => Unescape(token, pointer)).ToArray();
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
        /// Creates a pointer from its URI fragment identifier representation (RFC 6901 section 6), e.g. the part after <c>#</c>
        /// in <c>schemas.json#/components/schemas/with%20space</c>. The fragment is percent-decoded before it is parsed.
        /// </summary>
        /// <param name="fragment">The fragment, without the leading <c>#</c>.</param>
        /// <exception cref="AsyncApiException">The fragment is not a valid JSON pointer.</exception>
        public static JsonPointer FromUriFragment(string fragment)
        {
            if (fragment == null)
            {
                throw new ArgumentNullException(nameof(fragment));
            }

            return new JsonPointer(Uri.UnescapeDataString(fragment));
        }

        /// <summary>
        /// Gets the JSON string representation of this JSON pointer.
        /// </summary>
        public override string ToString()
        {
            return string.Concat(this.Tokens.Select(token => "/" + token.Replace("~", "~0").Replace("/", "~1")));
        }

        private static string Unescape(string token, string pointer)
        {
            if (token.IndexOf('~') < 0)
            {
                return token;
            }

            var builder = new StringBuilder(token.Length);
            for (var i = 0; i < token.Length; i++)
            {
                if (token[i] != '~')
                {
                    builder.Append(token[i]);
                    continue;
                }

                var next = i + 1 < token.Length ? token[i + 1] : '\0';
                if (next == '0')
                {
                    builder.Append('~');
                }
                else if (next == '1')
                {
                    builder.Append('/');
                }
                else
                {
                    throw new AsyncApiException($"The JSON pointer '{pointer}' is malformed: '~' must be followed by '0' or '1'.");
                }

                i++;
            }

            return builder.ToString();
        }
    }
}
