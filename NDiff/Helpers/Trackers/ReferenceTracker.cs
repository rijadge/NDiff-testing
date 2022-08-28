using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace NDiff.Helpers.Trackers
{
    public static class ReferenceTracker
    {
        /// <summary>
        /// Keeps track of all types that were referenced during analysis of method parameters and class/method attributes.
        /// </summary>
        public static HashSet<ITypeSymbol> ReferencedTypes { get; set; } = new HashSet<ITypeSymbol>();

        public static void AddIfNotExists(ITypeSymbol typeSymbol)
        {
            if (!ReferencedTypes.Contains(typeSymbol))
                ReferencedTypes.Add(typeSymbol);
        }
    }
}