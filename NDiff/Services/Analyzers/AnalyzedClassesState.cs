using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using NDiff.Models;

namespace NDiff.Services.Analyzers
{
    public class AnalyzedClassesState : IAnalyzedClassesState
    {
        private IDictionary<ITypeSymbol, ClassInformation> AnalyzedClasses { get; } =
#pragma warning disable RS1024
            new Dictionary<ITypeSymbol, ClassInformation>();
#pragma warning restore RS1024

        
        /// <summary>
        /// Adds a class to the <see cref="AnalyzedClasses"/>.
        /// </summary>
        /// <param name="symbolKey">The key.</param>
        /// <param name="classValue">The value.</param>
        public void AddAnalyzedClass(ITypeSymbol symbolKey, ClassInformation classValue)
        {
            AnalyzedClasses.Add(symbolKey, classValue);
        }

        /// <summary>
        /// Tries to get a <see cref="ClassInformation"/> base on the key.
        /// </summary>
        /// <param name="symbolKey">Key to look for.</param>
        /// <param name="classValue">The value as output parameter.</param>
        /// <returns>True if the key exists; otherwise, false.</returns>
        public bool TryGetAnalyzedClassValue(ITypeSymbol symbolKey, out ClassInformation classValue)
        {
            return AnalyzedClasses.TryGetValue(symbolKey, out classValue);
        }

        /// <summary>
        /// Checks if the class is analyzed.
        /// </summary>
        /// <param name="classSymbol">The key to check if it has been analyzed.</param>
        /// <returns>True if class is analyzed; otherwise, false.</returns>
        public bool IsClassAlreadyAnalyzed(ITypeSymbol classSymbol)
        {
            return AnalyzedClasses.ContainsKey(classSymbol);
        }

        /// <summary>
        /// Reads all the <see cref="AnalyzedClasses"/> values.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="ClassInformation"/> with all analyzed classes.</returns>
        public IEnumerable<ClassInformation> GetStateValues()
        {
            return AnalyzedClasses.Values;
        }
    }
}