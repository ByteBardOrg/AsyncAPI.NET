namespace ByteBard.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Writers;

    /// <summary>
    /// The definition of a message this application MAY use.
    /// </summary>
    [DebuggerDisplay("{Reference}")]
    public class AsyncApiMessageReference : AsyncApiMessage, IAsyncApiReferenceable, IEquatable<AsyncApiMessageReference>, IEquatable<AsyncApiMessage>
    {
        private AsyncApiMessage target;

        private AsyncApiMessage Resolved => this.Target ?? throw this.Reference.CreateUnresolvedException();

        private AsyncApiMessage Target
        {
            get
            {
                this.target ??= this.Reference.Workspace?.ResolveReference<AsyncApiMessage>(this.Reference.Reference);
                return this.target;
            }
        }

        public AsyncApiMessageReference(string reference)
        {
            this.Reference = new AsyncApiReference(reference, ReferenceType.Message);
        }

        public override AsyncApiMultiFormatSchema Headers { get => this.Target?.Headers; set => this.Resolved.Headers = value; }

        public override AsyncApiMultiFormatSchema Payload { get => this.Target?.Payload; set => this.Resolved.Payload = value; }

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

        public override IList<AsyncApiMessageTrait> Traits { get => this.Target?.Traits; set => this.Resolved.Traits = value; }

        public override IDictionary<string, IAsyncApiExtension> Extensions { get => this.Target?.Extensions; set => this.Resolved.Extensions = value; }

        public AsyncApiReference Reference { get; set; }

        public bool UnresolvedReference { get { return this.Target == null; } }

        public static bool operator !=(AsyncApiMessageReference left, AsyncApiMessageReference right) => !(left == right);

        public static bool operator ==(AsyncApiMessageReference left, AsyncApiMessageReference right)
        {
            return Equals(left, null) ? Equals(right, null) : left.Equals(right);
        }

        public bool Equals(AsyncApiMessageReference other)
        {
            if (other is null)
            {
                return false;
            }

            if (other.Target is AsyncApiMessageReference reference)
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
            if (obj is AsyncApiMessageReference reference)
            {
                return this.Equals(reference);
            }

            if (obj is AsyncApiMessage message)
            {
                return this.Equals(message);
            }

            return false;
        }

        public bool Equals(AsyncApiMessage other)
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
                // If messages are from a V2 component, and is proxied through channels its a reference pointing to a reference.
                if (this.Target is AsyncApiMessageReference reference)
                {
                    reference.SerializeV2(writer);
                    return;
                }

                if (this.Reference.Reference.StartsWith("#/channels"))
                {
                    // Try force inline anyway.
                    this.Reference.Workspace = writer.Workspace;
                    this.Target?.SerializeV2(writer);
                    return;
                }

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
