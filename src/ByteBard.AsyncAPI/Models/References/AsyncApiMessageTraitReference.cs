namespace ByteBard.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    /// <summary>
    /// The definition of a message trait this application MAY use.
    /// </summary>

    using System.Diagnostics;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Writers;

    [DebuggerDisplay("{Reference}")]
    public class AsyncApiMessageTraitReference : AsyncApiMessageTrait, IAsyncApiReferenceable, IEquatable<AsyncApiMessageTraitReference>, IEquatable<AsyncApiMessageTrait>
    {
        private AsyncApiMessageTrait target;

        private AsyncApiMessageTrait Resolved => this.Target ?? throw this.Reference.CreateUnresolvedException();

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

        public override AsyncApiMultiFormatSchema Headers { get => this.Target?.Headers; set => this.Resolved.Headers = value; }

        public override AsyncApiCorrelationId CorrelationId { get => this.Target?.CorrelationId; set => this.Resolved.CorrelationId = value; }

        public override string ContentType { get => this.Target?.ContentType; set => this.Resolved.ContentType = value; }

        public override string Name { get => this.Target?.Name; set => this.Resolved.Name = value; }

        public override string Title { get => this.Target?.Title; set => this.Resolved.Title = value; }

        public override string Summary { get => this.Target?.Summary; set => this.Resolved.Summary = value; }

        public override string Description { get => this.Target?.Description; set => this.Resolved.Description = value; }

        public override IList<AsyncApiTag> Tags { get => this.Target?.Tags; set => this.Resolved.Tags = value; }

        public override AsyncApiExternalDocumentation ExternalDocs { get => this.Target?.ExternalDocs; set => this.Resolved.ExternalDocs = value; }

        public override AsyncApiBindings<IMessageBinding> Bindings { get => this.Target?.Bindings; set => this.Resolved.Bindings = value; }

        public override IList<AsyncApiMessageExample> Examples { get => this.Target?.Examples; set => this.Resolved.Examples = value; }

        public override IDictionary<string, IAsyncApiExtension> Extensions { get => this.Target?.Extensions; set => this.Resolved.Extensions = value; }

        public AsyncApiReference Reference { get; set; }

        public bool UnresolvedReference { get { return this.Target == null; } }

        public static bool operator !=(AsyncApiMessageTraitReference left, AsyncApiMessageTraitReference right) => !(left == right);

        public static bool operator ==(AsyncApiMessageTraitReference left, AsyncApiMessageTraitReference right)
        {
            return Equals(left, null) ? Equals(right, null) : left.Equals(right);
        }

        public bool Equals(AsyncApiMessageTraitReference other)
        {
            if (other is null)
            {
                return false;
            }

            if (other.Target is AsyncApiMessageTraitReference reference)
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
            if (obj is AsyncApiMessageTraitReference reference)
            {
                return this.Equals(reference);
            }

            if (obj is AsyncApiMessageTrait message)
            {
                return this.Equals(message);
            }

            return false;
        }

        public bool Equals(AsyncApiMessageTrait other)
        {
            return other is not null && this.Target == other;
        }

        public override int GetHashCode()
        {
            return this.Target?.GetHashCode() ?? this.Reference?.GetHashCode() ?? 0;
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
                this.Resolved.SerializeV2(writer);
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
                this.Resolved.SerializeV3(writer);
            }
        }
    }
}
