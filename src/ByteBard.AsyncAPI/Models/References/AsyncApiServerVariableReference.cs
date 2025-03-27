namespace ByteBard.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    /// <summary>
    /// The definition of a server variable this application MAY use.
    /// </summary>
    using System.Diagnostics;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Writers;

    [DebuggerDisplay("{Reference}")]
    public class AsyncApiServerVariableReference : AsyncApiServerVariable, IAsyncApiReferenceable, IEquatable<AsyncApiServerVariableReference>, IEquatable<AsyncApiServerVariable>
    {
        private AsyncApiServerVariable target;

        private AsyncApiServerVariable Target
        {
            get
            {
                this.target ??= this.Reference.Workspace?.ResolveReference<AsyncApiServerVariable>(this.Reference);
                return this.target;
            }
        }

        public AsyncApiServerVariableReference(string reference)
        {
            this.Reference = new AsyncApiReference(reference, ReferenceType.ServerVariable);
        }

        public override IList<string> Enum { get => this.Target?.Enum; set => this.Target.Enum = value; }

        public override string Default { get => this.Target?.Default; set => this.Target.Default = value; }

        public override string Description { get => this.Target?.Description; set => this.Target.Description = value; }

        public override IList<string> Examples { get => this.Target?.Examples; set => this.Target.Examples = value; }

        public override IDictionary<string, IAsyncApiExtension> Extensions { get => this.Target?.Extensions; set => this.Target.Extensions = value; }

        public AsyncApiReference Reference { get; set; }

        public bool UnresolvedReference { get { return this.Target == null; } }

        public static bool operator !=(AsyncApiServerVariableReference left, AsyncApiServerVariableReference right) => !(left == right);

        public static bool operator ==(AsyncApiServerVariableReference left, AsyncApiServerVariableReference right)
        {
            return Equals(left, null) ? Equals(right, null) : left.Equals(right);
        }

        public bool Equals(AsyncApiServerVariableReference other)
        {
            if (other is null)
            {
                return false;
            }

            if (other.Target is AsyncApiServerVariableReference reference)
            {
                return this.Equals(reference);
            }

            return this.Target == other.Target;
        }

        public override bool Equals(object obj)
        {
            if (obj is AsyncApiServerVariableReference reference)
            {
                return this.Equals(reference);
            }

            if (obj is AsyncApiServerVariable message)
            {
                return this.Equals(message);
            }

            return false;
        }

        public bool Equals(AsyncApiServerVariable other)
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
