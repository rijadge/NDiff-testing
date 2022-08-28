using Microsoft.CodeAnalysis;

namespace NDiff.Services.Analyzers.AttributeAnalyzers.Attributes.ContentTypes
{
    public class ConsumesAnalyzer : ContentTypeAnalyzer
    {
        public ConsumesAnalyzer(AttributeData[] attributeData) : base(attributeData)
        {
        }

        protected override bool IsValid()
        {
            return AttributeData.Length is 1 && AttributeData[0].ConstructorArguments.Length is 1 or 2;
        }
    }
}