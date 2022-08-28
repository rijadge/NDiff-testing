using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace NDiff.Helpers
{
    public class SolutionProvider : IDisposable
    {
        private MSBuildWorkspace Workspace { get; set; }
        private VisualStudioInstance MsBuildInstance { get; set; }
        private Solution Solution { get; set; }
        protected IList<Project> Projects { get; set; } = new List<Project>();
        private string BasePath { get; set; }

        protected SolutionProvider()
        {
            SetupMsBuildLocator();
            CreateMsBuildWorkspace();
        }
        
        
        /// <summary>
        /// Loads all the projects of the main solution into <see cref="Projects"/>.
        /// </summary>
        protected async Task LoadSolutionProjects(string basePath)
        {
            BasePath = basePath;
            
            if (TryFindSolutionPath(BasePath, out var solutionPath))
            {
                await CreateSolution(solutionPath);

                Projects = Solution.Projects.ToList();
            }
            else if (TryFindProjectsPath(BasePath, out var projectsPath))
            {
                await LoadProjects(projectsPath);
            }
        }
        
        /// <summary>
        /// Loads all the projects from their paths to <see cref="Projects"/>. This method is called when there is no .sln solution path.
        /// </summary>
        /// <param name="projectsPath">Paths to the projects.</param>
        /// <seealso cref="TryFindProjectsPath"/>
        private async Task LoadProjects(IEnumerable<string> projectsPath)
        {
            foreach (var projectPath in projectsPath)
            {
                var project = await Workspace.OpenProjectAsync(projectPath);
                Projects.Add(project);
            }
        }
        
        /// <summary>
        /// Creates a <see cref="Solution"/> from the solution path.
        /// </summary>
        /// <param name="solutionPath">Path to the .sln file.</param>
        /// <seealso cref="TryFindSolutionPath"/>
        private async Task CreateSolution(string solutionPath)
        {
            Solution = await Workspace.OpenSolutionAsync(solutionPath);
        }
        /// <summary>
        /// Finds the .sln solution path from the most outer directory.
        /// </summary>
        /// <param name="basePath">The most outer directory.</param>
        /// <param name="solutionPath">Outputs the solution path.</param>
        /// <returns>True if the .sln file is found, otherwise, if solution file is not found it returns false.</returns>
        private static bool TryFindSolutionPath(string basePath, out string solutionPath)
        {
            solutionPath = Directory
                .GetFiles(basePath, "*.sln", SearchOption.AllDirectories)
                .FirstOrDefault();

            return !string.IsNullOrEmpty(solutionPath);
        }
        
        /// <summary>
        /// Finds all .csproj project file paths from the most outer directory. 
        /// </summary>
        /// <param name="basePath">The most outer directory.</param>
        /// <param name="projectsPath">Outputs the found projects path.</param>
        /// <returns>True if there are any paths, otherwise, if not paths are found it return false.</returns>
        private static bool TryFindProjectsPath(string basePath, out string[] projectsPath)
        {
            projectsPath = Directory
                .GetFiles(basePath, "*.csproj", SearchOption.AllDirectories);

            return projectsPath.Length > 0;
        }
        
        /// <summary>
        /// Creates a workspace. Make sure you call <see cref="SetupMsBuildLocator"/> first.
        /// </summary>
        private void CreateMsBuildWorkspace()
        {
            Workspace = MSBuildWorkspace.Create();
        }

        /// <summary>
        /// Sets up the build locator.
        /// </summary>
        private void SetupMsBuildLocator()
        {
            MsBuildInstance = MSBuildLocator.QueryVisualStudioInstances().ToArray().FirstOrDefault();
            MSBuildLocator.RegisterInstance(MsBuildInstance);
        }
        
        
        public void Dispose()
        {
            Workspace?.Dispose();
        }
    }
}