// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Models
{
    using System.Collections.Generic;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Writers;

    /// <summary>
    /// The definition of a MultiFormatSchema this application MAY use.
    /// </summary>
    public class AsyncApiMultiFormatSchemaReference : AsyncApiMultiFormatSchema, IAsyncApiReferenceable
    {
        private AsyncApiMultiFormatSchema target;

        private AsyncApiMultiFormatSchema Target
        {
            get
            {
                this.target ??= this.Reference.Workspace?.ResolveReference<AsyncApiMultiFormatSchema>(this.Reference.Reference);
                return this.target;
            }
        }

        public AsyncApiMultiFormatSchemaReference(string reference)
        {
            this.Reference = new AsyncApiReference(reference, ReferenceType.MultiFormatSchema);
        }

        public override string SchemaFormat { get => this.Target?.SchemaFormat; set => this.Target.SchemaFormat = value; }

        public override IAsyncApiSchema Schema { get => this.Target?.Schema; set => this.Target.Schema = value; }

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
