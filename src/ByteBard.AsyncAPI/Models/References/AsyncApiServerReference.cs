namespace ByteBard.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    /// <summary>
    /// The definition of a server this application MAY connect to.
    /// </summary>
    using System.Diagnostics;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Writers;

    [DebuggerDisplay("{Reference}")]
    public class AsyncApiServerReference : AsyncApiServer, IAsyncApiReferenceable, IEquatable<AsyncApiServerReference>, IEquatable<AsyncApiServer>
    {
        private AsyncApiServer target;

        private AsyncApiServer Resolved => this.Target ?? throw this.Reference.CreateUnresolvedException();

        private AsyncApiServer Target
        {
            get
            {
                this.target ??= this.Reference.Workspace?.ResolveReference<AsyncApiServer>(this.Reference);
                return this.target;
            }
        }

        public AsyncApiServerReference(string reference)
        {
            this.Reference = new AsyncApiReference(reference, ReferenceType.Server);
        }

        public override string Host { get => this.Target?.Host; set => this.Resolved.Host = value; }

        public override string PathName { get => this.Target?.PathName; set => this.Resolved.PathName = value; }

        public override string Protocol { get => this.Target?.Protocol; set => this.Resolved.Protocol = value; }

        public override string ProtocolVersion { get => this.Target?.ProtocolVersion; set => this.Resolved.ProtocolVersion = value; }

        public override string Description { get => this.Target?.Description; set => this.Resolved.Description = value; }

        public override string Title { get => this.Target?.Title; set => this.Resolved.Title = value; }

        public override string Summary { get => this.Target?.Summary; set => this.Resolved.Summary = value; }

        public override IDictionary<string, AsyncApiServerVariable> Variables { get => this.Target?.Variables; set => this.Resolved.Variables = value; }

        public override IList<AsyncApiSecurityScheme> Security { get => this.Target?.Security; set => this.Resolved.Security = value; }

        public override IList<AsyncApiTag> Tags { get => this.Target?.Tags; set => this.Resolved.Tags = value; }

        public override AsyncApiBindings<IServerBinding> Bindings { get => this.Target?.Bindings; set => this.Resolved.Bindings = value; }

        public override IDictionary<string, IAsyncApiExtension> Extensions { get => this.Target?.Extensions; set => this.Resolved.Extensions = value; }

        public AsyncApiReference Reference { get; set; }

        public bool UnresolvedReference { get { return this.Target == null; } }

        public static bool operator !=(AsyncApiServerReference left, AsyncApiServerReference right) => !(left == right);

        public static bool operator ==(AsyncApiServerReference left, AsyncApiServerReference right)
        {
            return Equals(left, null) ? Equals(right, null) : left.Equals(right);
        }

        public bool Equals(AsyncApiServerReference other)
        {
            if (other is null)
            {
                return false;
            }

            if (other.Target is AsyncApiServerReference reference)
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
            if (obj is AsyncApiServerReference reference)
            {
                return this.Equals(reference);
            }

            if (obj is AsyncApiServer message)
            {
                return this.Equals(message);
            }

            return false;
        }

        public bool Equals(AsyncApiServer other)
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