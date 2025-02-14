// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Writers;

    /// <summary>
    /// Holds a set of reusable objects for different aspects of the AsyncAPI specification.
    /// </summary>
    /// <remarks>
    /// All objects defined within the components object will have no effect on the API unless they are explicitly referenced from properties outside the components object.
    /// </remarks>
    public class AsyncApiComponents : IAsyncApiExtensible, IAsyncApiSerializable
    {
        /// <summary>
        /// An object to hold reusable Schema Object.
        /// </summary>
        /// <remarks>
        /// If this is a Schema Object, then the schemaFormat will be assumed to be "application/vnd.aai.asyncapi+json;version=asyncapi" where the version is equal to the AsyncAPI Version String.
        /// </remarks>
        public IDictionary<string, AsyncApiMultiFormatSchema> Schemas { get; set; } = new Dictionary<string, AsyncApiMultiFormatSchema>();

        /// <summary>
        /// An object to hold reusable Server Objects.
        /// </summary>
        public IDictionary<string, AsyncApiServer> Servers { get; set; } = new Dictionary<string, AsyncApiServer>();


        /// <summary>
        /// An object to hold reusable Channel Item Objects.
        /// </summary>
        public IDictionary<string, AsyncApiChannel> Channels { get; set; } = new Dictionary<string, AsyncApiChannel>();

        /// <summary>
        /// An object to hold reusable Operation Objects.
        /// </summary>
        public IDictionary<string, AsyncApiOperation> Operations { get; set; } = new Dictionary<string, AsyncApiOperation>();

        /// <summary>
        /// An object to hold reusable Message Objects.
        /// </summary>
        public IDictionary<string, AsyncApiMessage> Messages { get; set; } = new Dictionary<string, AsyncApiMessage>();

        /// <summary>
        /// An object to hold reusable Security Scheme Objects.
        /// </summary>
        public IDictionary<string, AsyncApiSecurityScheme> SecuritySchemes { get; set; } = new Dictionary<string, AsyncApiSecurityScheme>();


        /// <summary>
        /// An object to hold reusable Server Variable Objects.
        /// </summary>
        public IDictionary<string, AsyncApiServerVariable> ServerVariables { get; set; } = new Dictionary<string, AsyncApiServerVariable>();

        /// <summary>
        /// An object to hold reusable Parameter Objects.
        /// </summary>
        public IDictionary<string, AsyncApiParameter> Parameters { get; set; } = new Dictionary<string, AsyncApiParameter>();

        /// <summary>
        /// An object to hold reusable Correlation ID Objects.
        /// </summary>
        public IDictionary<string, AsyncApiCorrelationId> CorrelationIds { get; set; } = new Dictionary<string, AsyncApiCorrelationId>();

        /// <summary>
        /// An object to hold reusable Operation Reply Objects.
        /// </summary>
        public IDictionary<string, AsyncApiOperationReply> Replies { get; set; } = new Dictionary<string, AsyncApiOperationReply>();

        /// <summary>
        /// An object to hold reusable Operation Reply Address Objects.
        /// </summary>
        public IDictionary<string, AsyncApiOperationReplyAddress> ReplyAddresses { get; set; } = new Dictionary<string, AsyncApiOperationReplyAddress>();

        /// <summary>
        /// An object to hold reusable External Documentation Objects.
        /// </summary>
        public IDictionary<string, AsyncApiExternalDocumentation> ExternalDocs { get; set; } = new Dictionary<string, AsyncApiExternalDocumentation>();

        /// <summary>
        /// An object to hold reusable Tag Objects.
        /// </summary>
        public IDictionary<string, AsyncApiTag> Tags { get; set; } = new Dictionary<string, AsyncApiTag>();

        /// <summary>
        /// An object to hold reusable Operation Trait Objects.
        /// </summary>
        public IDictionary<string, AsyncApiOperationTrait> OperationTraits { get; set; } = new Dictionary<string, AsyncApiOperationTrait>();

        /// <summary>
        /// An object to hold reusable Message Trait Objects.
        /// </summary>
        public IDictionary<string, AsyncApiMessageTrait> MessageTraits { get; set; } = new Dictionary<string, AsyncApiMessageTrait>();

        /// <summary>
        /// An object to hold reusable Server Bindings Objects.
        /// </summary>
        public IDictionary<string, AsyncApiBindings<IServerBinding>> ServerBindings { get; set; } = new Dictionary<string, AsyncApiBindings<IServerBinding>>();

        /// <summary>
        /// An object to hold reusable Channel Bindings Objects.
        /// </summary>
        public IDictionary<string, AsyncApiBindings<IChannelBinding>> ChannelBindings { get; set; } = new Dictionary<string, AsyncApiBindings<IChannelBinding>>();

        /// <summary>
        /// An object to hold reusable Operation Bindings Objects.
        /// </summary>
        public IDictionary<string, AsyncApiBindings<IOperationBinding>> OperationBindings { get; set; } = new Dictionary<string, AsyncApiBindings<IOperationBinding>>();

        /// <summary>
        /// An object to hold reusable Message Bindings Objects.
        /// </summary>
        public IDictionary<string, AsyncApiBindings<IMessageBinding>> MessageBindings { get; set; } = new Dictionary<string, AsyncApiBindings<IMessageBinding>>();

        public IDictionary<string, IAsyncApiExtension> Extensions { get; set; } = new Dictionary<string, IAsyncApiExtension>();

        public void SerializeV2(IAsyncApiWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            // If references have been inlined we don't need the to render the components section
            // however if they have cycles, then we will need a component rendered
            if (writer.GetSettings().InlineLocalReferences)
            {
                var loops = writer.GetSettings().LoopDetector.Loops;
                writer.WriteStartObject();
                if (loops.TryGetValue(typeof(AsyncApiJsonSchemaReference), out List<object> schemas))
                {
                    var asyncApiSchemas = schemas.Cast<AsyncApiJsonSchemaReference>().Distinct().ToList()
                        .ToDictionary<AsyncApiJsonSchemaReference, string>(k => k.Reference.FragmentId);

                    writer.WriteOptionalMap(
                       AsyncApiConstants.Schemas,
                       this.Schemas,
                       (w, key, component) =>
                        {
                            component.SerializeV2(w);
                        });
                }

                writer.WriteEndObject();
                return;
            }

            writer.WriteStartObject();

            // Serialize each referenceable object as full object without reference if the reference in the object points to itself.
            // If the reference exists but points to other objects, the object is serialized to just that reference.

            // schemas
            writer.WriteOptionalMap(
                AsyncApiConstants.Schemas,
                this.Schemas,
                (w, key, component) =>
                {
                    if (component is AsyncApiMultiFormatSchemaReference reference)
                    {
                        reference.Schema.SerializeV2(w);
                    }
                    else
                    {
                        component.Schema.SerializeV2(w);
                    }
                });

            // servers
            writer.WriteOptionalMap(
                AsyncApiConstants.Servers,
                this.Servers,
                (w, key, component) =>
                {
                    if (component is AsyncApiServerReference reference)
                    {
                        reference.SerializeV2(w);
                    }
                    else
                    {
                        component.SerializeV2(w);
                    }
                });

            // servers
            writer.WriteOptionalMap(
                AsyncApiConstants.ServerVariables,
                this.ServerVariables,
                (w, key, component) =>
                {
                    if (component is AsyncApiServerVariableReference reference)
                    {
                        reference.SerializeV2(w);
                    }
                    else
                    {
                        component.SerializeV2(w);
                    }
                });

            // channels
            writer.WriteOptionalMap(
                AsyncApiConstants.Channels,
                this.Channels,
                (w, key, component) =>
                {
                    if (component is AsyncApiChannelReference reference)
                    {
                        reference.SerializeV2(w);
                    }
                    else
                    {
                        component.SerializeV2(w);
                    }
                });

            // messages
            writer.WriteOptionalMap(
                AsyncApiConstants.Messages,
                this.Messages,
                (w, key, component) =>
                {
                    if (component is AsyncApiMessageReference reference)
                    {
                        reference.SerializeV2(w);
                    }
                    else
                    {
                        component.SerializeV2(w);
                    }
                });

            // securitySchemes
            writer.WriteOptionalMap(
                AsyncApiConstants.SecuritySchemes,
                this.SecuritySchemes,
                (w, key, component) =>
                {
                    if (component is AsyncApiSecuritySchemeReference reference)
                    {
                        reference.SerializeV2(w);
                    }
                    else
                    {
                        component.SerializeV2(w);
                    }
                });

            // parameters
            writer.WriteOptionalMap(
                AsyncApiConstants.Parameters,
                this.Parameters,
                (w, key, component) =>
                {
                    if (component is AsyncApiParameterReference reference)
                    {
                        reference.SerializeV2(w);
                    }
                    else
                    {
                        component.SerializeV2(w);
                    }
                });

            // correlationIds
            writer.WriteOptionalMap(
                AsyncApiConstants.CorrelationIds,
                this.CorrelationIds,
                (w, key, component) =>
                {
                    if (component is AsyncApiCorrelationIdReference reference)
                    {
                        reference.SerializeV2(w);
                    }
                    else
                    {
                        component.SerializeV2(w);
                    }
                });

            // operationTraits
            writer.WriteOptionalMap(
                AsyncApiConstants.OperationTraits,
                this.OperationTraits,
                (w, key, component) =>
                {
                    if (component is AsyncApiOperationTraitReference reference)
                    {
                        reference.SerializeV2(w);
                    }
                    else
                    {
                        component.SerializeV2(w);
                    }
                });

            // messageTraits
            writer.WriteOptionalMap(
                AsyncApiConstants.MessageTraits,
                this.MessageTraits,
                (w, key, component) =>
                {
                    if (component is AsyncApiMessageTraitReference reference)
                    {
                        reference.SerializeV2(w);
                    }
                    else
                    {
                        component.SerializeV2(w);
                    }
                });

            //// serverBindings
            writer.WriteOptionalMap(
                AsyncApiConstants.ServerBindings,
                this.ServerBindings,
                (w, key, component) =>
                {
                    if (component is AsyncApiBindingsReference<IServerBinding> reference)
                    {
                        reference.SerializeV2(w);
                    }
                    else
                    {
                        component.SerializeV2(w);
                    }
                });

            //// channelBindings
            writer.WriteOptionalMap(
                AsyncApiConstants.ChannelBindings,
                this.ChannelBindings,
                (w, key, component) =>
                {
                    if (component is AsyncApiBindingsReference<IChannelBinding> reference)
                    {
                        reference.SerializeV2(w);
                    }
                    else
                    {
                        component.SerializeV2(w);
                    }
                });

            //// operationBindings
            writer.WriteOptionalMap(
                AsyncApiConstants.OperationBindings,
                this.OperationBindings,
                (w, key, component) =>
                {
                    if (component is AsyncApiBindingsReference<IOperationBinding> reference)
                    {
                        reference.SerializeV2(w);
                    }
                    else
                    {
                        component.SerializeV2(w);
                    }
                });

            //// messageBindings
            writer.WriteOptionalMap(
                AsyncApiConstants.MessageBindings,
                this.MessageBindings,
                (w, key, component) =>
                {
                    if (component is AsyncApiBindingsReference<IMessageBinding> reference)
                    {
                        reference.SerializeV2(w);
                    }
                    else
                    {
                        component.SerializeV2(w);
                    }
                });

            // extensions
            writer.WriteExtensions(this.Extensions);

            writer.WriteEndObject();
        }

        public void SerializeV3(IAsyncApiWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            // If references have been inlined we don't need the to render the components section
            // however if they have cycles, then we will need a component rendered
            if (writer.GetSettings().InlineLocalReferences)
            {
                var loops = writer.GetSettings().LoopDetector.Loops;
                writer.WriteStartObject();
                if (loops.TryGetValue(typeof(AsyncApiMultiFormatSchemaReference), out List<object> schemas))
                {
                    var asyncApiSchemas = schemas.Cast<AsyncApiMultiFormatSchemaReference>().Distinct().ToList()
                        .ToDictionary<AsyncApiMultiFormatSchemaReference, string>(k => k.Reference.FragmentId);

                    writer.WriteOptionalMap(
                       AsyncApiConstants.Schemas,
                       this.Schemas,
                       (w, key, component) =>
                        {
                            component.SerializeV3(w);
                        });
                }

                writer.WriteEndObject();
                return;
            }

            writer.WriteStartObject();

            // Serialize each referenceable object as full object without reference if the reference in the object points to itself.
            // If the reference exists but points to other objects, the object is serialized to just that reference.

            // schemas
            writer.WriteOptionalMap(
                AsyncApiConstants.Schemas,
                this.Schemas,
                (w, key, component) =>
                {
                    if (component is AsyncApiMultiFormatSchemaReference reference)
                    {
                        reference.SerializeV3(w);
                    }
                    else
                    {
                        component.SerializeV3(w);
                    }
                });

            // servers
            writer.WriteOptionalMap(
                AsyncApiConstants.Servers,
                this.Servers,
                (w, key, component) =>
                {
                    if (component is AsyncApiServerReference reference)
                    {
                        reference.SerializeV3(w);
                    }
                    else
                    {
                        component.SerializeV3(w);
                    }
                });

            // servers
            writer.WriteOptionalMap(
                AsyncApiConstants.ServerVariables,
                this.ServerVariables,
                (w, key, component) =>
                {
                    if (component is AsyncApiServerVariableReference reference)
                    {
                        reference.SerializeV3(w);
                    }
                    else
                    {
                        component.SerializeV3(w);
                    }
                });

            // channels
            writer.WriteOptionalMap(
                AsyncApiConstants.Channels,
                this.Channels,
                (w, key, component) =>
                {
                    if (component is AsyncApiChannelReference reference)
                    {
                        reference.SerializeV3(w);
                    }
                    else
                    {
                        component.SerializeV3(w);
                    }
                });

            // operations
            writer.WriteOptionalMap(
                AsyncApiConstants.Operations,
                this.Operations,
                (w, key, component) =>
                {
                    if (component is AsyncApiOperationReference reference)
                    {
                        reference.SerializeV3(w);
                    }
                    else
                    {
                        component.SerializeV3(w);
                    }
                });

            // messages
            writer.WriteOptionalMap(
                AsyncApiConstants.Messages,
                this.Messages,
                (w, key, component) =>
                {
                    if (component is AsyncApiMessageReference reference)
                    {
                        reference.SerializeV3(w);
                    }
                    else
                    {
                        component.SerializeV3(w);
                    }
                });

            // securitySchemes
            writer.WriteOptionalMap(
                AsyncApiConstants.SecuritySchemes,
                this.SecuritySchemes,
                (w, key, component) =>
                {
                    if (component is AsyncApiSecuritySchemeReference reference)
                    {
                        reference.SerializeV3(w);
                    }
                    else
                    {
                        component.SerializeV3(w);
                    }
                });

            // server variables
            writer.WriteOptionalMap(
                AsyncApiConstants.ServerVariables,
                this.ServerVariables,
                (w, key, component) =>
                {
                    if (component is AsyncApiServerVariableReference reference)
                    {
                        reference.SerializeV3(w);
                    }
                    else
                    {
                        component.SerializeV3(w);
                    }
                });

            // parameters
            writer.WriteOptionalMap(
                AsyncApiConstants.Parameters,
                this.Parameters,
                (w, key, component) =>
                {
                    if (component is AsyncApiParameterReference reference)
                    {
                        reference.SerializeV3(w);
                    }
                    else
                    {
                        component.SerializeV3(w);
                    }
                });

            // correlationIds
            writer.WriteOptionalMap(
                AsyncApiConstants.CorrelationIds,
                this.CorrelationIds,
                (w, key, component) =>
                {
                    if (component is AsyncApiCorrelationIdReference reference)
                    {
                        reference.SerializeV3(w);
                    }
                    else
                    {
                        component.SerializeV3(w);
                    }
                });

            // replies
            writer.WriteOptionalMap(
                AsyncApiConstants.Reply,
                this.Replies,
                (w, key, component) =>
                {
                    if (component is AsyncApiOperationReplyReference reference)
                    {
                        reference.SerializeV3(w);
                    }
                    else
                    {
                        component.SerializeV3(w);
                    }
                });

            // replies
            writer.WriteOptionalMap(
                AsyncApiConstants.Address,
                this.ReplyAddresses,
                (w, key, component) =>
                {
                    if (component is AsyncApiOperationReplyAddressReference reference)
                    {
                        reference.SerializeV3(w);
                    }
                    else
                    {
                        component.SerializeV3(w);
                    }
                });

            // external docs
            writer.WriteOptionalMap(
                AsyncApiConstants.ExternalDocs,
                this.ExternalDocs,
                (w, key, component) =>
                {
                    if (component is AsyncApiExternalDocumentationReference reference)
                    {
                        reference.SerializeV3(w);
                    }
                    else
                    {
                        component.SerializeV3(w);
                    }
                });

            // tags
            writer.WriteOptionalMap(
                AsyncApiConstants.Tags,
                this.Tags,
                (w, key, component) =>
                {
                    if (component is AsyncApiTagReference reference)
                    {
                        reference.SerializeV3(w);
                    }
                    else
                    {
                        component.SerializeV3(w);
                    }
                });

            // operationTraits
            writer.WriteOptionalMap(
                AsyncApiConstants.OperationTraits,
                this.OperationTraits,
                (w, key, component) =>
                {
                    if (component is AsyncApiOperationTraitReference reference)
                    {
                        reference.SerializeV3(w);
                    }
                    else
                    {
                        component.SerializeV3(w);
                    }
                });

            // messageTraits
            writer.WriteOptionalMap(
                AsyncApiConstants.MessageTraits,
                this.MessageTraits,
                (w, key, component) =>
                {
                    if (component is AsyncApiMessageTraitReference reference)
                    {
                        reference.SerializeV3(w);
                    }
                    else
                    {
                        component.SerializeV3(w);
                    }
                });

            //// serverBindings
            writer.WriteOptionalMap(
                AsyncApiConstants.ServerBindings,
                this.ServerBindings,
                (w, key, component) =>
                {
                    if (component is AsyncApiBindingsReference<IServerBinding> reference)
                    {
                        reference.SerializeV3(w);
                    }
                    else
                    {
                        component.SerializeV3(w);
                    }
                });

            //// channelBindings
            writer.WriteOptionalMap(
                AsyncApiConstants.ChannelBindings,
                this.ChannelBindings,
                (w, key, component) =>
                {
                    if (component is AsyncApiBindingsReference<IChannelBinding> reference)
                    {
                        reference.SerializeV3(w);
                    }
                    else
                    {
                        component.SerializeV3(w);
                    }
                });

            //// operationBindings
            writer.WriteOptionalMap(
                AsyncApiConstants.OperationBindings,
                this.OperationBindings,
                (w, key, component) =>
                {
                    if (component is AsyncApiBindingsReference<IOperationBinding> reference)
                    {
                        reference.SerializeV3(w);
                    }
                    else
                    {
                        component.SerializeV3(w);
                    }
                });

            //// messageBindings
            writer.WriteOptionalMap(
                AsyncApiConstants.MessageBindings,
                this.MessageBindings,
                (w, key, component) =>
                {
                    if (component is AsyncApiBindingsReference<IMessageBinding> reference)
                    {
                        reference.SerializeV3(w);
                    }
                    else
                    {
                        component.SerializeV3(w);
                    }
                });

            // extensions
            writer.WriteExtensions(this.Extensions);

            writer.WriteEndObject();
        }
    }
}