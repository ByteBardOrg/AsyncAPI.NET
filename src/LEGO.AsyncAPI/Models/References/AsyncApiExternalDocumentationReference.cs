// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Writers;

    [DebuggerDisplay("{Reference}")]
    public class AsyncApiExternalDocumentationReference : AsyncApiExternalDocumentation, IAsyncApiReferenceable, IEquatable<AsyncApiExternalDocumentationReference>, IEquatable<AsyncApiExternalDocumentation>
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
            this.Reference = new AsyncApiReference(reference, ReferenceType.ExternalDocs);
        }

        public override string Description { get => this.Target?.Description; set => this.Target.Description = value; }

        public override Uri Url { get => this.Target?.Url; set => this.Target.Url = value; }

        public override IDictionary<string, IAsyncApiExtension> Extensions { get => this.Target?.Extensions; set => this.Target.Extensions = value; }

        public AsyncApiReference Reference { get; set; }

        public bool UnresolvedReference { get { return this.Target == null; } }

        public static bool operator !=(AsyncApiExternalDocumentationReference left, AsyncApiExternalDocumentationReference right) => !(left == right);

        public static bool operator ==(AsyncApiExternalDocumentationReference left, AsyncApiExternalDocumentationReference right)
        {
            return Equals(left, null) ? Equals(right, null) : left.Equals(right);
        }

        public bool Equals(AsyncApiExternalDocumentationReference other)
        {
            if (other is null)
            {
                return false;
            }

            if (other.Target is AsyncApiExternalDocumentationReference reference)
            {
                return this.Equals(reference);
            }

            return this.Target == other.Target;
        }

        public override bool Equals(object obj)
        {
            if (obj is AsyncApiExternalDocumentationReference reference)
            {
                return this.Equals(reference);
            }

            if (obj is AsyncApiExternalDocumentation message)
            {
                return this.Equals(message);
            }

            return false;
        }

        public bool Equals(AsyncApiExternalDocumentation other)
        {
            return this.Target == other;
        }

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
