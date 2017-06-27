using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace UpdateVersionConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            ILoggerFactory loggerFactory = new LoggerFactory()
               .AddConsole()
               .AddDebug();
            var logger = loggerFactory.CreateLogger<Program>();

            var app = new Microsoft.Extensions.CommandLineUtils.CommandLineApplication();
            app.Name = "dotnet UpdateVersionConsole.dll";
            var csprojoption = app.Option("--CSProjPath", "Full path of the CS Project File", Microsoft.Extensions.CommandLineUtils.CommandOptionType.SingleValue);
            var buildnumber = app.Option("--BuildNumber", "Build number which needs to appended to project version", Microsoft.Extensions.CommandLineUtils.CommandOptionType.SingleValue);
            //give people help with --help
            app.HelpOption("-? | -h | --help");

            app.OnExecute(() =>
            {
                string csprojFilePath = "";
                int intBuildNumber = 0;
                if (!csprojoption.HasValue())
                {
                    logger.LogError("Please specific the CSProj file path. Use --help for more info");
                    return 1;
                }
                else
                {
                    csprojFilePath = csprojoption.Value();
                    var ext = Path.GetExtension(csprojFilePath);
                    if (ext != ".csproj")
                    {
                        logger.LogError("Please specific file with .csproj extension. Use --help for more info");
                        return 11;
                    }

                    if (!File.Exists(csprojFilePath))
                    {
                        logger.LogError("CSProj file doesn't exist. Please specify valid file. Use --help for more info");
                        return 12;

                    }           
                }

                if (!buildnumber.HasValue())
                {
                    logger.LogError("Please give build number which needs to be appended. Use --help for more info");
                    return 2;
                }
                else
                {
                    if (! int.TryParse(buildnumber.Value(), out intBuildNumber))
                    {
                        logger.LogError("Please give build number as whole number. Use --help for more info");
                        return 21;
                    }
                }

                UpdateCSProjVersion.UpdateBuildNumber(csprojFilePath, intBuildNumber.ToString());

                return 0;
            });

            app.Execute(args);
        }
    }
}