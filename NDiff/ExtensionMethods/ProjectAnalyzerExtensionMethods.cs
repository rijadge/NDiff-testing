using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace NDiff.ExtensionMethods
{
    public static class ProjectAnalyzerExtensionMethods
    {
        /// <summary>
        /// Ignores class files part of directories provided in <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="enumerable"></param>
        /// <param name="directories">Directories to ignore.</param>
        /// <returns>Class files <see cref="Document"/> not part of <see cref="directories"/>.</returns>
        public static IEnumerable<Document> NotInFolderIgnoreCase(this IEnumerable<Document> enumerable,
            IEnumerable<string> directories)
        {
            return directories.Aggregate(enumerable,
                (current, directory) =>
                    current.Where(el => !el.Folders.Contains(directory, StringComparer.OrdinalIgnoreCase)));
        }
    }
}