namespace ByteBard.AsyncAPI.Readers.ParseNodes
{
    using System.Globalization;
    using System.Text.Json.Nodes;

    public static class JsonPointerExtensions
    {
        public static JsonNode Find(this JsonPointer currentPointer, JsonNode baseJsonNode)
        {
            if (currentPointer.Tokens.Length == 0)
            {
                return baseJsonNode;
            }

            var pointer = baseJsonNode;
            foreach (var token in currentPointer.Tokens)
            {
                if (pointer is JsonArray sequence)
                {
                    // RFC 6901 array indices are "0" or digits without a leading zero.
                    if ((token.Length > 1 && token[0] == '0')
                        || !int.TryParse(token, NumberStyles.None, CultureInfo.InvariantCulture, out var index)
                        || index >= sequence.Count)
                    {
                        return null;
                    }

                    pointer = sequence[index];
                }
                else if (pointer is not JsonObject map || !map.TryGetPropertyValue(token, out pointer))
                {
                    // The token walks into a scalar or a missing property.
                    return null;
                }
            }

            return pointer;
        }
    }
}