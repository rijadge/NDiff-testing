using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NDiff.Enums;
using NDiff.ExtensionMethods;
using NDiff.Services.Analyzers.AttributeAnalyzers.Attributes;
using NDiff.UnitTests.Analyzers.Attributes.GeneralData;
using Xunit;

namespace NDiff.UnitTests.Analyzers.Attributes
{
    [Collection("Project builder collection")]
    public class AreaAttributeTest : GeneralDataProvider, IClassFixture<AttributeFixture>
    {
        private readonly AttributeFixture _attributeFixture;
        private const string AttributeName = "Area";

        public AreaAttributeTest(AttributeFixture attributeFixture)
        {
            _attributeFixture = attributeFixture;
        }

        [Theory]
        [InlineData(null, "")]
        [InlineData("products", "products")]
        [InlineData("UPPERCASE", "UPPERCASE")]
        public void AnalyzeAreaAttribute_AttributeExists_ReturnsAttributeValue(string input, string expected)
        {
            // arrange
            var sourceCode = CreateAreaAttributeSourceCode(input);
            _attributeFixture.SetupDocument(sourceCode);
            var classSymbol = _attributeFixture.GetSymbol<ClassDeclarationSyntax>(TestClassName);
            var areaAttributes = classSymbol.GetAttributesOfType(AttributeType.Area).ToArray();
            var areaAnalyzer = new AreaAnalyzer(areaAttributes);

            // act
            areaAnalyzer.AnalyzeAttributes();

            // assert
            Assert.Equal(expected, areaAnalyzer.AreaValue);
        }


        #region AreaCodeGeneration

        private string CreateAreaAttributeSourceCode(string classAreaAttribute = null)
        {
            var classTextAttribute = classAreaAttribute is not null ? $@"""{classAreaAttribute}""" : "";

            return CreateTestClassWithAttribute(AttributeName, classAttributeBody: classTextAttribute);
        }

        #endregion
    }
}