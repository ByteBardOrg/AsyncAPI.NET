// Copyright (c) The LEGO Group. All rights reserved.

using System.Collections.Generic;
using System.Linq;

namespace LEGO.AsyncAPI.Validation.Rules
{
    using System;
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
                        context.RootDocument.Channels.Values.Where(channel => operation.Channel.Equals(channel));

                    var referencedChannel = channels.FirstOrDefault(c => operation.Channel.Equals(c));
                    if (referencedChannel == null)
                    {
                        context.CreateError(
                            "OperationChannelRef",
                            string.Format(Resource.Validation_OperationMustReferenceValidChannel, operation.Title));
                    }
                });

        public static ValidationRule<AsyncApiOperation> OperationMessages =>
            new ValidationRule<AsyncApiOperation>(
                (context, operation) =>
                {
                    if (context.RootDocument?.Operations.Values.FirstOrDefault(op => op == operation) is null)
                    {
                        return;
                    }

                    var channels =
                        context.RootDocument.Channels.Values.Where(channel => operation.Channel.Equals(channel));

                    var referencedChannel = channels.FirstOrDefault(c => operation.Channel.Equals(c));

                    if (referencedChannel == null)
                    {
                        return;
                    }

                    if (!AllOperationsMessagesReferencesChannelMessages(operation.Messages, referencedChannel.Messages.Values))
                    {
                        context.CreateError(
                            "OperationChannelRef",
                            string.Format(Resource.Validation_OperationMessagesMustReferenceOperationChannel, operation.Title));
                    }
                });

        private static bool AllOperationsMessagesReferencesChannelMessages(
            IList<AsyncApiMessageReference> operationMessages, ICollection<AsyncApiMessage> channelMessages) =>
            operationMessages.All(opMessage => OperationMessageReferencesAnyChannelMessage(opMessage, channelMessages));

        private static bool OperationMessageReferencesAnyChannelMessage(
            AsyncApiMessageReference operationMessage, ICollection<AsyncApiMessage> channelMessages) =>
            channelMessages.Any(channelMessage => channelMessage.Equals(operationMessage));
    }
}