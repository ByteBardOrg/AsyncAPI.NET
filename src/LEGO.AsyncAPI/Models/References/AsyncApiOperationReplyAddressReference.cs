// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Models
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Writers;

    [DebuggerDisplay("{Reference}")]
    public class AsyncApiOperationReplyAddressReference : AsyncApiOperationReplyAddress, IAsyncApiReferenceable
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
