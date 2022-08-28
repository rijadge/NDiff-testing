using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;
using NDiff.Services.Generators;

namespace NDiff.Services.Analyzers.AttributeAnalyzers.Attributes.ResponseTypes
{
    /// <summary>
    /// This class is valid for ProducesErrorResponseType and ProducesDefaultResponseType.
    /// </summary>
    public class CustomResponseTypeAnalyzer : Attribute
    {
        /// <summary>
        /// Contains the OpenApiMedia type that was returned after analyzing the AttributeData.
        /// If the AttributeData is not valid or there are no Types, it returns null; otherwise, the <see cref="OpenApiMediaType"/>
        /// instance for that type.
        /// </summary>
        public OpenApiMediaType OpenApiMedia { get; set; }

        public CustomResponseTypeAnalyzer(AttributeData[] attributeData) : base(attributeData)
        {
        }

        protected override void Analyze()
        {
            OpenApiMedia =
                ContentGenerator.GenerateOpenApiMedia((ITypeSymbol) AttributeData[0].ConstructorArguments[0].Value);
        }

        protected override bool IsValid()
        {
            if (AttributeData.Length is not 1) return false;

            return AttributeData[0].ConstructorArguments.Length is 1 &&
                   AttributeData[0].ConstructorArguments[0].Kind is TypedConstantKind.Type;
        }
    }
}