namespace ByteBard.AsyncAPI.Validation.Rules
{
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Validations;

    [AsyncApiRule]
    public static class AsyncApiServerRules
    {
        [AsyncApiVersionRule(AsyncApiVersion.AsyncApi3_0)]
        public static ValidationRule<AsyncApiServer> ServerRequiredFields =>
            new ValidationRule<AsyncApiServer>(
                (context, server) =>
                {
                    context.Enter("host");
                    if (server.Host == null)
                    {
                        context.CreateError(
                            nameof(ServerRequiredFields),
                            string.Format(Resource.Validation_FieldRequired, "host", "server"));
                    }

                    context.Exit();

                    context.Enter("protocol");
                    if (server.Protocol == null)
                    {
                        context.CreateError(
                            nameof(ServerRequiredFields),
                            string.Format(Resource.Validation_FieldRequired, "protocol", "server"));
                    }

                    context.Exit();
                });

        [AsyncApiVersionRule(AsyncApiVersion.AsyncApi2_0)]
        public static ValidationRule<AsyncApiServer> V2ServerRequiredFields =>
            new ValidationRule<AsyncApiServer>(
                (context, server) =>
                {
                    context.Enter("url");
                    if (server.Host == null)
                    {
                        context.CreateError(
                            nameof(V2ServerRequiredFields),
                            string.Format(Resource.Validation_FieldRequired, "url", "server"));
                    }
 
                    context.Exit();

                    context.Enter("protocol");
                    if (server.Protocol == null)
                    {
                        context.CreateError(
                            nameof(V2ServerRequiredFields),
                            string.Format(Resource.Validation_FieldRequired, "protocol", "server"));
                    }

                    context.Exit();
                });
    }
}