using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using NDiff.Action;
using NDiff.Helpers;
using NDiff.Services.Analyzers.ProjectAnalyzers;
using NDiff.Services.Generators;

namespace NDiff.Services.Analyzers.SolutionAnalyzers
{
    public class SolutionAnalyzer : SolutionProvider, ISolutionAnalyzer
    {
        private readonly IProjectAnalyzer _projectAnalyzer;
        private readonly OpenApiGenerator _openApiGenerator;
        private readonly IAnalyzedClassesState _analyzedClassesState;

        public SolutionAnalyzer(IProjectAnalyzer projectAnalyzer, OpenApiGenerator openApiGenerator,
            IAnalyzedClassesState analyzedClassesState)
        {
            _projectAnalyzer = projectAnalyzer;
            _openApiGenerator = openApiGenerator;
            _analyzedClassesState = analyzedClassesState;
        }

        public async Task AnalyzeSolutionProjects(string basePath, ActionInputs inputs)
        {
            await LoadSolutionProjects(basePath);

            if (Projects == null || Projects.Count == 0)
                return;

            foreach (var project in Projects)
            {
                _projectAnalyzer.AnalyzeProject(project);
            }
            var paths = new OpenApiPaths();
            foreach (var analyzedClassesValue in _analyzedClassesState.GetStateValues())
            {
                _openApiGenerator.ClassInformation = analyzedClassesValue;
                _openApiGenerator.GenerateOpenApi(paths); 
            }
            _openApiGenerator.CreateDocument(paths, inputs.Directory);

        }
    }
}
