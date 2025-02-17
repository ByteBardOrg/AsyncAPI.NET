// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Readers
{
    using LEGO.AsyncAPI.Models;
    using LEGO.AsyncAPI.Readers.ParseNodes;
    using System.Collections.Generic;

    /// <summary>
    /// Class containing logic to deserialize AsyncApi document into
    /// runtime AsyncApi object model.
    /// </summary>
    internal static partial class AsyncApiV2Deserializer
    {
        public static AsyncApiSecurityScheme LoadSecurityRequirement(ParseNode node)
        {
            var mapNode = node.CheckMapNode("security");

            var securityScheme = new AsyncApiSecurityScheme();

            foreach (var property in mapNode)
            {
                var scheme = LoadSecuritySchemeByReference(mapNode.Context, property.Name);
                var scopes = property.Value.CreateSimpleList(value => value.GetScalarValue());
                if (scheme != null)
                {
                    node.Context.SetTempStorage(TempStorageKeys.SecuritySchemeScopes, scopes, property.Name);
                    return scheme;
                }
                else
                {
                    mapNode.Context.Diagnostic.Errors.Add(
                        new AsyncApiError(node.Context.GetLocation(), $"Scheme {property.Name} is not found"));
                }
            }

            return null;
        }

        private static AsyncApiSecuritySchemeReference LoadSecuritySchemeByReference(
            ParsingContext context,
            string schemeName)
        {
            var securitySchemeObject = new AsyncApiSecuritySchemeReference("#/components/securitySchemes/" + schemeName);

            return securitySchemeObject;
        }
    }
}
