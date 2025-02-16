// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Validation.Rules
{
    using LEGO.AsyncAPI.Models;
    using LEGO.AsyncAPI.Validations;

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