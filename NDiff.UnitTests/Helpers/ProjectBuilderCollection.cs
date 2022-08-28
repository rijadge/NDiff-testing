using Xunit;

namespace NDiff.UnitTests.Helpers
{
    [CollectionDefinition("Project builder collection")]
    public class ProjectBuilderCollection : ICollectionFixture<ProjectBuilderFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}