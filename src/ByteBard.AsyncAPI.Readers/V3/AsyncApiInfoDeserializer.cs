namespace ByteBard.AsyncAPI.Readers
{
    using System;
    using ByteBard.AsyncAPI.Extensions;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers.ParseNodes;

    internal static partial class AsyncApiV3Deserializer
    {
        private static FixedFieldMap<AsyncApiInfo> infoFixedFields = new()
        {
            { "title", (a, n) => { a.Title = n.GetScalarValue(); } },
            { "version", (a, n) => { a.Version = n.GetScalarValue(); } },
            { "description", (a, n) => { a.Description = n.GetScalarValue(); } },
            { "termsOfService", (a, n) => { a.TermsOfService = new Uri(n.GetScalarValue()); } },
            { "contact", (a, n) => { a.Contact = LoadContact(n); } },
            { "license", (a, n) => { a.License = LoadLicense(n); } },
            { "tags", (a, n) => { a.Tags = n.CreateList(LoadTag); } },
            { "externalDocs", (a, n) => { a.ExternalDocs = LoadExternalDocs(n); } },
        };

        private static PatternFieldMap<AsyncApiInfo> infoPatternFields =
            new()
            {
                { s => s.StartsWith("x-"), (a, p, n) => a.AddExtension(p, LoadExtension(p, n)) },
            };

        public static AsyncApiInfo LoadInfo(ParseNode node)
        {
            var mapNode = node.CheckMapNode("info");
            var info = new AsyncApiInfo();

            ParseMap(mapNode, info, infoFixedFields, infoPatternFields);

            return info;
        }
    }
}
