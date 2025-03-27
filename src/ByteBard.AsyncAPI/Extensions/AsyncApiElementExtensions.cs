namespace ByteBard.AsyncAPI.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Services;
    using ByteBard.AsyncAPI.Validations;

    /// <summary>
    /// Extension methods that apply across all AsyncAPIElements.
    /// </summary>
    public static class AsyncApiElementExtensions
    {
        /// <summary>
        /// Validate element and all child elements.
        /// </summary>
        /// <param name="element">Element to validate.</param>
        /// <param name="ruleSet">Optional set of rules to use for validation.</param>
        /// <returns>An IEnumerable of errors.  This function will never return null.</returns>
        public static IEnumerable<AsyncApiError> Validate(this IAsyncApiElement element, ValidationRuleSet ruleSet)
        {
            var validator = new AsyncApiValidator(ruleSet);
            var walker = new AsyncApiWalker(validator);
            walker.Walk(element);
            return validator.Errors.Cast<AsyncApiError>().Union(validator.Warnings);
        }
    }
}
