using Microsoft.AspNetCore.WebUtilities;
using Microsoft.CodeAnalysis;

namespace NDiff.ExtensionMethods
{
    public static class TypedConstantExtensionMethods
    {
        /// <summary>
        /// Checks if the <see cref="TypedConstant"/> is an argument of a given type.
        /// </summary>
        /// <param name="argument">The <see cref="TypedConstant"/> instance.</param>
        /// <param name="type">The <see cref="TypedConstantKind"/> to check.</param>
        /// <returns></returns>
        public static bool IsOfType(this TypedConstant argument, TypedConstantKind type)
        {
            return argument.Kind == type;
        }
        
        /// <summary>
        /// Checks if the status code of ProduceResponseType attribute is Valid.
        /// </summary>
        /// <param name="argument">Argument of status code.</param>
        /// <returns>True if status code is valid, otherwise, false.</returns>
        public static bool IsStatusCodeValid(this TypedConstant argument)
        {
            if (argument.Value == null) return false;

            var statusCodeValue = (int) argument.Value;

            return !string.IsNullOrEmpty(ReasonPhrases.GetReasonPhrase(statusCodeValue));
        }

        /// <summary>
        /// Checks if the status code of ProduceResponseType attribute is Valid.
        /// </summary>
        /// <param name="argument">Argument of status code.</param>
        /// <param name="statusCode">Outputs the status code as integer.</param>
        /// <param name="statusDescription">Outputs the status description generated from <see cref="ReasonPhrases"/>.</param>
        /// <returns>True if status code is valid (and outputs the <see cref="statusCode"/> and <see cref="statusDescription"/>), otherwise, false.</returns>
        public static bool TryGetStatusDescription(this TypedConstant argument, out int statusCode,
            out string statusDescription)
        {
            statusCode = (int) argument.Value;
            statusDescription = ReasonPhrases.GetReasonPhrase(statusCode);

            return !string.IsNullOrEmpty(statusDescription);
        }
    }
}