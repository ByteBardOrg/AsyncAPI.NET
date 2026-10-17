namespace ByteBard.AsyncAPI.Bindings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers.Exceptions;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;

    /// <summary>
    /// A value represented by either a single string (<see cref="SingleString"/>) or a list of strings (<see cref="StringList"/>).
    /// </summary>
    /// <remarks>Strings and string lists convert implicitly, so <c>statement.Action = "sns:Publish";</c> works.</remarks>
    public abstract class StringOrStringList : IAsyncApiElement
    {
        private protected StringOrStringList()
        {
        }

        public static implicit operator StringOrStringList(string value) => value == null ? null : new SingleString(value);

        public static implicit operator StringOrStringList(string[] values) => values == null ? null : new StringList(values);

        public static implicit operator StringOrStringList(List<string> values) => values == null ? null : new StringList(values);

        /// <summary>
        /// Parses a scalar as a single string, or a list as a list of strings.
        /// </summary>
        /// <exception cref="AsyncApiReaderException">The node is neither a scalar nor a list of scalars.</exception>
        public static StringOrStringList Parse(ParseNode node)
        {
            switch (node)
            {
                case ValueNode:
                    return new SingleString(node.GetScalarValue());
                case ListNode listNode:
                    return new StringList(listNode.Select(item => item.GetScalarValue()));
                default:
                    throw new AsyncApiReaderException(
                        $"An error occured while parsing a {nameof(StringOrStringList)} node. " +
                        $"Node should contain a string value or a list of strings.",
                        node.Context);
            }
        }

        /// <summary>
        /// Writes the string or the list of strings directly, without a property name.
        /// </summary>
        public abstract void Write(IAsyncApiWriter writer);
    }

    /// <summary>The single string alternative of <see cref="StringOrStringList"/>.</summary>
    public sealed class SingleString : StringOrStringList
    {
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        public SingleString(string value)
        {
            this.Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Value { get; }

        public static implicit operator string(SingleString value) => value?.Value;

        public override void Write(IAsyncApiWriter writer) => writer.WriteValue(this.Value);
    }

    /// <summary>The list alternative of <see cref="StringOrStringList"/>.</summary>
    public sealed class StringList : StringOrStringList
    {
        /// <summary>
        /// Copies the supplied strings.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="values"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="values"/> contains a null item.</exception>
        public StringList(IEnumerable<string> values)
        {
            var copy = (values ?? throw new ArgumentNullException(nameof(values))).ToArray();
            if (copy.Any(value => value == null))
            {
                throw new ArgumentException($"{nameof(StringList)} items cannot be null.", nameof(values));
            }

            this.Values = copy;
        }

        public IReadOnlyList<string> Values { get; }

        public static implicit operator string[](StringList value) => value?.Values.ToArray();

        public override void Write(IAsyncApiWriter writer)
        {
            writer.WriteStartArray();
            foreach (var value in this.Values)
            {
                writer.WriteValue(value);
            }

            writer.WriteEndArray();
        }
    }
}
