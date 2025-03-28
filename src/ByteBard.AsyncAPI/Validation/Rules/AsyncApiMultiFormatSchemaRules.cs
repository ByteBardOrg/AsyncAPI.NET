namespace ByteBard.AsyncAPI.Validation.Rules
{
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Validations;

    [AsyncApiRule]
    public static class AsyncApiMultiFormatSchemaRules
    {
        public static ValidationRule<AsyncApiMultiFormatSchema> MultiFormatSchemaRequiredFields =>
            new ValidationRule<AsyncApiMultiFormatSchema>(
                (context, multiFormatSchema) =>
                {
                    context.Enter("schema");
                    if (multiFormatSchema.Schema == null)
                    {
                        context.CreateError(
                            nameof(MultiFormatSchemaRequiredFields),
                            string.Format(Resource.Validation_FieldRequired, "schema", "multiFormatSchema"));
                    }

                    context.Exit();
                });
    }
}