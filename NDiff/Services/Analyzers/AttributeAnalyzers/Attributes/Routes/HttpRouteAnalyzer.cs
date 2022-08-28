using Microsoft.CodeAnalysis;

namespace NDiff.Services.Analyzers.AttributeAnalyzers.Attributes.Routes
{
    public class HttpRouteAnalyzer : RouteAnalyzer
    {
        public HttpRouteAnalyzer(AttributeData[] attributeData) : base(attributeData)
        {
        }

        /// <summary>
        /// Checks if the HttpXXXX route is valid.
        /// </summary>
        /// <param name="attributeData">The attribute.</param>
        /// <returns>True if the number of constructor arguments is 0 or 1 and the number of arguments is 0, 1, or 2. Otherwise, false.</returns>
        protected override bool IsValidAttribute(AttributeData attributeData)
        {
            return attributeData.ConstructorArguments.Length is 0 or 1 &&
                   attributeData.NamedArguments.Length is 0 or 1 or 2;
        }
    }
}