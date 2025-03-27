namespace ByteBard.AsyncAPI.Validation.Rules
{
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Validations;

    [AsyncApiRule]
    public static class AsyncApiCorrelationIdRules
    {
        public static ValidationRule<AsyncApiCorrelationId> CorrelationIdRequiredFields =>
            new ValidationRule<AsyncApiCorrelationId>(
                (context, correlationId) =>
                {
                    context.Enter("location");
                    if (correlationId.Location == null)
                    {
                        context.CreateError(
                            nameof(CorrelationIdRequiredFields),
                            string.Format(Resource.Validation_FieldRequired, "location", "correlationId"));
                    }

                    context.Exit();
                });
    }
}