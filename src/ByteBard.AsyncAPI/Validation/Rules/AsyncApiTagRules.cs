namespace ByteBard.AsyncAPI.Validation.Rules
{
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Validations;

    [AsyncApiRule]
    public static class AsyncApiTagRules
    {
        public static ValidationRule<AsyncApiTag> TagRequiredFields =>
            new ValidationRule<AsyncApiTag>(
                (context, tag) =>
                {
                    context.Enter("name");
                    if (tag.Name == null)
                    {
                        context.CreateError(
                            nameof(TagRequiredFields),
                            string.Format(Resource.Validation_FieldRequired, "name", "tag"));
                    }

                    context.Exit();
                });
    }
}