// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Models
{
    using System.Collections.Generic;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Writers;

    /// <summary>
    /// The definition of a message trait this application MAY use.
    /// </summary>
    public class AsyncApiMessageTraitReference : AsyncApiMessageTrait, IAsyncApiReferenceable
    {
        private AsyncApiMessageTrait target;

        private AsyncApiMessageTrait Target
        {
            get
            {
                this.target ??= this.Reference.Workspace?.ResolveReference<AsyncApiMessageTrait>(this.Reference);
                return this.target;
            }
        }

        public AsyncApiMessageTraitReference(string reference)
        {
            this.Reference = new AsyncApiReference(reference, ReferenceType.MessageTrait);
        }

        public override AsyncApiMultiFormatSchema Headers { get => this.Target?.Headers; set => this.Target.Headers = value; }

        public override AsyncApiCorrelationId CorrelationId { get => this.Target?.CorrelationId; set => this.Target.CorrelationId = value; }

        public override string ContentType { get => this.Target?.ContentType; set => this.Target.ContentType = value; }

        public override string Name { get => this.Target?.Name; set => this.Target.Name = value; }

        public override string Title { get => this.Target?.Title; set => this.Target.Title = value; }

        public override string Summary { get => this.Target?.Summary; set => this.Target.Summary = value; }

        public override string Description { get => this.Target?.Description; set => this.Target.Description = value; }

        public override IList<AsyncApiTag> Tags { get => this.Target?.Tags; set => this.Target.Tags = value; }

        public override AsyncApiExternalDocumentation ExternalDocs { get => this.Target?.ExternalDocs; set => this.Target.ExternalDocs = value; }

        public override AsyncApiBindings<IMessageBinding> Bindings { get => this.Target?.Bindings; set => this.Target.Bindings = value; }

        public override IList<AsyncApiMessageExample> Examples { get => this.Target?.Examples; set => this.Target.Examples = value; }

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
