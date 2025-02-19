// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Validation.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using LEGO.AsyncAPI.Models;
    using LEGO.AsyncAPI.Validations;

    [AsyncApiRule]
    public static class AsyncApiDocumentRules
    {
        private static TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);

        /// <summary>
        /// The key regex.
        /// </summary>
        public static Regex KeyRegex = new Regex(@"^[a-zA-Z0-9\.\-_]+$", RegexOptions.None, RegexTimeout);

        public static ValidationRule<AsyncApiDocument> DocumentRequiredFields =>
            new ValidationRule<AsyncApiDocument>(
                (context, document) =>
                {
                    context.Enter("info");
                    if (document.Info == null)
                    {
                        context.CreateError(
                            nameof(DocumentRequiredFields),
                            string.Format(Resource.Validation_FieldRequired, "info", "document"));
                    }

                    context.Exit();
                });

        public static ValidationRule<AsyncApiDocument> ChannelKeyRegex =>
            new ValidationRule<AsyncApiDocument>(
                (context, document) =>
                {
                    context.Enter("channels");
                    var hashSet = new HashSet<string>();
                    foreach (var key in document.Channels?.Keys)
                    {
                        if (!KeyRegex.IsMatch(key))
                        {
                            context.CreateError(
                                "ChannelKeys",
                                string.Format(Resource.Validation_KeyMustMatchRegularExpr, key, "channels", KeyRegex.ToString()));
                        }

                        if (!hashSet.Add(key))
                        {
                            context.CreateError("ChannelKey", string.Format(Resource.Validation_ChannelsMustBeUnique));
                        }
                    }

                    context.Exit();
                });

        public static ValidationRule<AsyncApiDocument> ServerKeyRegex =>
            new ValidationRule<AsyncApiDocument>(
                (context, document) =>
                {
                    if (document.Servers?.Keys == null)
                    {
                        return;
                    }

                    context.Enter("servers");
                    foreach (var key in document.Servers?.Keys)
                    {
                        if (!KeyRegex.IsMatch(key))
                        {
                            context.CreateError(
                                "ServerKeys",
                                string.Format(Resource.Validation_KeyMustMatchRegularExpr, key, "servers", KeyRegex.ToString()));
                        }
                    }

                    context.Exit();
                });
    }
}
