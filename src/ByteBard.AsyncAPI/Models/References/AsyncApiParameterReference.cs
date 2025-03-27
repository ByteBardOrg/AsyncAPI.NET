namespace ByteBard.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    /// <summary>
    /// The definition of a parameter this application MAY use.
    /// </summary>
    using System.Diagnostics;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Writers;

    [DebuggerDisplay("{Reference}")]
    public class AsyncApiParameterReference : AsyncApiParameter, IAsyncApiReferenceable, IEquatable<AsyncApiParameterReference>, IEquatable<AsyncApiParameter>
    {
        private AsyncApiParameter target;

        private AsyncApiParameter Target
        {
            get
            {
                this.target ??= this.Reference.Workspace?.ResolveReference<AsyncApiParameter>(this.Reference);
                return this.target;
            }
        }

        public AsyncApiParameterReference(string reference)
        {
            this.Reference = new AsyncApiReference(reference, ReferenceType.Parameter);
        }

        public override IList<string> Enum { get => this.Target?.Enum; set => this.Target.Enum = value; }

        public override string Default { get => this.Target?.Default; set => this.Target.Default = value; }

        public override string Description { get => this.Target?.Description; set => this.Target.Description = value; }

        public override IList<string> Examples { get => this.Target?.Examples; set => this.Target.Examples = value; }

        public override string Location { get => this.Target?.Location; set => this.Target.Location = value; }

        public override IDictionary<string, IAsyncApiExtension> Extensions { get => this.Target?.Extensions; set => this.Target.Extensions = value; }

        public AsyncApiReference Reference { get; set; }

        public bool UnresolvedReference { get { return this.Target == null; } }

        public static bool operator !=(AsyncApiParameterReference left, AsyncApiParameterReference right) => !(left == right);

        public static bool operator ==(AsyncApiParameterReference left, AsyncApiParameterReference right)
        {
            return Equals(left, null) ? Equals(right, null) : left.Equals(right);
        }

        public bool Equals(AsyncApiParameterReference other)
        {
            if (other is null)
            {
                return false;
            }

            if (other.Target is AsyncApiParameterReference reference)
            {
                return this.Equals(reference);
            }

            return this.Target == other.Target;
        }

        public override bool Equals(object obj)
        {
            if (obj is AsyncApiParameterReference reference)
            {
                return this.Equals(reference);
            }

            if (obj is AsyncApiParameter message)
            {
                return this.Equals(message);
            }

            return false;
        }

        public bool Equals(AsyncApiParameter other)
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
