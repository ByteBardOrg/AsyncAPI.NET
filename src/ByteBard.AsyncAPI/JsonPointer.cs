namespace ByteBard.AsyncAPI
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.Json.Nodes;
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
        /// Tries to create a pointer from its URI fragment identifier representation (RFC 6901 section 6).
        /// </summary>
        /// <param name="fragment">The fragment, without the leading <c>#</c>.</param>
        /// <param name="pointer">The pointer, or <c>null</c> when the fragment is not a valid JSON pointer.</param>
        /// <returns><c>true</c> when the fragment is a valid JSON pointer.</returns>
        public static bool TryFromUriFragment(string fragment, out JsonPointer pointer)
        {
            pointer = null;
            if (fragment == null)
            {
                return false;
            }

            try
            {
                pointer = FromUriFragment(fragment);
                return true;
            }
            catch (AsyncApiException)
            {
                return false;
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
        /// Finds the node this pointer references.
        /// </summary>
        /// <param name="document">The document to evaluate the pointer against.</param>
        /// <returns>The referenced node, or <c>null</c> when it does not exist.</returns>
        public JsonNode Find(JsonNode document)
        {
            var node = document;
            foreach (var token in this.Tokens)
            {
                if (node is JsonArray sequence)
                {
                    // RFC 6901 array indices are "0" or digits without a leading zero.
                    if ((token.Length > 1 && token[0] == '0')
                        || !int.TryParse(token, NumberStyles.None, CultureInfo.InvariantCulture, out var index)
                        || index >= sequence.Count)
                    {
                        return null;
                    }

                    node = sequence[index];
                }
                else if (node is not JsonObject map || !map.TryGetPropertyValue(token, out node))
                {
                    // The token walks into a scalar or a missing property.
                    return null;
                }
            }

            return node;
        }

        /// <summary>
        /// Gets the JSON string representation of this JSON pointer.
        /// </summary>
        public override string ToString()
        {
            return string.Concat(this.Tokens.Select(token => "/" + EscapeToken(token)));
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
