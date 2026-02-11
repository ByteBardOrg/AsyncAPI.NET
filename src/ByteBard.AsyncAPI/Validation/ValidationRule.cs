namespace ByteBard.AsyncAPI.Validations
{
    using System;

    /// <summary>
    /// Class containing validation rule logic.
    /// </summary>
    public abstract class ValidationRule
    {
        /// <summary>
        /// Element Type.
        /// </summary>
        internal abstract Type ElementType { get; }

        /// <summary>
        /// Gets or sets the AsyncAPI versions this rule applies to.
        /// Null means the rule applies to all versions.
        /// </summary>
        public AsyncApiVersion[] ApplicableVersions { get; internal set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="item">The object item.</param>
        internal abstract void Evaluate(IValidationContext context, object item);
    }
}
