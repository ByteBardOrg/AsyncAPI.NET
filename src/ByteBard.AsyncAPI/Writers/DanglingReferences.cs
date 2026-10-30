namespace ByteBard.AsyncAPI.Writers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json.Nodes;

    /// <summary>
    /// A local reference can point at a location the written document does not have, for example a 2.x
    /// "#/channels/a/publish/message" written as 3.x, or a 3.x "#/operations/a/traits/0" written as 2.x.
    /// Such references are written inline instead.
    /// </summary>
    internal static class DanglingReferences
    {
        private const int MaxPasses = 10;

        /// <summary>
        /// Writes the document as JSON to find references whose target it does not contain, and returns settings that write them inline.
        /// </summary>
        /// <param name="settings">The settings the document is written with.</param>
        /// <param name="write">Writes the document to the given writer.</param>
        internal static AsyncApiWriterSettings Inline(AsyncApiWriterSettings settings, Action<IAsyncApiWriter> write)
        {
            if (settings.InlineLocalReferences)
            {
                return settings;
            }

            var dangling = new HashSet<string>();

            // Inlining a reference writes its target, which can hold further references, so repeat until no new ones dangle.
            for (var pass = 0; pass < MaxPasses; pass++)
            {
                var passSettings = dangling.Count == 0 ? settings : settings.WithDanglingReferences(new HashSet<string>(dangling));
                var json = WriteJson(passSettings, write, out var workspace);
                var document = JsonNode.Parse(json);

                // A reference nothing can resolve stays a reference, as it was written.
                var found = new HashSet<string>();
                Collect(document, document, found);
                found.RemoveWhere(reference => !workspace.Contains(reference));
                if (found.IsSubsetOf(dangling))
                {
                    return passSettings;
                }

                dangling.UnionWith(found);
            }

            return settings.WithDanglingReferences(dangling);
        }

        private static string WriteJson(AsyncApiWriterSettings settings, Action<IAsyncApiWriter> write, out AsyncApiWorkspace workspace)
        {
            using (var stringWriter = new StringWriter(settings.CultureInfo))
            {
                var writer = new AsyncApiJsonWriter(stringWriter, settings, false);
                write(writer);
                writer.Flush();
                workspace = writer.Workspace;
                return stringWriter.ToString();
            }
        }

        private static void Collect(JsonNode node, JsonNode document, ISet<string> dangling)
        {
            if (node is JsonArray array)
            {
                foreach (var item in array)
                {
                    Collect(item, document, dangling);
                }

                return;
            }

            if (node is not JsonObject map)
            {
                return;
            }

            if (map.TryGetPropertyValue("$ref", out var value)
                && value is JsonValue referenceValue
                && referenceValue.TryGetValue<string>(out var reference)
                && reference.StartsWith("#")
                && !(JsonPointer.TryFromUriFragment(reference.Substring(1), out var pointer) && pointer.Find(document) != null))
            {
                dangling.Add(reference);
            }

            foreach (var property in map)
            {
                Collect(property.Value, document, dangling);
            }
        }
    }
}
