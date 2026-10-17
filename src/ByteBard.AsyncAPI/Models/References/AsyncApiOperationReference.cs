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

        private AsyncApiOperation Resolved => this.Target ?? throw this.Reference.CreateUnresolvedException();

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

        public override AsyncApiAction Action { get => this.Resolved.Action; set => this.Resolved.Action = value; }

        public override AsyncApiChannelReference Channel { get => this.Target?.Channel; set => this.Resolved.Channel = value; }

        public override string Title { get => this.Target?.Title; set => this.Resolved.Title = value; }

        public override string Summary { get => this.Target?.Summary; set => this.Resolved.Summary = value; }

        public override string Description { get => this.Target?.Description; set => this.Resolved.Description = value; }

        public override IList<AsyncApiSecurityScheme> Security { get => this.Target?.Security; set => this.Resolved.Security = value; }

        public override IList<AsyncApiTag> Tags { get => this.Target?.Tags; set => this.Resolved.Tags = value; }

        public override AsyncApiExternalDocumentation ExternalDocs { get => this.Target?.ExternalDocs; set => this.Resolved.ExternalDocs = value; }

        public override AsyncApiBindings<IOperationBinding> Bindings { get => this.Target?.Bindings; set => this.Resolved.Bindings = value; }

        public override IList<AsyncApiOperationTrait> Traits { get => this.Target?.Traits; set => this.Resolved.Traits = value; }

        public override IList<AsyncApiMessageReference> Messages { get => this.Target?.Messages; set => this.Resolved.Messages = value; }

        public override AsyncApiOperationReply Reply { get => this.Target?.Reply; set => this.Resolved.Reply = value; }

        public override IDictionary<string, IAsyncApiExtension> Extensions { get => this.Target?.Extensions; set => this.Resolved.Extensions = value; }

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

            if (this.Target is null || other.Target is null)
            {
                return this.Target is null && other.Target is null && this.Reference == other.Reference;
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
            return other is not null && this.Target == other;
        }

        public override int GetHashCode()
        {
            return this.Target?.GetHashCode() ?? this.Reference?.GetHashCode() ?? 0;
        }

        public override void SerializeV2(IAsyncApiWriter writer)
        {
            this.Reference.Workspace = writer.Workspace;
            this.Resolved.SerializeV2(writer);
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
                this.Resolved.SerializeV3(writer);
            }
        }
    }
}
