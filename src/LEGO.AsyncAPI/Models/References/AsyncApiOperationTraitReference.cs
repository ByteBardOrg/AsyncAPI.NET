// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Models
{
    using System.Collections.Generic;
    /// <summary>
    /// The definition of an operation trait this application MAY use.
    /// </summary>

    using System.Diagnostics;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Writers;

    [DebuggerDisplay("{Reference}")]
    public class AsyncApiOperationTraitReference : AsyncApiOperationTrait, IAsyncApiReferenceable
    {
        private AsyncApiOperationTrait target;

        private AsyncApiOperationTrait Target
        {
            get
            {
                this.target ??= this.Reference.Workspace?.ResolveReference<AsyncApiOperationTrait>(this.Reference);
                return this.target;
            }
        }

        public AsyncApiOperationTraitReference(string reference)
        {
            this.Reference = new AsyncApiReference(reference, ReferenceType.OperationTrait);
        }

        public override string Title { get => this.Target?.Title; set => this.Target.Title = value; }

        public override string Summary { get => this.Target?.Summary; set => this.Target.Summary = value; }

        public override string Description { get => this.Target?.Description; set => this.Target.Description = value; }

        public override IList<AsyncApiSecurityScheme> Security { get => this.Target?.Security; set => this.Target.Security = value; }

        public override IList<AsyncApiTag> Tags { get => this.Target?.Tags; set => this.Target.Tags = value; }

        public override AsyncApiExternalDocumentation ExternalDocs { get => this.Target?.ExternalDocs; set => this.Target.ExternalDocs = value; }

        public override AsyncApiBindings<IOperationBinding> Bindings { get => this.Target?.Bindings; set => this.Target.Bindings = value; }

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
    }
}
