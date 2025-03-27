namespace ByteBard.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Writers;

    [DebuggerDisplay("{Reference}")]
    public class AsyncApiOperationReplyAddressReference : AsyncApiOperationReplyAddress, IAsyncApiReferenceable, IEquatable<AsyncApiOperationReplyAddressReference>, IEquatable<AsyncApiOperationReplyAddress>
    {
        private AsyncApiOperationReplyAddress target;

        private AsyncApiOperationReplyAddress Target
        {
            get
            {
                this.target ??= this.Reference.Workspace?.ResolveReference<AsyncApiOperationReplyAddress>(this.Reference.Reference);
                return this.target;
            }
        }

        public AsyncApiOperationReplyAddressReference(string reference)
        {
            this.Reference = new AsyncApiReference(reference, ReferenceType.OperationReplyAddress);
        }

        public override string Description { get => this.Target?.Description; set => this.Target.Description = value; }

        public override string Location { get => this.Target?.Location; set => this.Target.Location = value; }

        public override IDictionary<string, IAsyncApiExtension> Extensions { get => this.Target?.Extensions; set => this.Target.Extensions = value; }

        public AsyncApiReference Reference { get; set; }

        public bool UnresolvedReference { get { return this.Target == null; } }

        public static bool operator !=(AsyncApiOperationReplyAddressReference left, AsyncApiOperationReplyAddressReference right) => !(left == right);

        public static bool operator ==(AsyncApiOperationReplyAddressReference left, AsyncApiOperationReplyAddressReference right)
        {
            return Equals(left, null) ? Equals(right, null) : left.Equals(right);
        }

        public bool Equals(AsyncApiOperationReplyAddressReference other)
        {
            if (other is null)
            {
                return false;
            }

            if (other.Target is AsyncApiOperationReplyAddressReference reference)
            {
                return this.Equals(reference);
            }

            return this.Target == other.Target;
        }

        public override bool Equals(object obj)
        {
            if (obj is AsyncApiOperationReplyAddressReference reference)
            {
                return this.Equals(reference);
            }

            if (obj is AsyncApiOperationReplyAddress message)
            {
                return this.Equals(message);
            }

            return false;
        }

        public bool Equals(AsyncApiOperationReplyAddress other)
        {
            return this.Target == other;
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
