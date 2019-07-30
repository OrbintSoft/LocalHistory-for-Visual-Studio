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
    using System.IO;
    using JetBrains.Annotations;

    /// <summary>
    /// Utility class.
    /// </summary>
    internal static class Utils
    {
        /// <summary>
        /// Normalize the given path to a valid path.
        /// </summary>
        /// <param name="path">The path to be normalized.</param>
        /// <returns>The normalized path.</returns>
        [NotNull]
        public static string NormalizePath(string path)
        {
            return Path.GetFullPath(path.Replace('/', '\\'));
        }

        /// <summary>
        /// Get the repositiry path for a file.
        /// </summary>
        /// <param name="filePath">The path of the file.</param>
        /// <param name="solutionDirectory">The solution directory.</param>
        /// <returns>The repositort path for that file.</returns>
        public static string GetRepositoryPathForFile(string filePath, string solutionDirectory)
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
        public static string GetRootRepositoryPath(string solutionDirectory)
        {
            return Path.Combine(solutionDirectory, ".localhistory");
        }
    }
}
