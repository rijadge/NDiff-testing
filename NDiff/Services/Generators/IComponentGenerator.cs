using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;

namespace NDiff.Services.Generators
{
    public interface IComponentGenerator
    {
        /// <summary>
        /// Generates the components for all referenced classes throughout the application.
        /// </summary>
        /// <returns>A <see cref="OpenApiComponents"/> instance.</returns>
        public OpenApiComponents GenerateComponents();

        /// <summary>
        /// Checks if the symbol has already been analyzed.
        /// </summary>
        /// <param name="symbol">The symbol to check.</param>
        /// <returns>True if analyzed; otherwise, false.</returns>
        public bool AnalyzedTypesContains(ITypeSymbol symbol);

        /// <summary>
        /// Adds the symbol for the next analysis iteration.
        /// </summary>
        /// <param name="symbol">The symbol to be added.</param>
        public void AddToNextAnalyzedTypes(ITypeSymbol symbol);
    }
}