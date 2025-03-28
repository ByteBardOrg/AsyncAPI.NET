namespace ByteBard.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    /// <summary>
    /// The definition of a correlation ID this application MAY use.
    /// </summary>

    using System.Diagnostics;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Writers;

    [DebuggerDisplay("{Reference}")]
    public class AsyncApiCorrelationIdReference : AsyncApiCorrelationId, IAsyncApiReferenceable, IEquatable<AsyncApiCorrelationIdReference>, IEquatable<AsyncApiCorrelationId>
    {
        private AsyncApiCorrelationId target;

        private AsyncApiCorrelationId Target
        {
            get
            {
                this.target ??= this.Reference.Workspace?.ResolveReference<AsyncApiCorrelationId>(this.Reference.Reference);
                return this.target;
            }
        }

        public AsyncApiCorrelationIdReference(string reference)
        {
            this.Reference = new AsyncApiReference(reference, ReferenceType.CorrelationId);
        }

        public override string Description { get => this.Target?.Description; set => this.Target.Description = value; }

        public override string Location { get => this.Target?.Location; set => this.Target.Location = value; }

        public override IDictionary<string, IAsyncApiExtension> Extensions { get => this.Target?.Extensions; set => this.Target.Extensions = value; }

        public AsyncApiReference Reference { get; set; }

        public bool UnresolvedReference { get { return this.Target == null; } }

        public static bool operator !=(AsyncApiCorrelationIdReference left, AsyncApiCorrelationIdReference right) => !(left == right);

        public static bool operator ==(AsyncApiCorrelationIdReference left, AsyncApiCorrelationIdReference right)
        {
            return Equals(left, null) ? Equals(right, null) : left.Equals(right);
        }

        public bool Equals(AsyncApiCorrelationIdReference other)
        {
            if (other is null)
            {
                return false;
            }

            if (other.Target is AsyncApiCorrelationIdReference reference)
            {
                return this.Equals(reference);
            }

            return this.Target == other.Target;
        }

        public override bool Equals(object obj)
        {
            if (obj is AsyncApiCorrelationIdReference reference)
            {
                return this.Equals(reference);
            }

            if (obj is AsyncApiCorrelationId message)
            {
                return this.Equals(message);
            }

            return false;
        }

        public bool Equals(AsyncApiCorrelationId other)
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
