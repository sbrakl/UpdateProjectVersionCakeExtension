using Cake.Core;
using Cake.Core.Annotations;
using Cake.Common.Diagnostics;
using Cake.Core.IO;
using System.IO;
using System.Text.RegularExpressions;
using System;

namespace UpdateCSProjVersion
{
    public static class MyCakeExtension
    {

        /// <summary>
        /// Reads all text from a file
        /// </summary>
        /// <returns>The file's text.</returns>
        /// <param name="context">The context.</param>
        /// <param name="file">The file to read.</param>
        [CakeMethodAlias]
        public static string FileReadText(this ICakeContext context, FilePath file)
        {
            var filename = file.MakeAbsolute(context.Environment).FullPath;

            return File.ReadAllText(filename);
        }

        /// <summary>
        /// Writes all text to a file
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="file">The file to write to.</param>
        /// <param name="text">The text to write.</param>
        [CakeMethodAlias]
        public static void FileWriteText(this ICakeContext context, FilePath file, string text)
        {
            var filename = file.MakeAbsolute(context.Environment).FullPath;

            File.WriteAllText(filename, text);
        }

        /// <summary>
        /// Update the CSProj Version with the buildNumber
        /// </summary>
        /// <param name="context"></param>
        /// <param name="csprojFilePath"></param>
        /// <param name="buildNumber"></param>
        /// <returns></returns>
        [CakeMethodAlias]
        public static bool UpdateBuildNumber(this ICakeContext context, string csprojFilePath, string buildNumber)
        {

            string productVersion = GetProductionVersionFromCSProj(context, csprojFilePath);

            bool valid = IsProductVersionValid(context, productVersion);
            if (!valid) return false;
            var newVersion = CreateNewVersion(context, productVersion, buildNumber);

            bool status = UpdateVersionNumberToCSProj(context, csprojFilePath, newVersion);
            return true;
        }

      

        private static bool IsProductVersionValid(this ICakeContext context, string productVersion)
        {
            string regexPattern = @"\d.\d.\d[.-]\w*";
            var rx = new Regex(regexPattern);
            if (rx.IsMatch(productVersion))
            {
                LoggingAliases.Information(context, 
                 "Following product version is **valid** and inline with 1.0.0.0 or 1.0.0-aplha format ",
                 productVersion);
                return true;
            }
            else
            {
                LoggingAliases.Error(context,  "Product version found in csproj doesn't match required format {0} {1}",
                    productVersion,
                    " format required must be inline of 1.0.0.0 or 1.0.0-aplha");
                throw new InvalidDataException("Product version found in csproj doesn't match required format, format required must be inline of 1.0.0.0 or 1.0.0-aplha");
            }
        }

        private static string GetProductionVersionFromCSProj(this ICakeContext context, string csprojFilePath)
        {
            var contents = FileReadText(context, csprojFilePath);
            string regexPattern = @"<Version>(.*?)<\/Version>";
            var rx = new Regex(regexPattern);
            string productVersion = "";

            if (rx.IsMatch(contents))
            {
                var m = rx.Match(contents);                
                productVersion = m.Groups[1].Value;
            }

            LoggingAliases.Information(context, "Following product version found in the csproj file {0}", productVersion);           

            return productVersion;
        }

        private static string CreateNewVersion(this ICakeContext context, string productVersion, string buildNumber)
        {

            string[] versionparts = productVersion.Split('.');
            string newVersion = "";
            if (versionparts.Length >= 3)
            {
                newVersion = string.Join(".", versionparts[0], versionparts[1], versionparts[2], buildNumber);
                LoggingAliases.Information(context, "Created new version number: {0}", newVersion);
            }
            else
            {
                LoggingAliases.Error( context, "Product version doesn't match three slack, can't form new version number {0} {1}",
                productVersion, " format required must be inline of 1.0.0.0 or 1.0.0-aplha");
                throw new InvalidDataException("Product version doesn't match three slack, can't form new version number ");
            }
            return newVersion;
        }

        private static bool UpdateVersionNumberToCSProj(ICakeContext context, string csprojFilePath, string newVersion)
        {
            var contents = FileReadText(context, csprojFilePath);
            string regexPattern = @"<Version>(.*?)<\/Version>";
            var rx = new Regex(regexPattern);
            string xmlizednewVersion = @"<Version>" + newVersion + "</Version>";
            string newcontents = rx.Replace(contents, xmlizednewVersion);
            FileWriteText(context, csprojFilePath, newcontents);

            LoggingAliases.Information (context,  "Update the new product version in the csproj file ",
                 xmlizednewVersion);

            return true;
        }

    }
}
