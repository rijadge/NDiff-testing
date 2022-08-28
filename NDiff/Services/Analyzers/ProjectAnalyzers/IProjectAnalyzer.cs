using Microsoft.CodeAnalysis;

namespace NDiff.Services.Analyzers.ProjectAnalyzers
{
    public interface IProjectAnalyzer
    {
        /// <summary>
        /// Analyzes the project.
        /// </summary>
        /// <param name="project">Project to be analyzed.</param>
        void AnalyzeProject(Project project);
    }
}