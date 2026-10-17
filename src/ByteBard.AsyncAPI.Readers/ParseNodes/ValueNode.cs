namespace ByteBard.AsyncAPI.Readers.ParseNodes
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text.Json.Nodes;
    using ByteBard.AsyncAPI.Exceptions;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers.Exceptions;

    public class ValueNode : ParseNode
    {
        private readonly JsonNode node;
        private string cachedScalarValue;

        public ValueNode(ParsingContext context, JsonNode node)
            : base(
            context)
        {
            if (!(node is JsonValue scalarNode))
            {
                throw new AsyncApiReaderException("Expected a value.");
            }

            this.node = scalarNode;
        }

        public override string GetScalarValue()
        {
            if (this.cachedScalarValue == null)
            {
                var scalarNode = this.node is JsonValue value ? value : throw new AsyncApiException($"Expected scalar value");
                this.cachedScalarValue = Convert.ToString(scalarNode.GetValue<object>(), this.Context.Settings.CultureInfo);
            }

            return this.cachedScalarValue;
        }

        public override string GetScalarValueOrDefault(string defaultValue = null)
        {
            var value = this.GetScalarValue();
            if (value is not null)
            {
                return value;
            }

            return defaultValue;
        }

        public override int GetIntegerValue()
        {
            if (int.TryParse(this.GetScalarValue(), NumberStyles.Integer, this.Context.Settings.CultureInfo, out int value))
            {
                return value;
            }

            throw new AsyncApiReaderException(this.DescribeInvalidInteger("integer", int.MinValue, int.MaxValue), this.Context);
        }

        public override int? GetIntegerValueOrDefault(int? defaultValue = null)
        {
            if (int.TryParse(this.GetScalarValue(), NumberStyles.Integer, this.Context.Settings.CultureInfo, out int value))
            {
                return value;
            }

            return defaultValue;
        }

        public override long GetLongValue()
        {
            if (long.TryParse(this.GetScalarValue(), NumberStyles.Integer, this.Context.Settings.CultureInfo, out long value))
            {
                return value;
            }

            throw new AsyncApiReaderException(this.DescribeInvalidInteger("long", long.MinValue, long.MaxValue), this.Context);
        }

        public override long? GetLongValueOrDefault(long? defaultValue = null)
        {
            if (long.TryParse(this.GetScalarValue(), NumberStyles.Integer, this.Context.Settings.CultureInfo, out long value))
            {
                return value;
            }

            return defaultValue;
        }

        public override bool GetBooleanValue()
        {
            if (bool.TryParse(this.GetScalarValue(), out bool value))
            {
                return value;
            }

            throw new AsyncApiReaderException($"Value '{this.GetScalarValue()}' could not parse to bool. Expected true or false.", this.Context);
        }

        public override bool? GetBooleanValueOrDefault(bool? defaultValue = null)
        {
            if (bool.TryParse(this.GetScalarValue(), out bool value))
            {
                return value;
            }

            return defaultValue;
        }

        public override AsyncApiAny CreateAny()
        {
            var value = this.GetScalarValue();
            return new AsyncApiAny(this.node);
        }

        /// <summary>
        /// Describes why the scalar is not a valid integer of the given type, telling an out-of-range number apart from a value that is not a whole number.
        /// </summary>
        private string DescribeInvalidInteger(string typeName, long min, long max)
        {
            var value = this.GetScalarValue();
            var digits = value?.Trim().TrimStart('+', '-');
            var isWholeNumber = !string.IsNullOrEmpty(digits) && digits.All(c => c >= '0' && c <= '9');

            return isWholeNumber
                ? $"Value '{value}' could not parse to {typeName}. It must be between {min} and {max}."
                : $"Value '{value}' could not parse to {typeName}. Expected a whole number.";
        }
    }
}