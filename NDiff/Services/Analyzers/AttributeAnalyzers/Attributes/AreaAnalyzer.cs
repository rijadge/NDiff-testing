using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;

namespace NDiff.Services.Analyzers.AttributeAnalyzers.Attributes
{
    public class AreaAnalyzer : Attribute
    {
        /// <summary>
        /// Contains the Area value. In case the <see cref="AttributeData"/> is not valid, it returns <see cref="string.Empty"/>.
        /// </summary>
        public string AreaValue { get; set; } = "";
        
        public AreaAnalyzer(AttributeData[] attributeData) : base(attributeData)
        {
        }
        
        /// <summary>
        /// Analyzes the <see cref="AreaAttribute"/> and finds the string that will be used in <see cref="RouteAttribute"/>.
        /// This can be overridden from other <see cref="AreaAttribute"/> of methods.
        /// </summary>
        protected override void Analyze()
        {
            AreaValue = AttributeData[0].ConstructorArguments[0].Value?.ToString();
        }

        protected override bool IsValid()
        {
            return AttributeData.Length is 1 && AttributeData[0].ConstructorArguments.Length is 1 &&
                   AttributeData[0].ConstructorArguments[0].Kind is TypedConstantKind.Primitive;
        }
    }
}