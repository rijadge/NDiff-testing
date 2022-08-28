using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiff.Action
{
    public class ActionInputs
    {
        string _repositoryName = null!;
        string _branchName = null!;
        string _repositoryUrl = null!;

        public ActionInputs()
        {
            if (Environment.GetEnvironmentVariable("GREETINGS") is { Length: > 0 } greetings)
            {
                Console.WriteLine(greetings);
            }
            if (Environment.GetEnvironmentVariable("GITHUB_SHA") is { Length: > 0 } GITHUB_SHA)
            {
                Console.WriteLine(GITHUB_SHA);
            }
        }

        [Option('o', "owner",
            Required = false,
            HelpText = "The owner, for example: \"dotnet\". Assign from `github.repository_owner`.")]
        public string Owner { get; set; } = null!;

        [Option('n', "name",
            Required = false,
            HelpText = "The repository name, for example: \"samples\". Assign from `github.repository`.")]
        public string Name
        {
            get => _repositoryName;
            set => ParseAndAssign(value, str => _repositoryName = str);
        }

        [Option('b', "branch",
            Required = false,
            HelpText = "The branch name, for example: \"refs/heads/main\". Assign from `github.ref`.")]
        public string Branch
        {
            get => _branchName;
            set => ParseAndAssign(value, str => _branchName = str);
        }

        [Option('d', "dir",
            Required = false,
            HelpText = "The root directory to start recursive searching from.")]
        public string Directory { get; set; } = null!;

        [Option('t', "token",
            Required = false,
            HelpText = "The token to use.")]
        public string Token { get; set; } = null!;

        [Option('w', "workspace",
            Required = false,
            HelpText = "The workspace directory, or repository root directory.")]
        public string WorkspaceDirectory { get; set; } = null!;

        [Option('r', "repository",
            Required = false,
            HelpText = "The repository url.")]
        public string Repository
        {
            get => _repositoryUrl;
            set => ParseRepository(value, str => _repositoryUrl = str);
        }

        static void ParseRepository(string? value, Action<string> assign)
        {
            if (value is { Length: > 0 } && assign is not null)
            {
                assign("https" + value.Remove(0, 3));
            }
        }

        static void ParseAndAssign(string? value, Action<string> assign)
        {
            if (value is { Length: > 0 } && assign is not null)
            {
                assign(value.Split("/")[^1]);
            }
        }
    }
}
