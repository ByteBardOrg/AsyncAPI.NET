// Copyright (c) The LEGO Group. All rights reserved.

using System.Collections.Generic;

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
                    context.Enter("type");
                    if (!Enum.IsDefined(typeof(SecuritySchemeType), securityScheme.Type))
                    {
                        context.CreateError(
                            nameof(SecuritySchemeRequiredFields),
                            string.Format(Resource.Validation_FieldRequired, "type", "securityScheme"));
                    }

                    context.Exit();

                    context.Enter("name");
                    if (string.IsNullOrWhiteSpace(securityScheme.Name) && securityScheme.IsFieldRequired("name"))
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
                    if (string.IsNullOrWhiteSpace(securityScheme.Scheme) && securityScheme.IsFieldRequired("scheme"))
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
                });

        public static ValidationRule<AsyncApiSecurityScheme> OpenIdConnectUrlMustBeAbsolute =>
            new ValidationRule<AsyncApiSecurityScheme>(
                (context, securityScheme) =>
                {
                    context.Enter("openIdConnectUrl");
                    if (securityScheme.OpenIdConnectUrl != null && !securityScheme.OpenIdConnectUrl.IsAbsoluteUri)
                    {
                        context.CreateError(
                            nameof(OpenIdConnectUrlMustBeAbsolute),
                            string.Format(Resource.Validation_MustBeAbsoluteUrl, "openIdConnectUrl", "securityScheme"));
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
        };
    }
}