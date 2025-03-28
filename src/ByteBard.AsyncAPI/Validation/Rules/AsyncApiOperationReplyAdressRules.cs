namespace ByteBard.AsyncAPI.Validation.Rules
{
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Validations;

    [AsyncApiRule]
    public static class AsyncApiOperationReplyAddressRules
    {
        public static ValidationRule<AsyncApiOperationReplyAddress> OperationReplyAddressRequiredFields =>
            new ValidationRule<AsyncApiOperationReplyAddress>(
                (context, replyAddress) =>
                {
                    context.Enter("location");
                    if (replyAddress.Location is null)
                    {
                        context.CreateError(
                            nameof(OperationReplyAddressRequiredFields),
                            string.Format(Resource.Validation_FieldRequired, "location", "replyAddress"));
                    }

                    context.Exit();
                });
    }
}