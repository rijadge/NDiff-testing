using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NDiff.Enums;
using NDiff.ExtensionMethods;
using NDiff.Services.Analyzers.AttributeAnalyzers.Attributes.ResponseTypes;
using NDiff.UnitTests.Analyzers.Attributes.GeneralData;
using Xunit;

namespace NDiff.UnitTests.Analyzers.Attributes
{
    [Collection("Project builder collection")]
    public class CustomResponseTypeTest : GeneralDataProvider, IClassFixture<AttributeFixture>
    {
        private readonly AttributeFixture _attributeFixture;
        private const string AttributeNameForError = "ProducesErrorResponseType";

        public CustomResponseTypeTest(AttributeFixture attributeFixture)
        {
            _attributeFixture = attributeFixture;
        }

        [Theory]
        [InlineData("string", OpenApiSchemaType.String)]
        [InlineData("object", OpenApiSchemaType.Object)]
        [InlineData("int", OpenApiSchemaType.Int)]
        public void AnalyzeCustomResponseType_ErrorTypeProvided_ReturnsType(string typeName, OpenApiSchemaType expectedType)
        {
            // arrange
            var sourceCode = CreateResponseTypeOfSourceCode(typeName);
            _attributeFixture.SetupDocument(sourceCode);
            var classSymbol = _attributeFixture.GetSymbol<ClassDeclarationSyntax>(TestClassName);
            var errorResponseTypeAttribute =
                classSymbol.GetAttributesOfType(AttributeType.ProducesErrorResponseType).ToArray();
            var customResponseTypeAnalyzer = new CustomResponseTypeAnalyzer(errorResponseTypeAttribute);

            // act
            customResponseTypeAnalyzer.AnalyzeAttributes();

            // assert
            Assert.NotNull(customResponseTypeAnalyzer.OpenApiMedia);
            Assert.Equal(customResponseTypeAnalyzer.OpenApiMedia.Schema.Type, expectedType.GetStringValue());
        }

        #region CustomResponseTypeCodeGeneration

        // Creates the Produces with typeof attribute.
        private string CreateResponseTypeOfSourceCode(string typeName)
        {
            var classTextAttribute = $@"typeof({typeName})";

            return CreateTestClassWithAttribute(AttributeNameForError, classAttributeBody: classTextAttribute);
        }

        #endregion

    }
}