// Copyright 2019 OrbintSoft - Stefano Balzarotti
// Copyright 2017 LOSTALLOY
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//    https://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace LOSTALLOY.LocalHistory
{
    using System;
    using System.Regex;
    using JetBrains.Annotations;
    using Pri.LongPath;

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
        /// <returns>The normalized full path.</returns>
        /// <exception cref="ArgumentNullException">Throw when path is null.</exception>
        /// <exception cref="ArgumentException">Throw when path is not valid.</exception>
        [NotNull]
        public static string NormalizePath([NotNull] string path)
        {
            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("path is invalid", nameof(path));
            }

            path = path.Replace('/', Path.DirectorySeparatorChar).Trim();

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
        /// <exception cref="ArgumentNullException">Throw when filePath or solutionDirectory are null.</exception>
        /// <exception cref="ArgumentException">Throw when filePath or solutionDirectory aren't valid paths.</exception>
        [NotNull]
        public static string GetRepositoryPathForFile([NotNull] string filePath, [NotNull] string solutionDirectory)
        {
            if (filePath is null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (solutionDirectory is null)
            {
                throw new ArgumentNullException(nameof(solutionDirectory));
            }

            if (filePath.Trim() == string.Empty || !IsValidPath(filePath))
            {
                throw new ArgumentException("path is invalid", nameof(filePath));
            }

            if (solutionDirectory.Trim() == string.Empty || !IsValidPath(solutionDirectory))
            {
                throw new ArgumentException("path is invalid", nameof(solutionDirectory));
            }

            var fileParentPath = Path.GetDirectoryName(filePath);
            string repositoryPath = null;
            if (!string.IsNullOrEmpty(fileParentPath))
            {
                // C:\ => C_\
                repositoryPath = fileParentPath.Replace(Path.VolumeSeparatorChar, '_');
            }

            var rootRepositoryPath = GetRootRepositoryPath(solutionDirectory);
            if (repositoryPath is null)
            {
                repositoryPath = rootRepositoryPath;
            }
            else
            {
                repositoryPath = Path.Combine(rootRepositoryPath, repositoryPath);
            }

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
            if (solutionDirectory is null)
            {
                throw new ArgumentNullException(nameof(solutionDirectory));
            }

            solutionDirectory = solutionDirectory.Trim();

            if (solutionDirectory == string.Empty || !IsValidPath(solutionDirectory))
            {
                throw new ArgumentException("path is invalid", nameof(solutionDirectory));
            }

            return Path.Combine(solutionDirectory, ".localhistory");
        }

        /// <summary>
        /// Convert the unix timestamp in a <see cref="DateTime">DateTime</see> format.
        /// </summary>
        /// <param name="unixTime">The unix timestamp.</param>
        /// <returns>The converted <see cref="DateTime"></returns>.
        public static DateTime ToDateTime(long unixTime)
        {
            var maxValue = (DateTime.MaxValue - EPOCH - TimeSpan.FromHours(24)).TotalSeconds - 1;
            var minValue = (DateTime.MinValue - EPOCH + TimeSpan.FromHours(24)).TotalSeconds;

            if (maxValue < unixTime)
            {
                throw new ArgumentOutOfRangeException(nameof(unixTime), $"value is too big, maxValue = {maxValue}");
            }

            if (minValue > unixTime)
            {
                throw new ArgumentOutOfRangeException(nameof(unixTime), $"value is too low, minValue = {minValue}");
            }

            return EPOCH.AddSeconds(unixTime).ToLocalTime();
        }

        /// <summary>
        /// Convert a <see cref="DateTime">DateTime</see> to unix timestamp.
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime">DateTime</see>.</param>
        /// <returns>The converted date in timetsamp format.</returns>
        public static long ToUnixTime(DateTime dateTime)
        {
            // This avoids timezone issues
            if (dateTime < new DateTime(1, 1, 2, 0, 0, 0))
            {
                throw new ArgumentOutOfRangeException(nameof(dateTime), "value is lower than 0001-01-02 00:00:00");
            }

            if (dateTime > new DateTime(9999, 12, 30, 23, 59, 59))
            {
                throw new ArgumentOutOfRangeException(nameof(dateTime), "value is bigger than 9999-12-30 23:59:59");
            }

            return (long)(dateTime.ToUniversalTime() - EPOCH).TotalSeconds;
        }

        /// <summary>
        /// Check if a a path is valid under windows.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>true if the path is valid.</returns>
        public static bool IsValidPath([CanBeNull] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            var driveCheck = new Regex(@"^[a-zA-Z]:\\$");
            var drive = false;
            if (path.Length >= 3 && driveCheck.IsMatch(path.Substring(0, 3)))
            {
                drive = true;
            }

            var strTheseAreInvalidFileNameChars = new string(Path.GetInvalidPathChars());
            strTheseAreInvalidFileNameChars += ":/?*\"";
            var containsABadCharacter = new Regex($"[{Regex.Escape(strTheseAreInvalidFileNameChars)}]");
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
        public static bool IsValidFileName([CanBeNull] string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            fileName = fileName.Trim();

            if (fileName.EndsWith("."))
            {
                return false;
            }

            var containsABadCharacter = new Regex($"[{Regex.Escape(new string(Path.GetInvalidPathChars()))}:/?*\"\\\\]");
            if (containsABadCharacter.IsMatch(fileName))
            {
                return false;
            }



            return true;
        }
    }
}
