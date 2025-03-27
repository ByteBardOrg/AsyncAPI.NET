namespace ByteBard.AsyncAPI.Readers.ParseNodes
{
    using System;
    using ByteBard.AsyncAPI.Models;

    internal class AnyFieldMapParameter<T>
    {
        public AnyFieldMapParameter(
            Func<T, AsyncApiAny> propertyGetter,
            Action<T, AsyncApiAny> propertySetter,
            Func<T, AsyncApiJsonSchema> schemaGetter)
        {
            this.PropertyGetter = propertyGetter;
            this.PropertySetter = propertySetter;
            this.SchemaGetter = schemaGetter;
        }

        public Func<T, AsyncApiAny> PropertyGetter { get; }

        public Action<T, AsyncApiAny> PropertySetter { get; }

        public Func<T, AsyncApiJsonSchema> SchemaGetter { get; }
    }
}