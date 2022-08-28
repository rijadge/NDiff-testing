using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NDiff.UnitTests.Helpers;
using Xunit;

namespace NDiff.UnitTests.Analyzers.Attributes
{
    /// <summary>
    /// Used for updating the same <see cref="Document"/> for the all tests inside a test class.
    /// </summary>
    [Collection("Project builder collection")]
    public class AttributeFixture : IDisposable
    {
        private const string ClassFilePathToAnalyze = "AttributesTestClass";
        private readonly ProjectBuilderFixture _projectBuilderFixture;
        private Document _document;

        public AttributeFixture(ProjectBuilderFixture projectBuilderFixture)
        {
            _projectBuilderFixture = projectBuilderFixture;
        }

        /// <summary>
        /// Sets up a new <see cref="Document"/> if it does not exist. Otherwise, it updates the existing one. 
        /// </summary>
        /// <param name="sourceCode">Source code used to update the <see cref="Document"/> (file).</param>
        public void SetupDocument(string sourceCode)
        {
            _document = _document == null
                ? _projectBuilderFixture.InitializeDocument(ClassFilePathToAnalyze, sourceCode)
                : _document.WithText(SourceText.From(sourceCode));
        }

        /// <summary>
        /// Used to retrieve an <see cref="ITypeSymbol"/>.
        /// </summary>
        /// <param name="name">The identifier name of the symbol.</param>
        /// <typeparam name="T">The Type to search in <see cref="SyntaxNode"/>.</typeparam>
        /// <returns>An instance of type <see cref="ITypeSymbol"/>.</returns>
        public ITypeSymbol GetSymbol<T>(string name) where T : TypeDeclarationSyntax
        {
            var classFileTree = _document.GetSyntaxTreeAsync().Result;
            var semanticModel = _document.GetSemanticModelAsync().Result;

            var classSyntax = classFileTree?.GetRoot()
                .DescendantNodesAndSelf()
                .OfType<T>()
                .Single(syntax => syntax.Identifier.Text == name);

            return semanticModel?.GetDeclaredSymbol(classSyntax) as ITypeSymbol;
        }

        public void Dispose()
        {
        }
    }
}