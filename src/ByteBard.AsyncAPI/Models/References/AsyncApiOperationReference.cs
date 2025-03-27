namespace ByteBard.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    /// <summary>
    /// The definition of an operation trait this application MAY use.
    /// </summary>

    using System.Diagnostics;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Writers;

    [DebuggerDisplay("{Reference}")]
    public class AsyncApiOperationReference : AsyncApiOperation, IAsyncApiReferenceable, IEquatable<AsyncApiOperationReference>, IEquatable<AsyncApiOperation>
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

        public override AsyncApiAction Action { get => this.Target.Action; set => this.Target.Action = value; }

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

        public static bool operator !=(AsyncApiOperationReference left, AsyncApiOperationReference right) => !(left == right);

        public static bool operator ==(AsyncApiOperationReference left, AsyncApiOperationReference right)
        {
            return Equals(left, null) ? Equals(right, null) : left.Equals(right);
        }

        public bool Equals(AsyncApiOperationReference other)
        {
            if (other is null)
            {
                return false;
            }

            if (other.Target is AsyncApiOperationReference reference)
            {
                return this.Equals(reference);
            }

            return this.Target == other.Target;
        }

        public override bool Equals(object obj)
        {
            if (obj is AsyncApiOperationReference reference)
            {
                return this.Equals(reference);
            }

            if (obj is AsyncApiOperation message)
            {
                return this.Equals(message);
            }

            return false;
        }

        public bool Equals(AsyncApiOperation other)
        {
            return this.Target == other;
        }

        public override void SerializeV2(IAsyncApiWriter writer)
        {
            this.Reference.Workspace = writer.Workspace;
            this.Target.SerializeV2(writer);
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
