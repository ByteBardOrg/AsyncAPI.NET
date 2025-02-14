// Copyright (c) The LEGO Group. All rights reserved.

using System;

namespace LEGO.AsyncAPI.Models
{
    using System.Collections.Generic;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Writers;

    /// <summary>
    /// The definition of an operation trait this application MAY use.
    /// </summary>
    public class AsyncApiOperationReference : AsyncApiOperation, IAsyncApiReferenceable
    {
        private AsyncApiOperation target;

        private AsyncApiOperation Target
        {
            get
            {
                this.target ??= this.Reference.Workspace?.ResolveReference<AsyncApiOperation>(this.Reference);
                return this.target;
            }
        }

        public AsyncApiOperationReference(string reference)
        {
            this.Reference = new AsyncApiReference(reference, ReferenceType.Operation);
        }

        public override AsyncApiAction? Action { get => this.Target?.Action; set => this.Target.Action = value; }

        public override AsyncApiChannelReference Channel { get => this.Target?.Channel; set => this.Target.Channel = value; }

        public override string Title { get => this.Target?.Title; set => this.Target.Title = value; }

        public override string Summary { get => this.Target?.Summary; set => this.Target.Summary = value; }

        public override string Description { get => this.Target?.Description; set => this.Target.Description = value; }

        public override IList<AsyncApiSecurityScheme> Security { get => this.Target?.Security; set => this.Target.Security = value; }

        public override IList<AsyncApiTag> Tags { get => this.Target?.Tags; set => this.Target.Tags = value; }

        public override AsyncApiExternalDocumentation ExternalDocs { get => this.Target?.ExternalDocs; set => this.Target.ExternalDocs = value; }

        public override AsyncApiBindings<IOperationBinding> Bindings { get => this.Target?.Bindings; set => this.Target.Bindings = value; }

        public override IList<AsyncApiOperationTrait> Traits { get => this.Target?.Traits; set => this.Target.Traits = value; }

        public override IList<AsyncApiMessageReference> Messages { get => this.Target?.Messages; set => this.Target.Messages = value; }

        public override AsyncApiOperationReply Reply { get => this.Target?.Reply; set => this.Target.Reply = value; }

        public override IDictionary<string, IAsyncApiExtension> Extensions { get => this.Target?.Extensions; set => this.Target.Extensions = value; }

        public AsyncApiReference Reference { get; set; }

        public bool UnresolvedReference { get { return this.Target == null; } }

        public override void SerializeV2(IAsyncApiWriter writer)
        {
            if (!writer.GetSettings().ShouldInlineReference(this.Reference))
            {
                this.Reference.SerializeV2(writer);
                return;
            }
            else
            {
                this.Reference.Workspace = writer.Workspace;
                this.Target.SerializeV2(writer);
            }
        }

        public override void SerializeV3(IAsyncApiWriter writer)
        {
            if (!writer.GetSettings().ShouldInlineReference(this.Reference))
            {
                this.Reference.SerializeV3(writer);
                return;
            }
            else
            {
                this.Reference.Workspace = writer.Workspace;
                this.Target.SerializeV3(writer);
            }
        }
    }
}
