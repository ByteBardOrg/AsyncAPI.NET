namespace ByteBard.AsyncAPI.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using ByteBard.AsyncAPI.Bindings;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Writers;

    public class AsyncApiBindings<TBinding> : IDictionary<string, TBinding>, IAsyncApiSerializable, IAsyncApiExtensible
        where TBinding : IBinding
    {
        private Dictionary<string, TBinding> inner = new Dictionary<string, TBinding>();

        public virtual void SerializeV2(IAsyncApiWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteStartObject();

            foreach (var binding in this)
            {
                var bindingType = binding.Key;
                var bindingValue = binding.Value;

                writer.WritePropertyName(bindingType);

                bindingValue.SerializeV2(writer);
            }

            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }

        public virtual void SerializeV3(IAsyncApiWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            // AsyncAPI 3 only allows the specification's binding names, so a custom binding is written as an extension.
            // Its x- name can then clash with an extension (or another binding) of the same name; refuse to write a
            // document with a duplicate property rather than silently dropping one of them.
            var names = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var extension in this.Extensions.Keys)
            {
                names[extension] = $"extension '{extension}'";
            }

            foreach (var binding in this)
            {
                var name = AsyncApiBindingKeys.ToV3Key(binding.Key);
                if (names.TryGetValue(name, out var existing))
                {
                    throw new AsyncApiWriterException(
                        $"Binding '{binding.Key}' is written as '{name}' in AsyncAPI 3, which clashes with {existing}.");
                }

                names[name] = $"binding '{binding.Key}'";
            }

            writer.WriteStartObject();

            foreach (var binding in this)
            {
                writer.WritePropertyName(AsyncApiBindingKeys.ToV3Key(binding.Key));

                binding.Value.SerializeV3(writer);
            }

            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }

        public virtual IDictionary<string, IAsyncApiExtension> Extensions { get; set; } = new Dictionary<string, IAsyncApiExtension>();

        public virtual void Add(TBinding binding)
        {
            this[binding.BindingKey] = binding;
        }

        public virtual TBinding this[string key]
        {
            get => this.inner[key];
            set => this.inner[key] = value;
        }

        public virtual ICollection<string> Keys => this.inner.Keys;

        public virtual ICollection<TBinding> Values => this.inner.Values;

        public virtual int Count => this.inner.Count;

        public virtual bool IsReadOnly => ((IDictionary<string, TBinding>)this.inner).IsReadOnly;

        public virtual void Add(string key, TBinding value)
        {
            this.inner.Add(key, value);
        }

        public virtual bool ContainsKey(string key)
        {
            return this.inner.ContainsKey(key);
        }

        public virtual bool Remove(string key)
        {
            return this.inner.Remove(key);
        }

        public virtual bool TryGetValue(string key, out TBinding value)
        {
            return this.inner.TryGetValue(key, out value);
        }

        public virtual void Add(KeyValuePair<string, TBinding> item)
        {
            ((IDictionary<string, TBinding>)this.inner).Add(item);
        }

        public virtual void Clear()
        {
            this.inner.Clear();
        }

        public virtual bool Contains(KeyValuePair<string, TBinding> item)
        {
            return ((IDictionary<string, TBinding>)this.inner).Contains(item);
        }

        public virtual void CopyTo(KeyValuePair<string, TBinding>[] array, int arrayIndex)
        {
            ((IDictionary<string, TBinding>)this.inner).CopyTo(array, arrayIndex);
        }

        public virtual bool Remove(KeyValuePair<string, TBinding> item)
        {
            return ((IDictionary<string, TBinding>)this.inner).Remove(item);
        }

        public virtual IEnumerator<KeyValuePair<string, TBinding>> GetEnumerator()
        {
            return this.inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.inner.GetEnumerator();
        }
    }
}
