namespace ByteBard.AsyncAPI.Models
{
    using System.Collections.Generic;
    using ByteBard.AsyncAPI.Writers;

    public class AvroNamedType : AsyncApiAvroSchema
    {
        private IDictionary<string, AsyncApiAny> metadata = new Dictionary<string, AsyncApiAny>();

        public AvroNamedType(string name, AsyncApiAvroSchema target = null)
        {
            this.Name = name;
            this.Target = target;
        }

        public string Name { get; set; }

        public AsyncApiAvroSchema Target { get; set; }

        public override string Type => this.Name;

        public override IDictionary<string, AsyncApiAny> Metadata
        {
            get => this.Target?.Metadata ?? this.metadata;
            set
            {
                if (this.Target != null)
                {
                    this.Target.Metadata = value;
                    return;
                }

                this.metadata = value ?? new Dictionary<string, AsyncApiAny>();
            }
        }

        public override T As<T>()
        {
            var result = base.As<T>();
            return result ?? this.Target?.As<T>();
        }

        public override bool Is<T>()
        {
            return base.Is<T>() || this.Target?.Is<T>() == true;
        }

        public override bool TryGetAs<T>(out T result)
        {
            if (base.TryGetAs(out result))
            {
                return true;
            }

            if (this.Target != null)
            {
                return this.Target.TryGetAs(out result);
            }

            result = default;
            return false;
        }

        public override void SerializeV2(IAsyncApiWriter writer)
        {
            writer.WriteValue(this.Name);
        }

        public override void SerializeV3(IAsyncApiWriter writer)
        {
            writer.WriteValue(this.Name);
        }
    }
}
