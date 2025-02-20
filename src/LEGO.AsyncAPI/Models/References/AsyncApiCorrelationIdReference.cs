// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Models
{
    using System.Collections.Generic;
    /// <summary>
    /// The definition of a correlation ID this application MAY use.
    /// </summary>

    using System.Diagnostics;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Writers;

    [DebuggerDisplay("{Reference}")]
    public class AsyncApiCorrelationIdReference : AsyncApiCorrelationId, IAsyncApiReferenceable
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
