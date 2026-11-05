namespace ByteBard.AsyncAPI.Bindings
{
    using System;
    using System.Collections.Generic;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Models.Interfaces;

    /// <summary>
    /// The binding names AsyncAPI 3 defines, and the key a binding is written under in an AsyncAPI 3 document.
    /// </summary>
    /// <remarks>
    /// In AsyncAPI 3 the server, channel, operation and message bindings objects only allow the protocol names
    /// the specification lists, plus <c>x-</c> extensions. A custom binding (one whose <see cref="IBinding.BindingKey"/>
    /// is not listed) is therefore written as <c>x-&lt;key&gt;</c>. AsyncAPI 2 allows any key, so it keeps the key as is.
    /// </remarks>
    public static class AsyncApiBindingKeys
    {
        // Every binding the AsyncAPI 3.0 and 3.1 specifications name in a bindings object. The prose is normative; the
        // JSON schemas omit mqtt5 and mercure, but the specification text lists them as fixed fields.
        private static readonly HashSet<string> SpecificationKeys = new HashSet<string>(StringComparer.Ordinal)
        {
            "amqp", "amqp1", "anypointmq", "googlepubsub", "http", "ibmmq", "jms", "kafka", "mercure", "mqtt", "mqtt5",
            "nats", "pulsar", "redis", "ros2", "sns", "solace", "sqs", "stomp", "ws",
        };

        /// <summary>
        /// Whether AsyncAPI 3 defines a binding with this name.
        /// </summary>
        /// <param name="key">The binding key.</param>
        /// <returns><c>true</c> for a binding the specification lists; <c>false</c> otherwise, including for <c>x-</c> keys.</returns>
        public static bool IsSpecificationKey(string key)
        {
            return key != null && SpecificationKeys.Contains(key);
        }

        /// <summary>
        /// The property name a binding is written under in an AsyncAPI 3 bindings object.
        /// </summary>
        /// <param name="key">The binding key.</param>
        /// <returns>The key itself for a specification binding or an <c>x-</c> key; otherwise the key with an <c>x-</c> prefix.</returns>
        public static string ToV3Key(string key)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return IsSpecificationKey(key) || key.StartsWith(AsyncApiConstants.ExtensionFieldNamePrefix, StringComparison.Ordinal)
                ? key
                : AsyncApiConstants.ExtensionFieldNamePrefix + key;
        }
    }
}
