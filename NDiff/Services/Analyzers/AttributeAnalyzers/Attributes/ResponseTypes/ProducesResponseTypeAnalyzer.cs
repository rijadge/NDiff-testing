using System.Collections.Generic;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;
using NDiff.ExtensionMethods;
using NDiff.Services.Generators;

namespace NDiff.Services.Analyzers.AttributeAnalyzers.Attributes.ResponseTypes
{
    /// <summary>
    /// Analyzes the ProducesResponseType attribute.
    /// </summary>
    public class ProducesResponseTypeAnalyzer : Attribute
    {
        /// <summary>
        /// A <see cref="Dictionary{TKey,TValue}"/> where key is the status code whereas the value is an <see cref="OpenApiMediaType"/> instance.
        /// The <see cref="OpenApiMediaType.Schema"/> value will be matched with Content Types.
        /// </summary>
        /// <seealso cref="Analyze"/>.
        internal Dictionary<int, OpenApiMediaType> MediatTypesForStatusCodes = new();

        public ProducesResponseTypeAnalyzer(AttributeData[] attributeData) : base(attributeData)
        {
        }

        protected override void Analyze()
        {
            foreach (var attributeData in AttributeData)
            {
                if (!IsValidAttribute(attributeData)) continue;

                var statusCode = -1;
                OpenApiMediaType openApiMedia = null;

                foreach (var constructorArgument in attributeData.ConstructorArguments)
                {
                    if (constructorArgument.IsOfType(TypedConstantKind.Primitive))
                    {
                        statusCode = (int) (constructorArgument.Value ?? -1);
                    }
                    else if (constructorArgument.IsOfType(TypedConstantKind.Type))
                    {
                        openApiMedia = ContentGenerator.GenerateOpenApiMedia((ITypeSymbol) constructorArgument.Value);
                    }
                }

                if (!IsStatusCodeValid(statusCode)) break;

                MediatTypesForStatusCodes[statusCode] = openApiMedia;
            }
        }

        /// <summary>
        /// Checks if the status code of ProduceResponseType attribute is Valid.
        /// </summary>
        private static bool IsStatusCodeValid(int statusCode)
        {
            return !string.IsNullOrEmpty(ReasonPhrases.GetReasonPhrase(statusCode));
        }

        /// <summary>
        /// Checks if the ProducesResponseType attribute is a valid and contains the supported constructor arguments.
        /// </summary>
        /// <param name="attributeData">Contains the <see cref="AttributeData"/> of ProducesResponseType attribute.</param>
        /// <returns>True if is valid; otherwise, false.</returns>
        private static bool IsValidAttribute(AttributeData attributeData)
        {
            return attributeData.ConstructorArguments.Length switch
            {
                1 => attributeData.ConstructorArguments[0].Kind is TypedConstantKind.Primitive,
                2 => (attributeData.ConstructorArguments[0].Kind is TypedConstantKind.Primitive &&
                      attributeData.ConstructorArguments[1].Kind is TypedConstantKind.Type) ||
                     (attributeData.ConstructorArguments[0].Kind is TypedConstantKind.Type &&
                      attributeData.ConstructorArguments[1].Kind is TypedConstantKind.Primitive),
                _ => false
            };
        }

        protected override bool IsValid() => true;
    }
}