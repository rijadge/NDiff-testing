using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;
using NDiff.ExtensionMethods;
using NDiff.Services.Generators;

namespace NDiff.Services.Analyzers.AttributeAnalyzers.Attributes.ContentTypes
{
    public class ProducesAnalyzer : ContentTypeAnalyzer
    {
        private bool _isPossibleOverriddenTypesAnalyzed;
        private OpenApiMediaType _producesOpenApiMedia;

        /// <summary>
        /// Contains the OpenApiMedia type that will be used as 200OK default response.
        /// If the AttributeData is not valid or there are no Types, it returns null; otherwise, the <see cref="OpenApiMediaType"/>
        /// instance for that type.
        /// </summary>
        public OpenApiMediaType ProducesOpenApiMedia
        {
            get
            {
                if (_isPossibleOverriddenTypesAnalyzed || !IsValid()) return _producesOpenApiMedia;
                
                FindPossibleOverriddenTypes();
                _isPossibleOverriddenTypesAnalyzed = true;

                return _producesOpenApiMedia;
            }
            private set => _producesOpenApiMedia = value;
        }

        public ProducesAnalyzer(AttributeData[] attributeData) : base(attributeData)
        {
        }

        /// <summary>
        /// Checks if the <see cref="AttributeData"/> contains NamedArguments with name Type that might
        /// override the <see cref="ProducesOpenApiMedia"/> found in constructor argument.
        /// </summary>
        /// <seealso cref="AnalyzeConstructorTypes"/>.
        private void FindPossibleOverriddenTypes()
        {
            foreach (var (key, value) in AttributeData[0].NamedArguments)
            {
                if (key == "Type")
                    ProducesOpenApiMedia = ContentGenerator.GenerateOpenApiMedia((ITypeSymbol) value.Value);
            }
        }

        protected override void AnalyzeConstructorTypes(TypedConstant constructorArgument)
        {
            base.AnalyzeConstructorTypes(constructorArgument);
            
            TryAnalyzeTypeKind(constructorArgument);
        }

        private void TryAnalyzeTypeKind(TypedConstant constructorArgument)
        {
            if (!constructorArgument.IsOfType(TypedConstantKind.Type)) return;

            ProducesOpenApiMedia = ContentGenerator.GenerateOpenApiMedia((ITypeSymbol) constructorArgument.Value);
        }

        protected override bool IsValid()
        {
            return AttributeData.Length is 1 && AttributeData[0].ConstructorArguments.Length is not 0;
        }
    }
}