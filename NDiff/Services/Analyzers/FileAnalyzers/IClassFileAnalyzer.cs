using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using NDiff.Models;

namespace NDiff.Services.Analyzers.FileAnalyzers
{
    public interface IClassFileAnalyzer
    {
        /// <summary>
        /// Analyzes one specific .cs file represented as <see cref="Document"/>.
        /// </summary>
        /// <param name="classFile">The class file <see cref="Document"/>.</param>
        void AnalyzeClassFile(Document classFile);
    }
}