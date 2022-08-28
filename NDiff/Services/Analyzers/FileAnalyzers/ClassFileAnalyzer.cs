using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NDiff.ExtensionMethods;
using NDiff.Services.Analyzers.ClassAnalyzers;

namespace NDiff.Services.Analyzers.FileAnalyzers
{
    public class ClassFileAnalyzer : IClassFileAnalyzer
    {
        private readonly IClassAnalyzer _classAnalyzer;

        public ClassFileAnalyzer(IClassAnalyzer classAnalyzer)
        {
            _classAnalyzer = classAnalyzer;
        }

        public void AnalyzeClassFile(Document classFile)
        {
            var classFileTree = classFile.GetSyntaxTreeAsync().Result;
            var semanticModel = classFile.GetSemanticModelAsync().Result;

            var classDeclarationSyntaxes = ClassDeclarationSyntaxes(classFileTree);

            foreach (var classDeclarationSyntax in classDeclarationSyntaxes)
            {
                var classSymbol = semanticModel?.GetDeclaredSymbol(classDeclarationSyntax) as ITypeSymbol;
                _classAnalyzer.AnalyzeClass(classSymbol, classDeclarationSyntax);
            }
        }

        /// <summary>
        /// Reads all the <see cref="ClassDeclarationSyntax"/>. It does not take into account nested classes
        /// because they are not considered controllers. It also filters only those classes that can be controllers.
        /// </summary>
        /// <param name="classFileTree">The <see cref="SyntaxTree"/> of the class file being analyzed.</param>
        /// <returns></returns>
        private static IEnumerable<ClassDeclarationSyntax> ClassDeclarationSyntaxes(SyntaxTree classFileTree)
        {
            return classFileTree?.GetRoot()
                .DescendantNodesAndSelf()
                .OfType<ClassDeclarationSyntax>()
                .IsNotNestedClass()
                .IsValidControllerClass();
        }
    }
}