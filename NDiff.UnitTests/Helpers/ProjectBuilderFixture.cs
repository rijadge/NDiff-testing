using System.Linq;
using Microsoft.CodeAnalysis;
using NDiff.Helpers;

namespace NDiff.UnitTests.Helpers
{
    /// <summary>
    /// Sets up the project for all the tests.
    /// </summary>
    public class ProjectBuilderFixture : SolutionProvider
    {
        private const string TestProject = "../../../../TestProject";
        private Project CurrentProject { get; }

        public ProjectBuilderFixture()
        {
            LoadSolutionProjects(TestProject).Wait();
            CurrentProject = Projects.First();
        }

        /// <summary>
        /// Creates a new document and adds it to the <see cref="CurrentProject"/>.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="sourceCode">The source code that the file (<see cref="Document"/>) will have.</param>
        /// <returns>A new instance of <see cref="Document"/>.</returns>
        public Document InitializeDocument(string fileName, string sourceCode)
        {
            var document = CurrentProject.AddDocument(fileName, sourceCode);

            return document;
        }
    }
}