namespace ByteBard.AsyncAPI.Readers.ParseNodes
{
    using System;
    using System.Collections.Generic;
    using ByteBard.AsyncAPI.Models;

    internal class AnyListFieldMapParameter<T>
    {
        public AnyListFieldMapParameter(
            Func<T, IList<AsyncApiAny>> propertyGetter,
            Action<T, IList<AsyncApiAny>> propertySetter,
            Func<T, AsyncApiJsonSchema> schemaGetter)
        {
            this.PropertyGetter = propertyGetter;
            this.PropertySetter = propertySetter;
            this.SchemaGetter = schemaGetter;
        }

        public Func<T, IList<AsyncApiAny>> PropertyGetter { get; }

        public Action<T, IList<AsyncApiAny>> PropertySetter { get; }

        public Func<T, AsyncApiJsonSchema> SchemaGetter { get; }
    }
}