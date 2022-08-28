using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using NDiff.ExtensionMethods;
using NDiff.Services.Analyzers.FileAnalyzers;

namespace NDiff.Services.Analyzers.ProjectAnalyzers
{
    public class ProjectAnalyzer : IProjectAnalyzer
    {
        private readonly IClassFileAnalyzer _classFileAnalyzer;

        public ProjectAnalyzer(IClassFileAnalyzer classFileAnalyzer)
        {
            _classFileAnalyzer = classFileAnalyzer;
        }

        public void AnalyzeProject(Project project)
        {
            var classFiles = GetClassFiles(project);

            foreach (var classFile in classFiles)
            {
                _classFileAnalyzer.AnalyzeClassFile(classFile);
            }
        }

        /// <summary>
        /// Returns all relevant class files.
        /// </summary>
        /// <param name="project">The project from which the class files are read.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Document"/> that represents the class file.</returns>
        private static IEnumerable<Document> GetClassFiles(Project project)
        {
            var directories = new[]
                {"obj", "bin", "release", "releases", "debugpublic", "debug", "x64", "x86", "build", "bld"};

            return project.Documents.NotInFolderIgnoreCase(directories);
        }
    }
}