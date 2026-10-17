namespace ByteBard.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Writers;

    [DebuggerDisplay("{Reference}")]
    public class AsyncApiTagReference : AsyncApiTag, IAsyncApiReferenceable, IEquatable<AsyncApiTagReference>, IEquatable<AsyncApiTag>
    {
        private AsyncApiTag target;

        private AsyncApiTag Resolved => this.Target ?? throw this.Reference.CreateUnresolvedException();

        private AsyncApiTag Target
        {
            get
            {
                this.target ??= this.Reference.Workspace?.ResolveReference<AsyncApiTag>(this.Reference.Reference);
                return this.target;
            }
        }

        public AsyncApiTagReference(string reference)
        {
            this.Reference = new AsyncApiReference(reference, ReferenceType.Tag);
        }

        public override string Name { get => this.Target?.Name; set => this.Resolved.Name = value; }

        public override string Description { get => this.Target?.Description; set => this.Resolved.Description = value; }

        public override AsyncApiExternalDocumentation ExternalDocs { get => this.Target?.ExternalDocs; set => this.Resolved.ExternalDocs = value; }

        public override IDictionary<string, IAsyncApiExtension> Extensions { get => this.Target?.Extensions; set => this.Resolved.Extensions = value; }

        public AsyncApiReference Reference { get; set; }

        public bool UnresolvedReference { get { return this.Target == null; } }

        public static bool operator !=(AsyncApiTagReference left, AsyncApiTagReference right) => !(left == right);

        public static bool operator ==(AsyncApiTagReference left, AsyncApiTagReference right)
        {
            return Equals(left, null) ? Equals(right, null) : left.Equals(right);
        }

        public bool Equals(AsyncApiTagReference other)
        {
            if (other is null)
            {
                return false;
            }

            if (other.Target is AsyncApiTagReference reference)
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
            if (obj is AsyncApiTagReference reference)
            {
                return this.Equals(reference);
            }

            if (obj is AsyncApiTag message)
            {
                return this.Equals(message);
            }

            return false;
        }

        public bool Equals(AsyncApiTag other)
        {
            return other is not null && this.Target == other;
        }

        public override int GetHashCode()
        {
            return this.Target?.GetHashCode() ?? this.Reference?.GetHashCode() ?? 0;
        }

        /// <summary>
        /// Serializes the v2.
        /// </summary>
        /// <remarks>
        /// If <see cref="ReferenceInlineSetting.InlineReferences"/> serialization of the referenced tag will be skipped.
        /// </remarks>
        /// <param name="writer">The writer.</param>
        public override void SerializeV2(IAsyncApiWriter writer)
        {
            if (!writer.GetSettings().ShouldInlineReference(this.Reference))
            {
                // We cannot serialize the Tag as a reference under V2.
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
