// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Writers;

    [DebuggerDisplay("{Reference}")]
    public class AsyncApiChannelReference : AsyncApiChannel, IAsyncApiReferenceable, IEquatable<AsyncApiChannel>, IEquatable<AsyncApiChannelReference>
    {
        private AsyncApiChannel target;

        private AsyncApiChannel Target
        {
            get
            {
                this.target ??= this.Reference.Workspace?.ResolveReference<AsyncApiChannel>(this.Reference.Reference);
                return this.target;
            }
        }

        public AsyncApiChannelReference(string reference)
        {
            this.Reference = new AsyncApiReference(reference, ReferenceType.Channel);
        }

        public override string Address { get => this.Target?.Address; set => this.Target.Address = value; }

        public override IDictionary<string, AsyncApiMessage> Messages { get => this.Target?.Messages; set => this.Target.Messages = value; }

        public override string Title { get => this.Target?.Title; set => this.Target.Title = value; }

        public override string Summary { get => this.Target?.Summary; set => this.Target.Summary = value; }

        public override string Description { get => this.Target?.Description; set => this.Target.Description = value; }

        public override IList<AsyncApiServerReference> Servers { get => this.Target?.Servers; set => this.Target.Servers = value; }

        public override IDictionary<string, AsyncApiParameter> Parameters { get => this.Target?.Parameters; set => this.Target.Parameters = value; }

        public override IList<AsyncApiTag> Tags { get => this.Target?.Tags; set => this.Target.Tags = value; }

        public override AsyncApiExternalDocumentation ExternalDocs { get => this.Target?.ExternalDocs; set => this.Target.ExternalDocs = value; }

        public override AsyncApiBindings<IChannelBinding> Bindings { get => this.Target?.Bindings; set => this.Target.Bindings = value; }

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

        public static bool operator !=(AsyncApiChannelReference left, AsyncApiChannelReference right) => !(left == right);

        public static bool operator ==(AsyncApiChannelReference left, AsyncApiChannelReference right)
        {
            return Equals(left, null) ? Equals(right, null) : left.Equals(right);
        }

        public bool Equals(AsyncApiChannelReference other)
        {
            if (other == null)
            {
                return false;
            }
            return this.Target == other.Target;
        }

        public override bool Equals(object obj)
        {
            if (obj is AsyncApiChannelReference reference)
            {
                return this.Equals(reference);
            }

            if (obj is AsyncApiChannel channel)
            {
                return this.Equals(channel);
            }

            return false;
        }

        public bool Equals(AsyncApiChannel other)
        {
            return this.Target == other;
        }
    }
}
