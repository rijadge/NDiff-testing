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
    public class ProducesAttributeTest : GeneralDataProvider, IClassFixture<AttributeFixture>
    {
        private readonly AttributeFixture _attributeFixture;
        private const string AttributeName = "Produces";

        public ProducesAttributeTest(AttributeFixture attributeFixture)
        {
            _attributeFixture = attributeFixture;
        }

        [Theory]
        [InlineData(null, new string[] { })]
        [InlineData(new[] {MediaTypeNames.Text.Plain}, new[] {MediaTypeNames.Text.Plain})]
        [InlineData(new[] {MediaTypeNames.Text.Html}, new[] {MediaTypeNames.Text.Html})]
        public void AnalyzeProducesAttribute_MediaTypeNamesProvided_ReturnsMediaTypes(string[] input, string[] expected)
        {
            // arrange
            var sourceCode = CreateTestClassWithAttribute(AttributeName, classAttributeBody: AttributeBody(input)?[..^1]);
            _attributeFixture.SetupDocument(sourceCode);
            var classSymbol = _attributeFixture.GetSymbol<ClassDeclarationSyntax>(TestClassName);
            var producesAttributes = classSymbol.GetAttributesOfType(AttributeType.Produces).ToArray();
            var producesAnalyzer = new ProducesAnalyzer(producesAttributes);

            // act
            producesAnalyzer.AnalyzeAttributes();
            var mediaTypeCollection = producesAnalyzer.ContentTypes;

            // assert
            Assert.Equal(expected, mediaTypeCollection);
            Assert.Equal(expected.Length, mediaTypeCollection.Count);
        }

        [Theory]
        [InlineData("12312312")]
        [InlineData("incorrect,/MediaType", "Mr0NgF0rm@t")]
        [InlineData("@#$!!@#!")]
        public void AnalyzeProducesAttribute_IncorrectMediaTypeNames_ReturnsEmptyResult(params string[] input)
        {
            // arrange
            var sourceCode = CreateTestClassWithAttribute(AttributeName, classAttributeBody: AttributeBody(input)?[..^1]);
            _attributeFixture.SetupDocument(sourceCode);
            var classSymbol = _attributeFixture.GetSymbol<ClassDeclarationSyntax>(TestClassName);
            var producesAttributes = classSymbol.GetAttributesOfType(AttributeType.Produces).ToArray();
            var producesAnalyzer = new ProducesAnalyzer(producesAttributes);

            // act
            producesAnalyzer.AnalyzeAttributes();
            var mediaTypeCollection = producesAnalyzer.ContentTypes;

            // assert
            Assert.Empty(mediaTypeCollection);
        }


        [Theory]
        [InlineData("int", OpenApiSchemaType.Int)]
        [InlineData("string", OpenApiSchemaType.String)]
        [InlineData("bool", OpenApiSchemaType.Bool)]
        public void AnalyzeProducesAttribute_TypeOfExist_ReturnsDefault200OKOpenApiResponse(string typeName,
            OpenApiSchemaType expectedType)
        {
            // arrange
            var sourceCode = CreateProducesTypeOfSourceCode(typeName);
            _attributeFixture.SetupDocument(sourceCode);
            var classSymbol = _attributeFixture.GetSymbol<ClassDeclarationSyntax>(TestClassName);
            var producesAttributes = classSymbol.GetAttributesOfType(AttributeType.Produces).ToArray();
            var producesAnalyzer = new ProducesAnalyzer(producesAttributes);

            // act
            producesAnalyzer.AnalyzeAttributes();
            var openApiResponse = producesAnalyzer.ProducesOpenApiMedia;

            // assert
            Assert.NotNull(openApiResponse);
            Assert.NotNull(openApiResponse.Schema);
            Assert.Equal(expectedType.GetStringValue(), openApiResponse.Schema.Type);
        }


        #region ProducesCodeGeneration

        // Creates the Produces with typeof attribute.
        private string CreateProducesTypeOfSourceCode(string typeName)
        {
            var classTextAttribute = $@"typeof({typeName})";
            var a = 1..1;
            return CreateTestClassWithAttribute(AttributeName, classAttributeBody: classTextAttribute);
        }

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