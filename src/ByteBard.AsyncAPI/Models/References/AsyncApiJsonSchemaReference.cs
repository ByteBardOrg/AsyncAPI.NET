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
            set => this.Target.Title = value;
        }

        public override SchemaType? Type
        {
            get => this.Target?.Type;
            set => this.Target.Type = value;
        }

        public override string Format
        {
            get => this.Target?.Format;
            set => this.Target.Format = value;
        }

        public override string Description
        {
            get => this.Target?.Description;
            set => this.Target.Description = value;
        }

        public override double? Maximum
        {
            get => this.Target?.Maximum;
            set => this.Target.Maximum = value;
        }

        public override double? ExclusiveMaximum
        {
            get => this.Target?.ExclusiveMaximum;
            set => this.Target.ExclusiveMaximum = value;
        }

        public override double? Minimum
        {
            get => this.Target?.Minimum;
            set => this.Target.Minimum = value;
        }

        public override double? ExclusiveMinimum
        {
            get => this.Target?.ExclusiveMinimum;
            set => this.Target.ExclusiveMinimum = value;
        }

        public override int? MaxLength
        {
            get => this.Target?.MaxLength;
            set => this.Target.MaxLength = value;
        }

        public override int? MinLength
        {
            get => this.Target?.MinLength;
            set => this.Target.MinLength = value;
        }

        public override string Pattern
        {
            get => this.Target?.Pattern;
            set => this.Target.Pattern = value;
        }

        public override double? MultipleOf
        {
            get => this.Target?.MultipleOf;
            set => this.Target.MultipleOf = value;
        }

        public override AsyncApiAny Default
        {
            get => this.Target?.Default;
            set => this.Target.Default = value;
        }

        public override bool ReadOnly
        {
            get => this.Target.ReadOnly;
            set => this.Target.ReadOnly = value;
        }

        public override bool WriteOnly
        {
            get => this.Target.WriteOnly;
            set => this.Target.WriteOnly = value;
        }

        public override IList<AsyncApiJsonSchema> AllOf
        {
            get => this.Target?.AllOf;
            set => this.Target.AllOf = value;
        }

        public override IList<AsyncApiJsonSchema> OneOf
        {
            get => this.Target?.OneOf;
            set => this.Target.OneOf = value;
        }

        public override IList<AsyncApiJsonSchema> AnyOf
        {
            get => this.Target?.AnyOf;
            set => this.Target.AnyOf = value;
        }

        public override AsyncApiJsonSchema Not
        {
            get => this.Target?.Not;
            set => this.Target.Not = value;
        }

        public override AsyncApiJsonSchema Contains
        {
            get => this.Target?.Contains;
            set => this.Target.Contains = value;
        }

        public override AsyncApiJsonSchema If
        {
            get => this.Target?.If;
            set => this.Target.If = value;
        }

        public override AsyncApiJsonSchema Then
        {
            get => this.Target?.Then;
            set => this.Target.Then = value;
        }

        public override AsyncApiJsonSchema Else
        {
            get => this.Target?.Else;
            set => this.Target.Else = value;
        }

        public override ISet<string> Required
        {
            get => this.Target?.Required;
            set => this.Target.Required = value;
        }

        public override AsyncApiJsonSchema Items
        {
            get => this.Target?.Items;
            set => this.Target.Items = value;
        }

        public override AsyncApiJsonSchema AdditionalItems
        {
            get => this.Target?.AdditionalItems;
            set => this.Target.AdditionalItems = value;
        }

        public override int? MaxItems
        {
            get => this.Target?.MaxItems;
            set => this.Target.MaxItems = value;
        }

        public override int? MinItems
        {
            get => this.Target?.MinItems;
            set => this.Target.MinItems = value;
        }

        public override bool? UniqueItems
        {
            get => this.Target?.UniqueItems;
            set => this.Target.UniqueItems = value;
        }

        public override IDictionary<string, AsyncApiJsonSchema> Properties
        {
            get => this.Target?.Properties;
            set => this.Target.Properties = value;
        }

        public override int? MaxProperties
        {
            get => this.Target?.MaxProperties;
            set => this.Target.MaxProperties = value;
        }

        public override int? MinProperties
        {
            get => this.Target?.MinProperties;
            set => this.Target.MinProperties = value;
        }

        public override AsyncApiJsonSchema AdditionalProperties
        {
            get => this.Target?.AdditionalProperties;
            set => this.Target.AdditionalProperties = value;
        }

        public override IDictionary<string, AsyncApiJsonSchema> PatternProperties
        {
            get => this.Target?.PatternProperties;
            set => this.Target.PatternProperties = value;
        }

        public override AsyncApiJsonSchema PropertyNames
        {
            get => this.Target?.PropertyNames;
            set => this.Target.PropertyNames = value;
        }

        public override string Discriminator
        {
            get => this.Target?.Discriminator;
            set => this.Target.Discriminator = value;
        }

        public override IList<AsyncApiAny> Enum
        {
            get => this.Target?.Enum;
            set => this.Target.Enum = value;
        }

        public override IList<AsyncApiAny> Examples
        {
            get => this.Target?.Examples;
            set => this.Target.Examples = value;
        }

        public override AsyncApiAny Const
        {
            get => this.Target?.Const;
            set => this.Target.Const = value;
        }

        public override bool Nullable
        {
            get => this.Target.Nullable;
            set => this.Target.Nullable = value;
        }

        public override AsyncApiExternalDocumentation ExternalDocs
        {
            get => this.Target?.ExternalDocs;
            set => this.Target.ExternalDocs = value;
        }

        public override bool Deprecated
        {
            get => this.Target.Deprecated;
            set => this.Target.Deprecated = value;
        }

        public override IDictionary<string, IAsyncApiExtension> Extensions
        {
            get => this.Target?.Extensions;
            set => this.Target.Extensions = value;
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
            return this.Target == other;
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
            if (this.Target is null)
            {
                throw new AsyncApiWriterException($"Cannot inline '{this.Reference.Reference}': the reference could not be resolved.");
            }

            // A schema that references itself can't be inlined, so it is written as a reference and rendered in components.
            if (IsInReferenceCycle(this.Target, writer.Workspace) || !settings.LoopDetector.PushLoop(this))
            {
                settings.LoopDetector.SaveLoop(this);
                this.Reference.SerializeV2(writer);
                return;
            }

            try
            {
                this.Target.SerializeV2(writer);
            }
            finally
            {
                settings.LoopDetector.PopLoop<AsyncApiJsonSchemaReference>();
            }
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
            if (this.Target is null)
            {
                throw new AsyncApiWriterException($"Cannot inline '{this.Reference.Reference}': the reference could not be resolved.");
            }

            // A schema that references itself can't be inlined, so it is written as a reference and rendered in components.
            if (IsInReferenceCycle(this.Target, writer.Workspace) || !settings.LoopDetector.PushLoop(this))
            {
                settings.LoopDetector.SaveLoop(this);
                this.Reference.SerializeV3(writer);
                return;
            }

            try
            {
                this.Target.SerializeV3(writer);
            }
            finally
            {
                settings.LoopDetector.PopLoop<AsyncApiJsonSchemaReference>();
            }
        }

        /// <summary>
        /// Determines whether the schema can reach itself through references.
        /// </summary>
        private static bool IsInReferenceCycle(AsyncApiJsonSchema schema, AsyncApiWorkspace workspace)
        {
            schema = Dereference(schema, workspace);
            if (schema is null)
            {
                return false;
            }

            var visited = new HashSet<AsyncApiJsonSchema>();
            var pending = new Stack<AsyncApiJsonSchema>();
            PushChildren(schema, pending);

            while (pending.Count > 0)
            {
                var current = Dereference(pending.Pop(), workspace);
                if (current is null)
                {
                    continue;
                }

                if (ReferenceEquals(current, schema))
                {
                    return true;
                }

                if (visited.Add(current))
                {
                    PushChildren(current, pending);
                }
            }

            return false;
        }

        private static AsyncApiJsonSchema Dereference(AsyncApiJsonSchema schema, AsyncApiWorkspace workspace)
        {
            var hops = 0;
            while (schema is AsyncApiJsonSchemaReference reference)
            {
                // Guards against references that only point at each other.
                if (++hops > 64)
                {
                    return null;
                }

                reference.Reference.Workspace ??= workspace;
                schema = reference.Target;
            }

            return schema;
        }

        private static void PushChildren(AsyncApiJsonSchema schema, Stack<AsyncApiJsonSchema> pending)
        {
            void Push(AsyncApiJsonSchema child)
            {
                if (child is not null)
                {
                    pending.Push(child);
                }
            }

            void PushAll(IEnumerable<AsyncApiJsonSchema> children)
            {
                if (children is null)
                {
                    return;
                }

                foreach (var child in children)
                {
                    Push(child);
                }
            }

            PushAll(schema.AllOf);
            PushAll(schema.OneOf);
            PushAll(schema.AnyOf);
            Push(schema.Not);
            Push(schema.Contains);
            Push(schema.If);
            Push(schema.Then);
            Push(schema.Else);
            Push(schema.Items);
            Push(schema.AdditionalItems);
            PushAll(schema.Properties?.Values);
            Push(schema.AdditionalProperties);
            PushAll(schema.PatternProperties?.Values);
            Push(schema.PropertyNames);
        }
    }
}