namespace ByteBard.AsyncAPI.Bindings.Kafka
{
    using System;
    using System.Collections.Generic;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Writers;

    public class TopicConfigurationObject : IAsyncApiElement
    {
        private static readonly HashSet<string> FixedFieldNames = new()
        {
            AsyncApiConstants.CleanupPolicy,
            AsyncApiConstants.RetentionMilliseconds,
            AsyncApiConstants.RetentionBytes,
            AsyncApiConstants.DeleteRetentionMilliseconds,
            AsyncApiConstants.MaxMessageBytes,
            AsyncApiConstants.ConfluentKeySchemaValidation,
            AsyncApiConstants.ConfluentKeySubjectName,
            AsyncApiConstants.ConfluentValueSchemaValidation,
            AsyncApiConstants.ConfluentValueSubjectName,
        };

        /// <summary>
        /// The cleanup.policy configuration option.
        /// </summary>
        public List<string> CleanupPolicy { get; set; }

        /// <summary>
        /// The retention.ms configuration option.
        /// </summary>
        public long? RetentionMilliseconds { get; set; }

        /// <summary>
        /// The retention.bytes configuration option.
        /// </summary>
        public long? RetentionBytes { get; set; }

        /// <summary>
        /// The delete.retention.ms configuration option.
        /// </summary>
        public long? DeleteRetentionMilliseconds { get; set; }

        /// <summary>
        /// The max.message.bytes configuration option.
        /// </summary>
        public int? MaxMessageBytes { get; set; }

        /// <summary>
        /// The confluent.key.schema.validation configuration option.
        /// </summary>
        public bool? ConfluentKeySchemaValidation { get; set; }

        /// <summary>
        /// The confluent.key.subject.name.strategy configuration option.
        /// </summary>
        public string ConfluentKeySubjectName { get; set; }

        /// <summary>
        /// The confluent.value.schema.validation configuration option.
        /// </summary>
        public bool? ConfluentValueSchemaValidation { get; set; }

        /// <summary>
        /// The confluent.value.subject.name.strategy configuration option.
        /// </summary>
        public string ConfluentValueSubjectName { get; set; }

        /// <summary>
        /// Additional Kafka topic configuration values not represented by fixed properties.
        /// Values are retained as JSON-backed <see cref="AsyncApiAny"/> instances.
        /// </summary>
        public Dictionary<string, AsyncApiAny> AdditionalProperties { get; set; } = new();

        public void Serialize(IAsyncApiWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteStartObject();
            writer.WriteOptionalCollection(AsyncApiConstants.CleanupPolicy, this.CleanupPolicy, (w, s) => w.WriteValue(s));
            writer.WriteOptionalProperty(AsyncApiConstants.RetentionMilliseconds, this.RetentionMilliseconds);
            writer.WriteOptionalProperty(AsyncApiConstants.RetentionBytes, this.RetentionBytes);
            writer.WriteOptionalProperty(AsyncApiConstants.DeleteRetentionMilliseconds, this.DeleteRetentionMilliseconds);
            writer.WriteOptionalProperty(AsyncApiConstants.MaxMessageBytes, this.MaxMessageBytes);
            writer.WriteOptionalProperty<bool>(AsyncApiConstants.ConfluentKeySchemaValidation, this.ConfluentKeySchemaValidation);
            writer.WriteOptionalProperty(AsyncApiConstants.ConfluentKeySubjectName, this.ConfluentKeySubjectName);
            writer.WriteOptionalProperty<bool>(AsyncApiConstants.ConfluentValueSchemaValidation, this.ConfluentValueSchemaValidation);
            writer.WriteOptionalProperty(AsyncApiConstants.ConfluentValueSubjectName, this.ConfluentValueSubjectName);
            if (this.AdditionalProperties != null)
            {
                foreach (var property in this.AdditionalProperties)
                {
                    if (!FixedFieldNames.Contains(property.Key))
                    {
                        writer.WritePropertyName(property.Key);
                        writer.WriteAny(property.Value);
                    }
                }
            }

            writer.WriteEndObject();
        }
    }
}
