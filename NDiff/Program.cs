using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NDiff.Action;
using NDiff.Services;
using NDiff.Services.Analyzers;
using NDiff.Services.Analyzers.ClassAnalyzers;
using NDiff.Services.Analyzers.FileAnalyzers;
using NDiff.Services.Analyzers.ProjectAnalyzers;
using NDiff.Services.Analyzers.SolutionAnalyzers;
using NDiff.Services.Generators;
using System;
using System.Linq;
using System.Threading.Tasks;
using static CommandLine.Parser;

namespace NDiff
{
    public class Program
    {
        private static TService Get<TService>(IHost host)
               where TService : notnull =>
               host.Services.GetRequiredService<TService>();

        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            
            // first step is to analyze first branch
            // analyze second branch
            // find differences
            // create pull request...

            var parser = Default.ParseArguments<ActionInputs>(() => new (), args);

            parser.WithNotParsed(
                errors =>
                {
                    Get<ILoggerFactory>(host)
                        .CreateLogger("DotNet.GitHubAction.Program")
                        .LogError(
                            string.Join(Environment.NewLine, errors.Select(error => error.ToString())));
                    Environment.Exit(2);
                });

            await parser.WithParsedAsync(options => {

                Console.WriteLine("FIRST PARSER");
                return StartAnalysisAsync(options, host);

            });
           

            await parser.WithParsedAsync(options => {

                Console.WriteLine("WORKER PARSER");

                var worker = Get<Worker>(host);

                return worker.ExecuteAsync(options); 

            });

            await host.RunAsync();
        }

        private static async Task StartAnalysisAsync(ActionInputs inputs, IHost host)
        {
            Console.WriteLine("REPOOO;" + inputs.Repository + " _ " + inputs.Token);
            var a = 3;
            var b = 4;
            var c = 5;
            var d = 6;
            var e = 7;
            var f = 8;
                
            var g = 9;
                
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                // services.AddHostedService<Worker>();
                services.AddTransient<Worker>();
                services.AddTransient<ISolutionAnalyzer, SolutionAnalyzer>();
                services.AddTransient<IProjectAnalyzer, ProjectAnalyzer>();
                services.AddTransient<IClassFileAnalyzer, ClassFileAnalyzer>();
                services.AddTransient<IClassAnalyzer, ClassAnalyzer>();
                services.AddSingleton<IAnalyzedClassesState, AnalyzedClassesState>();
                services.AddSingleton<IComponentGenerator, ComponentGenerator>();
                services.AddTransient<OpenApiGenerator>();
            });
    }
}