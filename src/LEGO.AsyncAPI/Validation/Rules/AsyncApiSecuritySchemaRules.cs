// Copyright (c) The LEGO Group. All rights reserved.

using System.Collections.Generic;
using System.Linq;

namespace LEGO.AsyncAPI.Validation.Rules
{
    using System;
    using LEGO.AsyncAPI.Models;
    using LEGO.AsyncAPI.Validations;

    [AsyncApiRule]
    public static class AsyncApiSecuritySchemeRules
    {
        public static ValidationRule<AsyncApiSecurityScheme> SecuritySchemeRequiredFields =>
            new ValidationRule<AsyncApiSecurityScheme>(
                (context, securityScheme) =>
                {
                    context.Enter("name");
                    if (securityScheme.Name is null && securityScheme.IsFieldRequired("name"))
                    {
                        context.CreateError(
                            nameof(SecuritySchemeRequiredFields),
                            string.Format(Resource.Validation_FieldRequired, "name", "securityScheme"));
                    }

                    context.Exit();

                    context.Enter("in");
                    if (securityScheme.In is null && securityScheme.IsFieldRequired("in"))
                    {
                        context.CreateError(
                            nameof(SecuritySchemeRequiredFields),
                            string.Format(Resource.Validation_FieldRequired, "in", "securityScheme"));
                    }

                    context.Exit();

                    context.Enter("scheme");
                    if (securityScheme.Scheme is null && securityScheme.IsFieldRequired("scheme"))
                    {
                        context.CreateError(
                            nameof(SecuritySchemeRequiredFields),
                            string.Format(Resource.Validation_FieldRequired, "scheme", "securityScheme"));
                    }

                    context.Exit();

                    context.Enter("flows");
                    if (securityScheme.Flows is null && securityScheme.IsFieldRequired("flows"))
                    {
                        context.CreateError(
                            nameof(SecuritySchemeRequiredFields),
                            string.Format(Resource.Validation_FieldRequired, "flows", "securityScheme"));
                    }

                    context.Exit();

                    context.Enter("openIdConnectUrl");
                    if (securityScheme.OpenIdConnectUrl is null && securityScheme.IsFieldRequired("openIdConnectUrl"))
                    {
                        context.CreateError(
                            nameof(SecuritySchemeRequiredFields),
                            string.Format(Resource.Validation_FieldRequired, "openIdConnectUrl", "securityScheme"));
                    }

                    context.Exit();

                    context.Enter("scopes");
                    if (securityScheme.Scopes is null && securityScheme.IsFieldRequired("scopes"))
                    {
                        context.CreateError(
                            nameof(SecuritySchemeRequiredFields),
                            string.Format(Resource.Validation_FieldRequired, "scopes", "securityScheme"));
                    }

                    context.Exit();
                });

        private static bool IsFieldRequired(this AsyncApiSecurityScheme sc, string fieldName)
        {
            return RequiredFieldsByType[fieldName](sc);
        }

        private static readonly Dictionary<string, Func<AsyncApiSecurityScheme, bool>> RequiredFieldsByType = new()
        {
            { "name", sc => sc.Type is SecuritySchemeType.ApiKey },
            { "in", sc => sc.Type is SecuritySchemeType.ApiKey or SecuritySchemeType.HttpApiKey },
            { "scheme", sc => sc.Type is SecuritySchemeType.Http },
            { "flows", sc => sc.Type is SecuritySchemeType.OAuth2 },
            { "openIdConnectUrl", sc => sc.Type is SecuritySchemeType.OpenIdConnect },
            { "scopes", sc => sc.Type is SecuritySchemeType.OpenIdConnect or SecuritySchemeType.OAuth2 },
        };
    }
}