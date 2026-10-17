namespace ByteBard.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Writers;

    [DebuggerDisplay("{Reference}")]
    public class AsyncApiChannelReference : AsyncApiChannel, IAsyncApiReferenceable, IEquatable<AsyncApiChannel>, IEquatable<AsyncApiChannelReference>
    {
        private AsyncApiChannel target;

        private AsyncApiChannel Resolved => this.Target ?? throw this.Reference.CreateUnresolvedException();

        private AsyncApiChannel Target
        {
            get
            {
                this.target ??= this.Reference.Workspace?.ResolveReference<AsyncApiChannel>(this.Reference.Reference);
                return this.target;
            }
        }

        public AsyncApiChannelReference(string reference)
        {
            this.Reference = new AsyncApiReference(reference, ReferenceType.Channel);
        }

        public override string Address { get => this.Target?.Address; set => this.Resolved.Address = value; }

        public override IDictionary<string, AsyncApiMessage> Messages { get => this.Target?.Messages; set => this.Resolved.Messages = value; }

        public override string Title { get => this.Target?.Title; set => this.Resolved.Title = value; }

        public override string Summary { get => this.Target?.Summary; set => this.Resolved.Summary = value; }

        public override string Description { get => this.Target?.Description; set => this.Resolved.Description = value; }

        public override IList<AsyncApiServerReference> Servers { get => this.Target?.Servers; set => this.Resolved.Servers = value; }

        public override IDictionary<string, AsyncApiParameter> Parameters { get => this.Target?.Parameters; set => this.Resolved.Parameters = value; }

        public override IList<AsyncApiTag> Tags { get => this.Target?.Tags; set => this.Resolved.Tags = value; }

        public override AsyncApiExternalDocumentation ExternalDocs { get => this.Target?.ExternalDocs; set => this.Resolved.ExternalDocs = value; }

        public override AsyncApiBindings<IChannelBinding> Bindings { get => this.Target?.Bindings; set => this.Resolved.Bindings = value; }

        public override IDictionary<string, IAsyncApiExtension> Extensions { get => this.Target?.Extensions; set => this.Resolved.Extensions = value; }

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

        public static bool operator !=(AsyncApiChannelReference left, AsyncApiChannelReference right) => !(left == right);

        public static bool operator ==(AsyncApiChannelReference left, AsyncApiChannelReference right)
        {
            return Equals(left, null) ? Equals(right, null) : left.Equals(right);
        }

        public bool Equals(AsyncApiChannelReference other)
        {
            if (other == null)
            {
                return false;
            }

            if (other.Target is AsyncApiChannelReference reference)
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
            if (obj is AsyncApiChannelReference reference)
            {
                return this.Equals(reference);
            }

            if (obj is AsyncApiChannel channel)
            {
                return this.Equals(channel);
            }

            return false;
        }

        public bool Equals(AsyncApiChannel other)
        {
            return other is not null && this.Target == other;
        }

        public override int GetHashCode()
        {
            return this.Target?.GetHashCode() ?? this.Reference?.GetHashCode() ?? 0;
        }
    }
}
