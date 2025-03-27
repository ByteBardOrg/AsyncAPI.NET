// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    /// <summary>
    /// The definition of an operation trait this application MAY use.
    /// </summary>

    using System.Diagnostics;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Writers;

    [DebuggerDisplay("{Reference}")]
    public class AsyncApiOperationTraitReference : AsyncApiOperationTrait, IAsyncApiReferenceable, IEquatable<AsyncApiOperationTraitReference>, IEquatable<AsyncApiOperationTrait>
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

        public static bool operator !=(AsyncApiOperationTraitReference left, AsyncApiOperationTraitReference right) => !(left == right);

        public static bool operator ==(AsyncApiOperationTraitReference left, AsyncApiOperationTraitReference right)
        {
            return Equals(left, null) ? Equals(right, null) : left.Equals(right);
        }

        public bool Equals(AsyncApiOperationTraitReference other)
        {
            if (other is null)
            {
                return false;
            }

            if (other.Target is AsyncApiOperationTraitReference reference)
            {
                return this.Equals(reference);
            }

            return this.Target == other.Target;
        }

        public override bool Equals(object obj)
        {
            if (obj is AsyncApiOperationTraitReference reference)
            {
                return this.Equals(reference);
            }

            if (obj is AsyncApiOperationTrait message)
            {
                return this.Equals(message);
            }

            return false;
        }

        public bool Equals(AsyncApiOperationTrait other)
        {
            return this.Target == other;
        }

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
