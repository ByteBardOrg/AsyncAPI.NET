﻿// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Writers;

    /// <summary>
    /// An object representing a Server Variable for server URL template substitution.
    /// </summary>
    public class AsyncApiServerVariable : IAsyncApiSerializable, IAsyncApiExtensible
    {
        /// <summary>
        /// An enumeration of string values to be used if the substitution options are from a limited set.
        /// </summary>
        public virtual IList<string> Enum { get; set; } = new List<string>();

        /// <summary>
        /// The default value to use for substitution, and to send, if an alternate value is not supplied.
        /// </summary>
        public virtual string Default { get; set; }

        /// <summary>
        /// An optional description for the server variable. CommonMark syntax MAY be used for rich text representation.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// An array of examples of the server variable.
        /// </summary>
        public virtual IList<string> Examples { get; set; } = new List<string>();

        /// <inheritdoc/>
        public virtual IDictionary<string, IAsyncApiExtension> Extensions { get; set; } = new Dictionary<string, IAsyncApiExtension>();

        public virtual void SerializeV2(IAsyncApiWriter writer)
        {
            this.SerializeCore(writer);
        }

        public virtual void SerializeV3(IAsyncApiWriter writer)
        {
            this.SerializeCore(writer);
        }

        private void SerializeCore(IAsyncApiWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteStartObject();

            // enums
            writer.WriteOptionalCollection(AsyncApiConstants.Enum, this.Enum, (w, s) => w.WriteValue(s));

            // default
            writer.WriteOptionalProperty(AsyncApiConstants.Default, this.Default);

            // description
            writer.WriteOptionalProperty(AsyncApiConstants.Description, this.Description);

            // examples
            writer.WriteOptionalCollection(AsyncApiConstants.Examples, this.Examples, (w, e) => w.WriteValue(e));

            // specification extensions
            writer.WriteExtensions(this.Extensions);

            writer.WriteEndObject();
        }
    }
}