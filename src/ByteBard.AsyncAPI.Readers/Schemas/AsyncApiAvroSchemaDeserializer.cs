namespace ByteBard.AsyncAPI.Readers
{
    using System.Collections.Generic;
    using ByteBard.AsyncAPI.Exceptions;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Models.Avro.LogicalTypes;
    using ByteBard.AsyncAPI.Readers.Exceptions;
    using ByteBard.AsyncAPI.Readers.ParseNodes;

    public class AsyncApiAvroSchemaDeserializer
    {
        private static readonly ISet<string> FieldPropertyNames = new HashSet<string>
        {
            "name",
            "type",
            "doc",
            "default",
            "aliases",
            "order",
        };

        private static readonly ISet<string> RecordPropertyNames = new HashSet<string>
        {
            "type",
            "name",
            "doc",
            "namespace",
            "aliases",
            "fields",
        };

        private static readonly ISet<string> EnumPropertyNames = new HashSet<string>
        {
            "type",
            "name",
            "doc",
            "namespace",
            "aliases",
            "symbols",
            "default",
        };

        private static readonly ISet<string> FixedPropertyNames = new HashSet<string>
        {
            "type",
            "name",
            "namespace",
            "aliases",
            "size",
        };

        private static readonly ISet<string> ArrayPropertyNames = new HashSet<string>
        {
            "type",
            "items",
        };

        private static readonly ISet<string> MapPropertyNames = new HashSet<string>
        {
            "type",
            "values",
        };

        private static readonly ISet<string> PrimitivePropertyNames = new HashSet<string>
        {
            "type",
        };

        private static readonly FixedFieldMap<AvroDecimal> DecimalFixedFields = new()
        {
            { "type", (a, n) => { } },
            { "logicalType", (a, n) => { } },
            { "precision", (a, n) => a.Precision = int.Parse(n.GetScalarValue()) },
            { "scale", (a, n) => a.Scale = int.Parse(n.GetScalarValue()) },
        };

        private static readonly FixedFieldMap<AvroUUID> UUIDFixedFields = new()
        {
            { "type", (a, n) => { } },
            { "logicalType", (a, n) => { } },
        };

        private static readonly FixedFieldMap<AvroDate> DateFixedFields = new()
        {
            { "type", (a, n) => { } },
            { "logicalType", (a, n) => { } },
        };

        private static readonly FixedFieldMap<AvroTimeMillis> TimeMillisFixedFields = new()
        {
            { "type", (a, n) => { } },
            { "logicalType", (a, n) => { } },
        };

        private static readonly FixedFieldMap<AvroTimeMicros> TimeMicrosFixedFields = new()
        {
            { "type", (a, n) => { } },
            { "logicalType", (a, n) => { } },
        };

        private static readonly FixedFieldMap<AvroTimestampMillis> TimestampMillisFixedFields = new()
        {
            { "type", (a, n) => { } },
            { "logicalType", (a, n) => { } },
        };

        private static readonly FixedFieldMap<AvroTimestampMicros> TimestampMicrosFixedFields = new()
        {
            { "type", (a, n) => { } },
            { "logicalType", (a, n) => { } },
        };

        private static readonly FixedFieldMap<AvroDuration> DurationFixedFields = new()
        {
            { "type", (a, n) => { } },
            { "logicalType", (a, n) => { } },
            { "name", (a, n) => a.Name = n.GetScalarValue() },
            { "namespace", (a, n) => a.Namespace = n.GetScalarValue() },
            { "aliases", (a, n) => a.Aliases = n.CreateSimpleList(n2 => n2.GetScalarValue()) },
            { "size", (a, n) => { } },
        };

        private static readonly PatternFieldMap<AvroDecimal> DecimalMetadataPatternFields = new()
        {
            { s => s.StartsWith(string.Empty), (a, p, n) => a.Metadata[p] = n.CreateAny() },
        };

        private static readonly PatternFieldMap<AvroUUID> UUIDMetadataPatternFields = new()
        {
            { s => s.StartsWith(string.Empty), (a, p, n) => a.Metadata[p] = n.CreateAny() },
        };

        private static readonly PatternFieldMap<AvroDate> DateMetadataPatternFields = new()
        {
            { s => s.StartsWith(string.Empty), (a, p, n) => a.Metadata[p] = n.CreateAny() },
        };

        private static readonly PatternFieldMap<AvroTimeMillis> TimeMillisMetadataPatternFields = new()
        {
            { s => s.StartsWith(string.Empty), (a, p, n) => a.Metadata[p] = n.CreateAny() },
        };

        private static readonly PatternFieldMap<AvroTimeMicros> TimeMicrosMetadataPatternFields = new()
        {
            { s => s.StartsWith(string.Empty), (a, p, n) => a.Metadata[p] = n.CreateAny() },
        };

        private static readonly PatternFieldMap<AvroTimestampMillis> TimestampMillisMetadataPatternFields = new()
        {
            { s => s.StartsWith(string.Empty), (a, p, n) => a.Metadata[p] = n.CreateAny() },
        };

        private static readonly PatternFieldMap<AvroTimestampMicros> TimestampMicrosMetadataPatternFields = new()
        {
            { s => s.StartsWith(string.Empty), (a, p, n) => a.Metadata[p] = n.CreateAny() },
        };

        private static readonly PatternFieldMap<AvroDuration> DurationMetadataPatternFields = new()
        {
            { s => s.StartsWith(string.Empty), (a, p, n) => a.Metadata[p] = n.CreateAny() },
        };

        private readonly Dictionary<string, AsyncApiAvroSchema> namedTypes = new Dictionary<string, AsyncApiAvroSchema>();
        private readonly Stack<string> namespaces = new Stack<string>();

        private string CurrentNamespace => this.namespaces.Count > 0 ? this.namespaces.Peek() : null;

        public static AsyncApiAvroSchema LoadSchema(ParseNode node)
        {
            return new AsyncApiAvroSchemaDeserializer().LoadSchemaCore(node);
        }

        private AsyncApiAvroSchema LoadSchemaCore(ParseNode node)
        {
            if (node is PropertyNode propertyNode)
            {
                node = propertyNode.Value;
            }

            if (node is ValueNode valueNode)
            {
                return this.LoadStringSchema(valueNode.GetScalarValue());
            }

            if (node is ListNode listNode)
            {
                var union = new AvroUnion();
                foreach (var item in listNode)
                {
                    union.Types.Add(this.LoadSchemaCore(item));
                }

                return union;
            }

            if (node is MapNode mapNode)
            {
                var pointer = mapNode.GetReferencePointer();

                if (pointer != null)
                {
                    return new AsyncApiAvroSchemaReference(pointer);
                }

                var isLogicalType = mapNode["logicalType"] != null;
                if (isLogicalType)
                {
                    return this.LoadLogicalType(mapNode);
                }

                var type = mapNode["type"]?.Value.GetScalarValue();
                switch (type)
                {
                    case "record":
                        return this.LoadRecord(mapNode);
                    case "enum":
                        return this.LoadEnum(mapNode);
                    case "fixed":
                        return this.LoadFixed(mapNode);
                    case "array":
                        return this.LoadArray(mapNode);
                    case "map":
                        return this.LoadMap(mapNode);
                    case "union":
                        return this.LoadUnion(mapNode);
                    default:
                        if (type != null)
                        {
                            var schema = this.LoadStringSchema(type);
                            this.ParseMetadata(mapNode, schema.Metadata, PrimitivePropertyNames);
                            return schema;
                        }

                        throw new AsyncApiException($"Unsupported type: {type}");
                }
            }

            throw new AsyncApiReaderException("Invalid node type");
        }

        private AvroRecord LoadRecord(MapNode mapNode)
        {
            var record = new AvroRecord
            {
                Name = this.GetStringValue(mapNode, "name"),
                Namespace = this.GetStringValue(mapNode, "namespace"),
                Doc = this.GetStringValue(mapNode, "doc"),
            };

            var aliases = mapNode["aliases"]?.Value;
            if (aliases != null)
            {
                record.Aliases = aliases.CreateSimpleList(n => n.GetScalarValue());
            }

            this.RegisterNamedType(record, record.Name, record.Namespace);

            this.namespaces.Push(this.GetNamespaceForNamedType(record.Name, record.Namespace));
            try
            {
                var fields = mapNode["fields"]?.Value;
                if (fields != null)
                {
                    record.Fields = fields.CreateList(this.LoadField);
                }
            }
            finally
            {
                this.namespaces.Pop();
            }

            this.ParseMetadata(mapNode, record.Metadata, RecordPropertyNames);
            return record;
        }

        private AvroEnum LoadEnum(MapNode mapNode)
        {
            var @enum = new AvroEnum
            {
                Name = this.GetStringValue(mapNode, "name"),
                Namespace = this.GetStringValue(mapNode, "namespace"),
                Doc = this.GetStringValue(mapNode, "doc"),
                Default = this.GetStringValue(mapNode, "default"),
            };

            var aliases = mapNode["aliases"]?.Value;
            if (aliases != null)
            {
                @enum.Aliases = aliases.CreateSimpleList(n => n.GetScalarValue());
            }

            var symbols = mapNode["symbols"]?.Value;
            if (symbols != null)
            {
                @enum.Symbols = symbols.CreateSimpleList(n => n.GetScalarValue());
            }

            this.RegisterNamedType(@enum, @enum.Name, @enum.Namespace);
            this.ParseMetadata(mapNode, @enum.Metadata, EnumPropertyNames);
            return @enum;
        }

        private AvroFixed LoadFixed(MapNode mapNode)
        {
            var @fixed = new AvroFixed
            {
                Name = this.GetStringValue(mapNode, "name"),
                Namespace = this.GetStringValue(mapNode, "namespace"),
            };

            var aliases = mapNode["aliases"]?.Value;
            if (aliases != null)
            {
                @fixed.Aliases = aliases.CreateSimpleList(n => n.GetScalarValue());
            }

            var size = mapNode["size"]?.Value;
            if (size != null)
            {
                @fixed.Size = int.Parse(size.GetScalarValue(), size.Context.Settings.CultureInfo);
            }

            this.RegisterNamedType(@fixed, @fixed.Name, @fixed.Namespace);
            this.ParseMetadata(mapNode, @fixed.Metadata, FixedPropertyNames);
            return @fixed;
        }

        private AvroArray LoadArray(MapNode mapNode)
        {
            var array = new AvroArray();
            var items = mapNode["items"]?.Value;
            if (items != null)
            {
                array.Items = this.LoadSchemaCore(items);
            }

            this.ParseMetadata(mapNode, array.Metadata, ArrayPropertyNames);
            return array;
        }

        private AvroMap LoadMap(MapNode mapNode)
        {
            var map = new AvroMap();
            var values = mapNode["values"]?.Value;
            if (values != null)
            {
                map.Values = this.LoadSchemaCore(values);
            }

            this.ParseMetadata(mapNode, map.Metadata, MapPropertyNames);
            return map;
        }

        private AvroUnion LoadUnion(MapNode mapNode)
        {
            var union = new AvroUnion();
            var types = mapNode["types"]?.Value;
            if (types is ListNode listNode)
            {
                foreach (var item in listNode)
                {
                    union.Types.Add(this.LoadSchemaCore(item));
                }
            }

            return union;
        }

        private AsyncApiAvroSchema LoadLogicalType(MapNode mapNode)
        {
            var type = mapNode["logicalType"]?.Value.GetScalarValue();
            switch (type)
            {
                case "decimal":
                    var @decimal = new AvroDecimal();
                    mapNode.ParseFields(@decimal, DecimalFixedFields, DecimalMetadataPatternFields);
                    return @decimal;
                case "uuid":
                    var uuid = new AvroUUID();
                    mapNode.ParseFields(uuid, UUIDFixedFields, UUIDMetadataPatternFields);
                    return uuid;
                case "date":
                    var date = new AvroDate();
                    mapNode.ParseFields(date, DateFixedFields, DateMetadataPatternFields);
                    return date;
                case "time-millis":
                    var timeMillis = new AvroTimeMillis();
                    mapNode.ParseFields(timeMillis, TimeMillisFixedFields, TimeMillisMetadataPatternFields);
                    return timeMillis;
                case "time-micros":
                    var timeMicros = new AvroTimeMicros();
                    mapNode.ParseFields(timeMicros, TimeMicrosFixedFields, TimeMicrosMetadataPatternFields);
                    return timeMicros;
                case "timestamp-millis":
                    var timestampMillis = new AvroTimestampMillis();
                    mapNode.ParseFields(timestampMillis, TimestampMillisFixedFields, TimestampMillisMetadataPatternFields);
                    return timestampMillis;
                case "timestamp-micros":
                    var timestampMicros = new AvroTimestampMicros();
                    mapNode.ParseFields(timestampMicros, TimestampMicrosFixedFields, TimestampMicrosMetadataPatternFields);
                    return timestampMicros;
                case "duration":
                    var duration = new AvroDuration();
                    mapNode.ParseFields(duration, DurationFixedFields, DurationMetadataPatternFields);
                    this.RegisterNamedType(duration, duration.Name, duration.Namespace);
                    return duration;
                default:
                    throw new AsyncApiException($"Unsupported type: {type}");
            }
        }

        private AvroField LoadField(ParseNode node)
        {
            var mapNode = node.CheckMapNode("field");
            var field = new AvroField
            {
                Name = this.GetStringValue(mapNode, "name"),
                Doc = this.GetStringValue(mapNode, "doc"),
            };

            var type = mapNode["type"]?.Value;
            if (type != null)
            {
                field.Type = this.LoadSchemaCore(type);
            }

            var @default = mapNode["default"]?.Value;
            if (@default != null)
            {
                field.Default = @default.CreateAny();
            }

            var aliases = mapNode["aliases"]?.Value;
            if (aliases != null)
            {
                field.Aliases = aliases.CreateSimpleList(n => n.GetScalarValue());
            }

            var order = mapNode["order"]?.Value;
            if (order != null)
            {
                field.Order = order.GetScalarValue().GetEnumFromDisplayName<AvroFieldOrder>();
            }

            this.ParseMetadata(mapNode, field.Metadata, FieldPropertyNames);
            return field;
        }

        private AsyncApiAvroSchema LoadStringSchema(string type)
        {
            if (this.IsPrimitiveType(type))
            {
                return new AvroPrimitive(type.GetEnumFromDisplayName<AvroPrimitiveType>());
            }

            var fullName = this.GetReferenceFullName(type);
            this.namedTypes.TryGetValue(fullName, out var target);
            return new AvroNamedType(type, target);
        }

        private void RegisterNamedType(AsyncApiAvroSchema schema, string name, string @namespace)
        {
            var fullName = this.GetFullName(name, @namespace);
            if (fullName != null)
            {
                this.namedTypes[fullName] = schema;
            }
        }

        private string GetReferenceFullName(string name)
        {
            if (name == null || name.IndexOf('.') >= 0)
            {
                return name;
            }

            var @namespace = this.CurrentNamespace;
            return string.IsNullOrEmpty(@namespace) ? name : $"{@namespace}.{name}";
        }

        private string GetFullName(string name, string @namespace)
        {
            if (name == null || name.IndexOf('.') >= 0)
            {
                return name;
            }

            @namespace ??= this.CurrentNamespace;
            return string.IsNullOrEmpty(@namespace) ? name : $"{@namespace}.{name}";
        }

        private string GetNamespaceForNamedType(string name, string @namespace)
        {
            if (name != null && name.IndexOf('.') >= 0)
            {
                var lastDot = name.LastIndexOf('.');
                return lastDot > 0 ? name.Substring(0, lastDot) : string.Empty;
            }

            return @namespace ?? this.CurrentNamespace;
        }

        private string GetStringValue(MapNode mapNode, string propertyName)
        {
            return mapNode[propertyName]?.Value.GetScalarValue();
        }

        private bool IsPrimitiveType(string type)
        {
            switch (type)
            {
                case "null":
                case "boolean":
                case "int":
                case "long":
                case "float":
                case "double":
                case "bytes":
                case "string":
                    return true;
                default:
                    return false;
            }
        }

        private void ParseMetadata(MapNode mapNode, IDictionary<string, AsyncApiAny> metadata, ISet<string> fixedFields)
        {
            foreach (var propertyNode in mapNode)
            {
                if (!fixedFields.Contains(propertyNode.Name))
                {
                    metadata[propertyNode.Name] = propertyNode.Value.CreateAny();
                }
            }
        }
    }
}
