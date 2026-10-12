namespace ByteBard.AsyncAPI.Bindings.Solace
{
    using System;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Writers;

    /// <summary>Represents a Solace integer, JSON Schema object, or JSON Schema reference.</summary>
    public class SolaceIntegerOrSchema : IAsyncApiSerializable
    {
        private long? integer;
        private AsyncApiJsonSchema schema;

        public long? Integer
        {
            get => this.integer;
            set
            {
                this.integer = value;
                if (value.HasValue) this.schema = null;
            }
        }

        public AsyncApiJsonSchema Schema
        {
            get => this.schema;
            set
            {
                this.schema = value;
                if (value != null) this.integer = null;
            }
        }

        /// <summary>
        /// Writes the integer or schema value directly for AsyncAPI 2.0, after validating that a value is present.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Neither an integer nor a schema is set.</exception>
        public void SerializeV2(IAsyncApiWriter writer) => this.Serialize(writer, false);
        /// <summary>
        /// Writes the integer or schema value directly for AsyncAPI 3.0, after validating that a value is present.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Neither an integer nor a schema is set.</exception>
        public void SerializeV3(IAsyncApiWriter writer) => this.Serialize(writer, true);

        /// <summary>
        /// Checks that an integer or schema is present, without validating its range or schema contents.
        /// </summary>
        /// <exception cref="InvalidOperationException">Neither an integer nor a schema is set.</exception>
        public void Validate()
        {
            if (!this.Integer.HasValue && this.Schema == null)
            {
                throw new InvalidOperationException("A Solace integer-or-schema value must contain an integer or a schema.");
            }
        }

        /// <summary>
        /// Validates and writes the integer or schema value without a property name or wrapper object.
        /// </summary>
        /// <param name="writer">The writer receiving the value.</param>
        /// <param name="v3">Uses AsyncAPI 3.0 schema serialization when true and AsyncAPI 2.0 when false.</param>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Neither an integer nor a schema is set.</exception>
        private void Serialize(IAsyncApiWriter writer, bool v3)
        {
            if (writer is null) throw new ArgumentNullException(nameof(writer));
            this.Validate();
            if (this.Integer.HasValue)
            {
                writer.WriteValue(this.Integer.Value);
                return;
            }

            if (this.Schema != null)
            {
                if (v3) this.Schema.SerializeV3(writer);
                else this.Schema.SerializeV2(writer);
            }
        }
    }
}
