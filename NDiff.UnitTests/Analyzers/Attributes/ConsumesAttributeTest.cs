using System.Linq;
using System.Net.Mime;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NDiff.Enums;
using NDiff.ExtensionMethods;
using NDiff.Services.Analyzers.AttributeAnalyzers.Attributes.ContentTypes;
using NDiff.UnitTests.Analyzers.Attributes.GeneralData;
using Xunit;

namespace NDiff.UnitTests.Analyzers.Attributes
{
    [Collection("Project builder collection")]
    public class ConsumesAttributeTest : GeneralDataProvider, IClassFixture<AttributeFixture>
    {
        private readonly AttributeFixture _attributeFixture;
        private const string AttributeName = "Consumes";


        public ConsumesAttributeTest(AttributeFixture attributeFixture)
        {
            _attributeFixture = attributeFixture;
        }

        [Theory]
        [InlineData(null, new string[] { })]
        [InlineData(new[] {MediaTypeNames.Text.Plain}, new[] {MediaTypeNames.Text.Plain})]
        [InlineData(new[] {MediaTypeNames.Text.Html, MediaTypeNames.Application.Json},
            new[] {MediaTypeNames.Text.Html, MediaTypeNames.Application.Json})]
        public void AnalyzeConsumesAttribute_MediaTypeNamesProvided_ReturnsMediaTypes(string[] input, string[] expected)
        {
            // arrange
            var sourceCode = CreateTestClassWithAttribute(AttributeName, classAttributeBody: AttributeBody(input)?[..^1]);
            _attributeFixture.SetupDocument(sourceCode);
            var classSymbol = _attributeFixture.GetSymbol<ClassDeclarationSyntax>(TestClassName);
            var consumesAttribute = classSymbol.GetAttributesOfType(AttributeType.Consumes).ToArray();
            var consumesAnalyzer = new ConsumesAnalyzer(consumesAttribute);

            // act
            consumesAnalyzer.AnalyzeAttributes();
            var mediaTypeCollection = consumesAnalyzer.ContentTypes;

            // assert
            Assert.Equal(expected, mediaTypeCollection);
            Assert.Equal(expected.Length, mediaTypeCollection.Count);
        }

        #region ConsumesCodeGeneration
        
        private string AttributeBody(string[] input)
        {
            var attributeBody = "";
            attributeBody =
                input?.Aggregate(attributeBody, (current, contentType) => current + $@"""{contentType}"",");

            return attributeBody;
        }
        
        #endregion
        
    }
}