using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace UpdateVersionConsole
{
    public static class UpdateCSProjVersion
    {
        private static ILogger _logger { get; set; }

        static UpdateCSProjVersion()
        {
            ILoggerFactory loggerFactory = new LoggerFactory()
               .AddConsole()
               .AddDebug();
            _logger = loggerFactory.CreateLogger<Program>();            
        }

        public static bool UpdateBuildNumber(string csprojFilePath, string buildNumber)
        {
            string productVersion = GetProductionVersionFromCSProj(csprojFilePath);

            bool valid = IsProductVersionValid(productVersion);
            if (!valid) return false;
            var newVersion = CreateNewVersion(productVersion, buildNumber);

            bool status = UpdateVersionNumberToCSProj(csprojFilePath, newVersion);
            return true;
        }

        private static string GetProductionVersionFromCSProj(string csprojFilePath)
        {
            var contents = File.ReadAllText(csprojFilePath);
            string regexPattern = @"<Version>(.*?)<\/Version>";
            var rx = new Regex(regexPattern);
            string productVersion = "";

            if (rx.IsMatch(contents))
            {
                var m = rx.Match(contents);
                productVersion = m.Groups[1].Value;
            }

            _logger.LogInformation("Following product version found in the csproj file {0}", productVersion);

            return productVersion;
        }

        private static bool IsProductVersionValid(string productVersion)
        {
            string regexPattern = @"\d.\d.\d[.-]\w*";
            var rx = new Regex(regexPattern);
            if (rx.IsMatch(productVersion))
            {
                _logger.LogInformation("Following product version is **valid** and inline with 1.0.0.0 or 1.0.0-aplha format ",
                 productVersion);
                return true;
            }
            else
            {
                _logger.LogError("Product version found in csproj doesn't match required format {0} {1}",
                    productVersion,
                    " format required must be inline of 1.0.0.0 or 1.0.0-aplha");
                throw new InvalidDataException("Product version found in csproj doesn't match required format, format required must be inline of 1.0.0.0 or 1.0.0-aplha");
            }
        }

        private static string CreateNewVersion(string productVersion, string buildNumber)
        {

            string[] versionparts = productVersion.Split('.');
            string newVersion = "";
            if (versionparts.Length >= 3)
            {
                newVersion = string.Join(".", versionparts[0], versionparts[1], versionparts[2], buildNumber);
                _logger.LogInformation("Created new version number: {0}", newVersion);
            }
            else
            {
                _logger.LogError("Product version doesn't match three slack, can't form new version number {0} {1}",
                productVersion, " format required must be inline of 1.0.0.0 or 1.0.0-aplha");
                throw new InvalidDataException("Product version doesn't match three slack, can't form new version number ");
            }
            return newVersion;
        }

        private static bool UpdateVersionNumberToCSProj(string csprojFilePath, string newVersion)
        {
            var contents = File.ReadAllText(csprojFilePath);
            string regexPattern = @"<Version>(.*?)<\/Version>";
            var rx = new Regex(regexPattern);
            string xmlizednewVersion = @"<Version>" + newVersion + "</Version>";
            string newcontents = rx.Replace(contents, xmlizednewVersion);
            File.WriteAllText(csprojFilePath, newcontents);

            _logger.LogInformation("Update the new product version in the csproj file ",
                 xmlizednewVersion);

            return true;
        }

    }
}
