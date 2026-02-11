namespace ByteBard.AsyncAPI.Validation.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Validations;

    [AsyncApiRule]
    public static class AsyncApiDocumentRules
    {
        private static TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);

        /// <summary>
        /// The key regex.
        /// </summary>
        private static Regex keyRegex = new Regex(@"^[a-zA-Z0-9\.\-_]+$", RegexOptions.None, RegexTimeout);

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
                        if (!keyRegex.IsMatch(key))
                        {
                            context.CreateError(
                                "ChannelKeys",
                                string.Format(Resource.Validation_KeyMustMatchRegularExpr, key, "channels", keyRegex.ToString()));
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
                        if (!keyRegex.IsMatch(key))
                        {
                            context.CreateError(
                                "ServerKeys",
                                string.Format(Resource.Validation_KeyMustMatchRegularExpr, key, "servers", keyRegex.ToString()));
                        }
                    }

                    context.Exit();
                });

        [AsyncApiVersionRule(AsyncApiVersion.AsyncApi2_0)]
        public static ValidationRule<AsyncApiDocument> V2ChannelsRequired =>
            new ValidationRule<AsyncApiDocument>(
                (context, document) =>
                {
                    context.Enter("channels");
                    if (document.Channels == null || document.Channels.Count == 0)
                    {
                        context.CreateError(
                                nameof(DocumentRequiredFields),
                                string.Format(Resource.Validation_FieldRequired, "channels", "document"));
                    }

                    context.Exit();
                });
    }
}
