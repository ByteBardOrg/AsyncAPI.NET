// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Writers;

    public class AsyncApiOperationReplyReference : AsyncApiOperationReply, IAsyncApiReferenceable
    {
        private AsyncApiOperationReply target;

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

        public override AsyncApiOperationReplyAddress Address { get => this.Target?.Address; set => this.Target.Address = value; }

        public override AsyncApiChannelReference Channel { get => this.Target?.Channel; set => this.Target.Channel = value; }

        public override IList<AsyncApiMessageReference> Messages { get => this.Target?.Messages; set => this.Target.Messages = value; }

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
