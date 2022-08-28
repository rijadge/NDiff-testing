using System.Linq;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.CodeAnalysis;
using Microsoft.Net.Http.Headers;
using NDiff.ExtensionMethods;

namespace NDiff.Services.Analyzers.AttributeAnalyzers.Attributes.ContentTypes
{
    /// <summary>
    /// Contains the main definitions for <see cref="ProducesAnalyzer"/>, <see cref="ConsumesAnalyzer"/>, and.
    /// </summary>
    public abstract class ContentTypeAnalyzer : Attribute
    {
        /// <summary>
        /// Contains the ContentTypes. In case of errors <see cref="AttributeData"/> returns empty. 
        /// </summary>
        /// <seealso cref="Analyze"/>.
        public MediaTypeCollection ContentTypes { get; set; } = new MediaTypeCollection();

        public ContentTypeAnalyzer(AttributeData[] attributeData) : base(attributeData)
        {
        }


        /// <summary>
        /// Analyzes the attributes and generates the <see cref="ContentTypes"/>.
        /// In case the attributes are not valid the <see cref="ContentTypes"/> returns empty.
        /// </summary>
        protected override void Analyze()
        {
            foreach (var attributeData in AttributeData)
            {
                foreach (var constructorArgument in attributeData.ConstructorArguments)
                {
                    AnalyzeConstructorTypes(constructorArgument);
                }
            }
        }

        /// <summary>
        /// Analyzes the constructor argument kinds <see cref="TypedConstantKind"/>.
        /// </summary>
        /// <param name="constructorArgument">The <see cref="TypedConstant"/> that is to be analyzed.</param>
        protected virtual void AnalyzeConstructorTypes(TypedConstant constructorArgument)
        {
            TryAnalyzePrimitiveKind(constructorArgument);
            TryAnalyzeArrayKind(constructorArgument);
        }

        private void TryAnalyzePrimitiveKind(TypedConstant constructorArgument)
        {
            if (!constructorArgument.IsOfType(TypedConstantKind.Primitive)) return;

            UpdateContent(constructorArgument.Value?.ToString());
        }

        private void TryAnalyzeArrayKind(TypedConstant constructorArgument)
        {
            if (!constructorArgument.IsOfType(TypedConstantKind.Array)) return;

            UpdateContent(constructorArgument.Values.Select(typedConstant => typedConstant.Value?.ToString())
                .ToArray());
        }

        /// <summary>
        /// Updates the <see cref="ContentTypes"/>.
        /// </summary>
        /// <param name="contentTypes">An array of strings that contains the content types.</param>
        private void UpdateContent(params string[] contentTypes)
        {
            foreach (var contentType in contentTypes)
            {
                MediaTypeHeaderValue.Parse(contentType);
                ContentTypes.Add(contentType);
            }
        }
    }
}