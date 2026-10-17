namespace ByteBard.AsyncAPI.Readers.ParseNodes
{
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
                    if (!int.TryParse(token, out var index) || index < 0 || index >= sequence.Count)
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