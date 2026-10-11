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

        public void SerializeV2(IAsyncApiWriter writer) => this.Serialize(writer, false);
        public void SerializeV3(IAsyncApiWriter writer) => this.Serialize(writer, true);

        public void Validate()
        {
            if (!this.Integer.HasValue && this.Schema == null)
            {
                throw new InvalidOperationException("A Solace integer-or-schema value must contain an integer or a schema.");
            }
        }

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
