using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using NDiff.Models;

namespace NDiff.Services.Analyzers
{
    /// <summary>
    /// A Singleton service that keeps the state of analyzed classes and provides methods to access it.
    /// Only one instance of this class is created, therefore the state is shared and is the same in all
    /// services that inject it.
    /// </summary>
    public interface IAnalyzedClassesState
    {
        /// <summary>
        /// Reads all the values of the <see cref="AnalyzedClassesState.AnalyzedClasses"/> that have been analyzed and are ready to be generated.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="ClassInformation"/>.</returns>
        IEnumerable<ClassInformation> GetStateValues();

        /// <summary>
        /// Adds to the <see cref="AnalyzedClassesState.AnalyzedClasses"/> the class symbol as key and the <see cref="ClassInformation"/> as value.
        /// </summary>
        /// <param name="symbolKey">The symbol to add as key.</param>
        /// <param name="classValue">The analyzed class value.</param>
        void AddAnalyzedClass(ITypeSymbol symbolKey, ClassInformation classValue);

        /// <summary>
        /// It tries to read the analyzed class value (<see cref="ClassInformation"/>) from the already <see cref="AnalyzedClassesState.AnalyzedClasses"/>.
        /// </summary>
        /// <param name="key">The symbol from which we want to get the analyzed information.</param>
        /// <param name="value">The value that will be returned in case one is found.</param>
        /// <returns>True, if the symbol is analyzed; otherwise, false.</returns>
        bool TryGetAnalyzedClassValue(ITypeSymbol key, out ClassInformation value);

        /// <summary>
        /// Checks if the class is analyzed.
        /// </summary>
        /// <param name="classSymbol"></param>
        /// <returns>True if analyzed, otherwise, false.</returns>
        public bool IsClassAlreadyAnalyzed(ITypeSymbol classSymbol);
    }
}