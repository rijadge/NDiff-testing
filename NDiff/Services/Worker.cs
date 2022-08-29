using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NDiff.Action;
using NDiff.Services.Analyzers.SolutionAnalyzers;

namespace NDiff.Services
{
    public class Worker //: BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<Worker> _logger;
        private readonly ISolutionAnalyzer _solutionAnalyzer;

        public Worker(ILogger<Worker> logger, ISolutionAnalyzer solutionAnalyzer, IConfiguration configuration)
        {
            _logger = logger;
            _solutionAnalyzer = solutionAnalyzer;
            _configuration = configuration;
        }

        public async Task ExecuteAsync(ActionInputs inputs)
        {
            var token = inputs.Token; //_configuration.GetSection("Token").Value;
            var repoUri = inputs.Repository; //_configuration.GetSection("RepoUri").Value;
            string path = null;
            try
            {
                //if (TryDownloadSolution(token, repoUri, out path))
                {
                    //path = _configuration.GetSection("SolutionUrl").Value;
                    var uri = "./";
                    Console.WriteLine("PATH created!:" + uri);
                    await _solutionAnalyzer.AnalyzeSolutionProjects(uri);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("EXCEPTION:" + e.Message);
                // ignore
            }
            finally
            {
               // path = "../api";
                if (!string.IsNullOrEmpty(path))
                    Console.WriteLine("SOL DELETED?:" + DeleteSolution(path));
            }
        }

        private static bool DeleteSolution(string path)
        {
            if (File.Exists(path))
                File.Delete($"{path}.zip");
            if (Directory.Exists(path))
                Directory.Delete(path, true);

            return !Directory.Exists(path) && !File.Exists($"{path}.zip");
        }

        private static bool TryDownloadSolution(string token, string uri, out string path)
        {
            using var client = new WebClient();
            client.Headers.Add("user-agent", "rijadgerguri");
            client.Headers.Add("Authorization", $"token {token}");

            path = "../api";

            client.DownloadFile(
                uri,
                $"{path}.zip");

            if (Directory.Exists(path))
                DeleteSolution(path);

            ZipFile.ExtractToDirectory($"{path}.zip", path);

            return Directory.Exists(path);
        }
    }
}
