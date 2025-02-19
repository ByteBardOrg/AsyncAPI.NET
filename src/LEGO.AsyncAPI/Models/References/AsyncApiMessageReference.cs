// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Writers;

    /// <summary>
    /// The definition of a message this application MAY use.
    /// </summary>
    public class AsyncApiMessageReference : AsyncApiMessage, IAsyncApiReferenceable, IEquatable<AsyncApiMessageReference>, IEquatable<AsyncApiMessage>
    {
        private AsyncApiMessage target;

        private AsyncApiMessage Target
        {
            get
            {
                this.target ??= this.Reference.Workspace?.ResolveReference<AsyncApiMessage>(this.Reference.Reference);
                return this.target;
            }
        }

        public AsyncApiMessageReference(string reference)
        {
            this.Reference = new AsyncApiReference(reference, ReferenceType.Message);
        }

        public override AsyncApiMultiFormatSchema Headers { get => this.Target?.Headers; set => this.Target.Headers = value; }

        public override AsyncApiMultiFormatSchema Payload { get => this.Target?.Payload; set => this.Target.Payload = value; }

        public override AsyncApiCorrelationId CorrelationId { get => this.Target?.CorrelationId; set => this.Target.CorrelationId = value; }

        public override string ContentType { get => this.Target?.ContentType; set => this.Target.ContentType = value; }

        public override string Name { get => this.Target?.Name; set => this.Target.Name = value; }

        public override string Title { get => this.Target?.Title; set => this.Target.Title = value; }

        public override string Summary { get => this.Target?.Summary; set => this.Target.Summary = value; }

        public override string Description { get => this.Target?.Description; set => this.Target.Description = value; }

        public override IList<AsyncApiTag> Tags { get => this.Target?.Tags; set => this.Target.Tags = value; }

        public override AsyncApiExternalDocumentation ExternalDocs { get => this.Target?.ExternalDocs; set => this.Target.ExternalDocs = value; }

        public override AsyncApiBindings<IMessageBinding> Bindings { get => this.Target?.Bindings; set => this.Target.Bindings = value; }

        public override IList<AsyncApiMessageExample> Examples { get => this.Target?.Examples; set => this.Target.Examples = value; }

        public override IList<AsyncApiMessageTrait> Traits { get => this.Target?.Traits; set => this.Target.Traits = value; }

        public override IDictionary<string, IAsyncApiExtension> Extensions { get => this.Target?.Extensions; set => this.Target.Extensions = value; }

        public AsyncApiReference Reference { get; set; }

        public bool UnresolvedReference { get { return this.Target == null; } }

        public static bool operator !=(AsyncApiMessageReference left, AsyncApiMessageReference right) => !(left == right);

        public static bool operator ==(AsyncApiMessageReference left, AsyncApiMessageReference right)
        {
            return Equals(left, null) ? Equals(right, null) : left.Equals(right);
        }

        public bool Equals(AsyncApiMessageReference other)
        {
            return this.Target == other.Target;
        }

        public override bool Equals(object obj)
        {
            if (obj is AsyncApiMessageReference reference)
            {
                return this.Equals(reference);
            }

            if (obj is AsyncApiMessage message)
            {
                return this.Equals(message);
            }

            return false;
        }

        public bool Equals(AsyncApiMessage other)
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
