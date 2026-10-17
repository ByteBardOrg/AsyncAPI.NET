namespace ByteBard.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Writers;

    [DebuggerDisplay("{Reference}")]
    public class AsyncApiOperationReplyReference : AsyncApiOperationReply, IAsyncApiReferenceable, IEquatable<AsyncApiOperationReplyReference>, IEquatable<AsyncApiOperationReply>
    {
        private AsyncApiOperationReply target;

        private AsyncApiOperationReply Resolved => this.Target ?? throw this.Reference.CreateUnresolvedException();

        private AsyncApiOperationReply Target
        {
            get
            {
                this.target ??= this.Reference.Workspace?.ResolveReference<AsyncApiOperationReply>(this.Reference.Reference);
                return this.target;
            }
        }

        public AsyncApiOperationReplyReference(string reference)
        {
            this.Reference = new AsyncApiReference(reference, ReferenceType.OperationReply);
        }

        public override AsyncApiOperationReplyAddress Address { get => this.Target?.Address; set => this.Resolved.Address = value; }

        public override AsyncApiChannelReference Channel { get => this.Target?.Channel; set => this.Resolved.Channel = value; }

        public override IList<AsyncApiMessageReference> Messages { get => this.Target?.Messages; set => this.Resolved.Messages = value; }

        public override IDictionary<string, IAsyncApiExtension> Extensions { get => this.Target?.Extensions; set => this.Resolved.Extensions = value; }

        public AsyncApiReference Reference { get; set; }

        public bool UnresolvedReference { get { return this.Target == null; } }

        public static bool operator !=(AsyncApiOperationReplyReference left, AsyncApiOperationReplyReference right) => !(left == right);

        public static bool operator ==(AsyncApiOperationReplyReference left, AsyncApiOperationReplyReference right)
        {
            return Equals(left, null) ? Equals(right, null) : left.Equals(right);
        }

        public bool Equals(AsyncApiOperationReplyReference other)
        {
            if (other is null)
            {
                return false;
            }

            if (other.Target is AsyncApiOperationReplyReference reference)
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
            if (obj is AsyncApiOperationReplyReference reference)
            {
                return this.Equals(reference);
            }

            if (obj is AsyncApiOperationReply message)
            {
                return this.Equals(message);
            }

            return false;
        }

        public bool Equals(AsyncApiOperationReply other)
        {
            return other is not null && this.Target == other;
        }

        public override int GetHashCode()
        {
            return this.Target?.GetHashCode() ?? this.Reference?.GetHashCode() ?? 0;
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
