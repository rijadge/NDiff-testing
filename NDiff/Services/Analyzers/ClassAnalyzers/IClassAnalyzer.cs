using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NDiff.Services.Analyzers.ClassAnalyzers
{
    public interface IClassAnalyzer
    {
        /// <summary>
        /// Recursive method to go through all class nodes and analyze them.
        /// </summary>
        /// <param name="classSymbol">Symbol of class.</param>
        /// <param name="classDeclarationSyntax">Syntax tree of class.</param>
        void AnalyzeClass(ITypeSymbol classSymbol, ClassDeclarationSyntax classDeclarationSyntax);
    }
}