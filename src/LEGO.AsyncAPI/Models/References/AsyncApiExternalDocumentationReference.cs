// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Writers;

    public class AsyncApiExternalDocumentationReference : AsyncApiExternalDocumentation, IAsyncApiReferenceable
    {
        private AsyncApiExternalDocumentation target;

        private AsyncApiExternalDocumentation Target
        {
            get
            {
                this.target ??= this.Reference.Workspace?.ResolveReference<AsyncApiExternalDocumentation>(this.Reference.Reference);
                return this.target;
            }
        }

        public AsyncApiExternalDocumentationReference(string reference)
        {
            this.Reference = new AsyncApiReference(reference, ReferenceType.Tag);
        }

        public override string Description { get => this.Target?.Description; set => this.Target.Description = value; }

        public override Uri Url { get => this.Target?.Url; set => this.Target.Url = value; }

        public override IDictionary<string, IAsyncApiExtension> Extensions { get => this.Target?.Extensions; set => this.Target.Extensions = value; }

        public AsyncApiReference Reference { get; set; }

        public bool UnresolvedReference { get { return this.Target == null; } }

        /// <summary>
        /// Serializes the v2.
        /// </summary>
        /// <remarks>
        /// If <see cref="ReferenceInlineSetting.InlineReferences"/> serialization of the referenced ExternalDocs will be skipped.
        /// </remarks>
        /// <param name="writer">The writer.</param>
        public override void SerializeV2(IAsyncApiWriter writer)
        {
            if (!writer.GetSettings().ShouldInlineReference(this.Reference))
            {
                // We cannot serialize the ExternalDocs as a reference under V2.
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
