// Copyright 2017 LOSTALLOY
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace LOSTALLOY.LocalHistory
{
    using System;
    using System.IO;
    using System.Regex;
    using JetBrains.Annotations;

    /// <summary>
    /// Utility class.
    /// </summary>
    internal static class Utils
    {
        // Epoch used for converting to unix time.
        private static readonly DateTime EPOCH = new DateTime(1970, 1, 1);

        /// <summary>
        /// Normalize the given path with the right path separator.
        /// </summary>
        /// <param name="path">The path to be normalized.</param>
        /// <returns>The normalized path.</returns>
        [NotNull]
        public static string NormalizePath([NotNull] string path)
        {
            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            path = path.Replace('/', Path.DirectorySeparatorChar);

            if (!IsValidPath(path))
            {
                throw new ArgumentException("path is invalid", nameof(path));
            }

            return Path.GetFullPath(path);
        }

        /// <summary>
        /// Get the repositiry path for a file.
        /// </summary>
        /// <param name="filePath">The path of the file.</param>
        /// <param name="solutionDirectory">The solution directory.</param>
        /// <returns>The repositort path for that file.</returns>
        [NotNull]
        public static string GetRepositoryPathForFile([NotNull] string filePath, [NotNull] string solutionDirectory)
        {
            var fileParentPath = Path.GetDirectoryName(filePath);
            string repositoryPath = null;
            if (!string.IsNullOrEmpty(fileParentPath))
            {
                repositoryPath =
                    fileParentPath
                        .Replace(
                            Path.VolumeSeparatorChar,
                            Path.DirectorySeparatorChar);
            }

            var rootRepositoryPath = GetRootRepositoryPath(solutionDirectory);
            if (repositoryPath == null)
            {
                repositoryPath = rootRepositoryPath;
            }
            else
            {
                repositoryPath = Path.Combine(rootRepositoryPath, repositoryPath);
            }

            LocalHistoryPackage.Log($"{nameof(repositoryPath)} for \"{filePath}\" is \"{repositoryPath}\"");
            return repositoryPath;
        }

        /// <summary>
        /// Return the repository path by the solution path.
        /// </summary>
        /// <param name="solutionDirectory">The path of the solution.</param>
        /// <returns>The path of the repository.</returns>
        [NotNull]
        public static string GetRootRepositoryPath([NotNull] string solutionDirectory)
        {
            return Path.Combine(solutionDirectory, ".localhistory");
        }

        /// <summary>
        /// Convert the unix timestamp in a <see cref="DateTime">DateTime</see> format.
        /// </summary>
        /// <param name="unixTime">The unix timestamp.</param>
        /// <returns>The converted <see cref="DateTime"></returns>.
        public static DateTime ToDateTime(long unixTime)
        {
            return EPOCH.ToLocalTime().AddSeconds(unixTime);
        }

        /// <summary>
        /// Convert a <see cref="DateTime">DateTime</see> ti unix timestamp.
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime">DateTime</see>.</param>
        /// <returns>The converted date in timetsamp format.</returns>
        public static long ToUnixTime(DateTime dateTime)
        {
            return (long)(dateTime - EPOCH.ToLocalTime()).TotalSeconds;
        }

        /// <summary>
        /// Check if a a path is valid under windows.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>true if the path is valid.</returns>
        public static bool IsValidPath(string path)
        {
            var driveCheck = new Regex(@"^[a-zA-Z]:\\$");
            var drive = false;
            if (path.Length >= 3 && driveCheck.IsMatch(path.Substring(0, 3)))
            {
                drive = true;
            }

            var strTheseAreInvalidFileNameChars = new string(Path.GetInvalidPathChars());
            strTheseAreInvalidFileNameChars += @":/?*" + "\"";
            var containsABadCharacter = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");
            string subpath = path;
            if (drive)
            {
                subpath = path.Substring(3, path.Length - 3);
            }

            if (containsABadCharacter.IsMatch(subpath))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if the given file name is valid under windows.
        /// </summary>
        /// <param name="fileName">The file name to check.</param>
        /// <returns>true if the file name is valid.</returns>
        public static bool IsValidFilename(string fileName)
        {
            var containsABadCharacter = new Regex("["
                  + Regex.Escape(new string(Path.GetInvalidPathChars())) + "]");

            if (containsABadCharacter.IsMatch(fileName))
            {
                return false;
            }

            return true;
        }
    }
}
