namespace ByteBard.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Writers;

    [DebuggerDisplay("{Reference}")]
    public class AsyncApiJsonSchemaReference : AsyncApiJsonSchema, IAsyncApiReferenceable, IEquatable<AsyncApiJsonSchemaReference>, IEquatable<AsyncApiJsonSchema>
    {
        private AsyncApiJsonSchema target;

        private AsyncApiJsonSchema Resolved => this.Target ?? throw this.Reference.CreateUnresolvedException();

        private AsyncApiJsonSchema Target
        {
            get
            {
                this.target ??= this.Reference.Workspace?.ResolveReference<AsyncApiJsonSchema>(this.Reference) ?? this.Reference.Workspace?.ResolveReference<AsyncApiMultiFormatSchema>(this.Reference)?.Schema?.As<AsyncApiJsonSchema>();
                return this.target;
            }
        }

        public AsyncApiJsonSchemaReference(string reference)
        {
            this.Reference = new AsyncApiReference(reference, ReferenceType.Schema);
        }

        public AsyncApiReference Reference { get; set; }

        public bool UnresolvedReference
        {
            get { return this.Target == null; }
        }

        public override string Title
        {
            get => this.Target?.Title;
            set => this.Resolved.Title = value;
        }

        public override SchemaType? Type
        {
            get => this.Target?.Type;
            set => this.Resolved.Type = value;
        }

        public override string Format
        {
            get => this.Target?.Format;
            set => this.Resolved.Format = value;
        }

        public override string Description
        {
            get => this.Target?.Description;
            set => this.Resolved.Description = value;
        }

        public override double? Maximum
        {
            get => this.Target?.Maximum;
            set => this.Resolved.Maximum = value;
        }

        public override double? ExclusiveMaximum
        {
            get => this.Target?.ExclusiveMaximum;
            set => this.Resolved.ExclusiveMaximum = value;
        }

        public override double? Minimum
        {
            get => this.Target?.Minimum;
            set => this.Resolved.Minimum = value;
        }

        public override double? ExclusiveMinimum
        {
            get => this.Target?.ExclusiveMinimum;
            set => this.Resolved.ExclusiveMinimum = value;
        }

        public override int? MaxLength
        {
            get => this.Target?.MaxLength;
            set => this.Resolved.MaxLength = value;
        }

        public override int? MinLength
        {
            get => this.Target?.MinLength;
            set => this.Resolved.MinLength = value;
        }

        public override string Pattern
        {
            get => this.Target?.Pattern;
            set => this.Resolved.Pattern = value;
        }

        public override double? MultipleOf
        {
            get => this.Target?.MultipleOf;
            set => this.Resolved.MultipleOf = value;
        }

        public override AsyncApiAny Default
        {
            get => this.Target?.Default;
            set => this.Resolved.Default = value;
        }

        public override bool ReadOnly
        {
            get => this.Target?.ReadOnly ?? false;
            set => this.Resolved.ReadOnly = value;
        }

        public override bool WriteOnly
        {
            get => this.Target?.WriteOnly ?? false;
            set => this.Resolved.WriteOnly = value;
        }

        public override IList<AsyncApiJsonSchema> AllOf
        {
            get => this.Target?.AllOf;
            set => this.Resolved.AllOf = value;
        }

        public override IList<AsyncApiJsonSchema> OneOf
        {
            get => this.Target?.OneOf;
            set => this.Resolved.OneOf = value;
        }

        public override IList<AsyncApiJsonSchema> AnyOf
        {
            get => this.Target?.AnyOf;
            set => this.Resolved.AnyOf = value;
        }

        public override AsyncApiJsonSchema Not
        {
            get => this.Target?.Not;
            set => this.Resolved.Not = value;
        }

        public override AsyncApiJsonSchema Contains
        {
            get => this.Target?.Contains;
            set => this.Resolved.Contains = value;
        }

        public override AsyncApiJsonSchema If
        {
            get => this.Target?.If;
            set => this.Resolved.If = value;
        }

        public override AsyncApiJsonSchema Then
        {
            get => this.Target?.Then;
            set => this.Resolved.Then = value;
        }

        public override AsyncApiJsonSchema Else
        {
            get => this.Target?.Else;
            set => this.Resolved.Else = value;
        }

        public override ISet<string> Required
        {
            get => this.Target?.Required;
            set => this.Resolved.Required = value;
        }

        public override AsyncApiJsonSchema Items
        {
            get => this.Target?.Items;
            set => this.Resolved.Items = value;
        }

        public override AsyncApiJsonSchema AdditionalItems
        {
            get => this.Target?.AdditionalItems;
            set => this.Resolved.AdditionalItems = value;
        }

        public override int? MaxItems
        {
            get => this.Target?.MaxItems;
            set => this.Resolved.MaxItems = value;
        }

        public override int? MinItems
        {
            get => this.Target?.MinItems;
            set => this.Resolved.MinItems = value;
        }

        public override bool? UniqueItems
        {
            get => this.Target?.UniqueItems;
            set => this.Resolved.UniqueItems = value;
        }

        public override IDictionary<string, AsyncApiJsonSchema> Properties
        {
            get => this.Target?.Properties;
            set => this.Resolved.Properties = value;
        }

        public override int? MaxProperties
        {
            get => this.Target?.MaxProperties;
            set => this.Resolved.MaxProperties = value;
        }

        public override int? MinProperties
        {
            get => this.Target?.MinProperties;
            set => this.Resolved.MinProperties = value;
        }

        public override AsyncApiJsonSchema AdditionalProperties
        {
            get => this.Target?.AdditionalProperties;
            set => this.Resolved.AdditionalProperties = value;
        }

        public override IDictionary<string, AsyncApiJsonSchema> PatternProperties
        {
            get => this.Target?.PatternProperties;
            set => this.Resolved.PatternProperties = value;
        }

        public override AsyncApiJsonSchema PropertyNames
        {
            get => this.Target?.PropertyNames;
            set => this.Resolved.PropertyNames = value;
        }

        public override string Discriminator
        {
            get => this.Target?.Discriminator;
            set => this.Resolved.Discriminator = value;
        }

        public override IList<AsyncApiAny> Enum
        {
            get => this.Target?.Enum;
            set => this.Resolved.Enum = value;
        }

        public override IList<AsyncApiAny> Examples
        {
            get => this.Target?.Examples;
            set => this.Resolved.Examples = value;
        }

        public override AsyncApiAny Const
        {
            get => this.Target?.Const;
            set => this.Resolved.Const = value;
        }

        public override bool Nullable
        {
            get => this.Target?.Nullable ?? false;
            set => this.Resolved.Nullable = value;
        }

        public override AsyncApiExternalDocumentation ExternalDocs
        {
            get => this.Target?.ExternalDocs;
            set => this.Resolved.ExternalDocs = value;
        }

        public override bool Deprecated
        {
            get => this.Target?.Deprecated ?? false;
            set => this.Resolved.Deprecated = value;
        }

        public override IDictionary<string, IAsyncApiExtension> Extensions
        {
            get => this.Target?.Extensions;
            set => this.Resolved.Extensions = value;
        }

        public static bool operator !=(AsyncApiJsonSchemaReference left, AsyncApiJsonSchemaReference right) => !(left == right);

        public static bool operator ==(AsyncApiJsonSchemaReference left, AsyncApiJsonSchemaReference right)
        {
            return Equals(left, null) ? Equals(right, null) : left.Equals(right);
        }

        public bool Equals(AsyncApiJsonSchemaReference other)
        {
            if (other is null)
            {
                return false;
            }

            if (other.Target is AsyncApiJsonSchemaReference reference)
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
            if (obj is AsyncApiJsonSchemaReference reference)
            {
                return this.Equals(reference);
            }

            if (obj is AsyncApiJsonSchema message)
            {
                return this.Equals(message);
            }

            return false;
        }

        public bool Equals(AsyncApiJsonSchema other)
        {
            return other is not null && this.Target == other;
        }

        public override int GetHashCode()
        {
            return this.Target?.GetHashCode() ?? this.Reference?.GetHashCode() ?? 0;
        }

        public override void SerializeV2(IAsyncApiWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            var settings = writer.GetSettings();
            if (!settings.ShouldInlineReference(this.Reference))
            {
                this.Reference.SerializeV2(writer);
                return;
            }

            this.Reference.Workspace = writer.Workspace;
            // If Loop is detected then just Serialize as a reference.
            if (!settings.LoopDetector.PushLoop(this))
            {
                settings.LoopDetector.SaveLoop(this);
                this.Reference.SerializeV2(writer);
                return;
            }

            this.Resolved.SerializeV2(writer);
        }

        public override void SerializeV3(IAsyncApiWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            var settings = writer.GetSettings();
            if (!settings.ShouldInlineReference(this.Reference))
            {
                this.Reference.SerializeV3(writer);
                return;
            }

            this.Reference.Workspace = writer.Workspace;
            // If Loop is detected then just Serialize as a reference.
            if (!settings.LoopDetector.PushLoop(this))
            {
                settings.LoopDetector.SaveLoop(this);
                this.Reference.SerializeV3(writer);
                return;
            }

            this.Resolved.SerializeV3(writer);
        }
    }
}