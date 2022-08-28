using System.Threading.Tasks;

namespace NDiff.Services.Analyzers.SolutionAnalyzers
{
    public interface ISolutionAnalyzer
    {
        /// <summary>
        /// Analyzes all the projects of the solution given in the <see cref="basePath"/>.
        /// </summary>
        /// <param name="basePath">The base path of the solution.</param>
        /// <returns></returns>
        Task AnalyzeSolutionProjects(string basePath);
    }
}