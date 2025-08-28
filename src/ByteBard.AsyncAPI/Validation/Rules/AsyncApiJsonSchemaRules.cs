namespace ByteBard.AsyncAPI.Validation.Rules;

using Models;
using Validations;

[AsyncApiRule]
public static class AsyncApiJsonSchemaRules
{
    public static ValidationRule<AsyncApiJsonSchema> NonNegativeFields =>
        new ValidationRule<AsyncApiJsonSchema>((context, schema) =>
        {
            context.Enter("maxLength");
            if (schema.MaxLength != null && schema.MaxLength < 0)
            {
                context.CreateError(
                    nameof(NonNegativeFields),
                    string.Format(Resource.Validation_MustBeNonNegative, "maxLength"));
            }

            context.Exit();

            context.Enter("minLength");
            if (schema.MinLength != null && schema.MinLength < 0)
            {
                context.CreateError(
                    nameof(NonNegativeFields),
                    string.Format(Resource.Validation_MustBeNonNegative, "minLength"));
            }

            context.Exit();

            context.Enter("maxItems");
            if (schema.MaxItems != null && schema.MaxItems < 0)
            {
                context.CreateError(
                    nameof(NonNegativeFields),
                    string.Format(Resource.Validation_MustBeNonNegative, "maxItems"));
            }

            context.Exit();

            context.Enter("minItems");
            if (schema.MinItems != null && schema.MinItems < 0)
            {
                context.CreateError(
                    nameof(NonNegativeFields),
                    string.Format(Resource.Validation_MustBeNonNegative, "minItems"));
            }

            context.Exit();

            context.Enter("maxProperties");
            if (schema.MaxProperties != null && schema.MaxProperties < 0)
            {
                context.CreateError(
                    nameof(NonNegativeFields),
                    string.Format(Resource.Validation_MustBeNonNegative, "maxProperties"));
            }

            context.Exit();

            context.Enter("minProperties");
            if (schema.MinProperties != null && schema.MinProperties < 0)
            {
                context.CreateError(
                    nameof(NonNegativeFields),
                    string.Format(Resource.Validation_MustBeNonNegative, "minProperties"));
            }

            context.Exit();
        });
}