// Copyright (c) The LEGO Group. All rights reserved.

using System.Linq;

namespace LEGO.AsyncAPI.Validation.Rules
{
    using System;
    using System.Threading;
    using LEGO.AsyncAPI.Models;
    using LEGO.AsyncAPI.Validations;

    [AsyncApiRule]
    public static class AsyncApiOperationRules
    {
        public static ValidationRule<AsyncApiOperation> OperationRequiredFields =>
            new ValidationRule<AsyncApiOperation>(
                (context, operation) =>
                {
                    context.Enter("action");
                    if (!Enum.IsDefined(typeof(AsyncApiAction), operation.Action))
                    {
                        context.CreateError(
                            nameof(OperationRequiredFields),
                            string.Format(Resource.Validation_FieldRequired, "action", "operation"));
                    }

                    context.Exit();

                    context.Enter("channel");
                    if (operation.Channel is null)
                    {
                        context.CreateError(
                            nameof(OperationRequiredFields),
                            string.Format(Resource.Validation_FieldRequired, "channel", "operation"));
                    }

                    context.Exit();
                });

        public static ValidationRule<AsyncApiOperation> OperationChannelReference =>
            new ValidationRule<AsyncApiOperation>(
                (context, operation) =>
                {
                    if (context.RootDocument?.Operations.Values.FirstOrDefault(op => op == operation) is null)
                    {
                        return;
                    }

                    var channels =
                        context.RootDocument.Channels.Values.Where(channel => channel.Equals(operation.Channel));

                    var referencedChannel = channels.FirstOrDefault(c => c.Equals(operation.Channel));
                    if (referencedChannel == null)
                    {
                        context.CreateError(
                            "OperationChannelRef",
                            string.Format(Resource.Validation_OperationMustReferenceValidChannel, operation.Title));
                        return;
                    }
                    if (!operation.Messages.All(refMessage => referencedChannel.Messages.Any(message => message.Equals(refMessage))))
                    {
                        context.CreateError(
                            "OperationChannelRef",
                            string.Format(Resource.Validation_OperationMessagesMustReferenceOperationChannel, operation.Title));
                        return;
                    }
                });

        public static ValidationRule<AsyncApiOperation> OperationMessages =>
            new ValidationRule<AsyncApiOperation>(
                (context, operation) =>
                {
                    var channels =
                        context.RootDocument.Channels.Values.Where(channel => channel.Equals(operation.Channel));

                    var referencedChannel = channels.FirstOrDefault(c => c.Equals(operation.Channel));
                    if (!operation.Messages.All(refMessage => referencedChannel.Messages.Any(message => message.Equals(refMessage))))
                    {
                        context.CreateError(
                            "OperationChannelRef",
                            string.Format(Resource.Validation_OperationMessagesMustReferenceOperationChannel, operation.Title));
                        return;
                    }
                });
    }
}