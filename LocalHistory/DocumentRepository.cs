// Copyright 2019 OrbintSoft - Stefano Balzarotti
// Copyright 2017 LOSTALLOY
// Copyright 2013 Intel Corporation
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
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using Pri.LongPath;

    /// <summary>
    /// This class acts as file repository for versioned files.
    /// </summary>
    internal class DocumentRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentRepository"/> class for the given solution and repository.
        /// </summary>
        /// <param name="solutionDirectory">The directory of the solution.</param>
        /// <param name="repositoryDirectory">The directory where to store versioned repository.</param>
        public DocumentRepository([NotNull]string solutionDirectory, [NotNull] string repositoryDirectory)
        {
            ValidateParameters(solutionDirectory, repositoryDirectory);
            this.SolutionDirectory = solutionDirectory;
            this.RepositoryDirectory = repositoryDirectory;

            if (!Directory.Exists(this.RepositoryDirectory))
            {
                Directory.CreateDirectory(this.RepositoryDirectory);
            }

            File.SetAttributes(this.RepositoryDirectory, System.IO.FileAttributes.Hidden);
        }

        /// <summary>
        /// Gets or sets the path of the solution directory.
        /// </summary>
        [NotNull]
        public string SolutionDirectory { get; set; }

        /// <summary>
        /// Gets or sets the path of the file respository.
        /// </summary>
        [NotNull]
        public string RepositoryDirectory { get; set; }

        // TODO: remove this

        /// <summary>
        /// Gets or sets a reference to the control.
        /// </summary>
        [CanBeNull]
        public LocalHistoryControl Control { get; set; }

        /// <summary>
        /// Creates a new new revision in the repository for the given project item.
        /// </summary>
        /// <param name="filePath">The path of the file to be versioned.</param>
        /// <returns>The repository node for the file.</returns>
        [CanBeNull]
        public DocumentNode CreateRevision([CanBeNull] string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }
            else
            {
                filePath = Utils.NormalizePath(filePath);
                DocumentNode newNode = null;

                try
                {
                    var dateTime = DateTime.Now;
                    newNode = this.CreateRevisionNode(filePath, dateTime);
                    if (newNode is null)
                    {
                        return null;
                    }
                    else
                    {
                        // Create the parent directory if it doesn't exist
                        if (!Directory.Exists(newNode.RepositoryPath))
                        {
                            Directory.CreateDirectory(newNode.RepositoryPath);
                        }

                        // Copy the file to the repository
                        File.Copy(filePath, newNode.VersionFileFullFilePath, true);

                        if (this.Control == null)
                        {
                            this.Control = (LocalHistoryControl)LocalHistoryPackage.Instance.ToolWindow?.Content;
                        }

                        if (this.Control?.LatestDocument?.OriginalPath?.Equals(newNode.OriginalPath) == true)
                        {
                            this.Control.DocumentItems.Insert(0, newNode);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LocalHistoryPackage.Log(ex.Message);
                }

                return newNode;
            }
        }

        /// <summary>
        /// Creates a new <see cref="DocumentNode" /> for the given file and time.
        /// </summary>
        /// <param name="filePath">the path of the file to be versioned.</param>
        /// <param name="dateTime">the datetime of this revision.</param>
        /// <returns>The revision <see cref="DocumentNode"/>.</returns>
        [CanBeNull]
        public DocumentNode CreateRevisionNode([CanBeNull]string filePath, DateTime dateTime)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            var repositoryPath = Utils.GetRepositoryPathForFile(filePath, this.SolutionDirectory);

            var originalFilePath = filePath;
            var fileName = Path.GetFileName(filePath);
            return new DocumentNode(repositoryPath, originalFilePath, fileName, dateTime);
        }

        /// <summary>
        /// Creates a <see cref="DocumentNode" /> for a given <paramref name="versionedFullFilePath"/>.
        /// </summary>
        /// <param name="versionedFullFilePath">the full path of the versioned file.</param>
        /// <returns>The <see cref="DocumentNode"/>.</returns>
        [CanBeNull]
        public DocumentNode CreateDocumentNodeForFilePath([CanBeNull] string versionedFullFilePath)
        {
            if (versionedFullFilePath == null)
            {
                return null;
            }

            versionedFullFilePath = Utils.NormalizePath(versionedFullFilePath);
            string[] parts = Path.GetFileName(versionedFullFilePath).Split('$');
            if (parts.Length <= 1)
            {
                return null;
            }

            var dateFromFileName = parts[0];
            var fileName = parts[1];
            var label = parts.Length == 3 ? parts[2] : null;

            string originalFullFilePath = null;
            var repositoryPath = Path.GetDirectoryName(versionedFullFilePath) ?? this.RepositoryDirectory;
            var versionedFileDir = Utils.NormalizePath(Path.GetDirectoryName(versionedFullFilePath));
            var shouldTryOldFormat = false;
            if (!string.IsNullOrEmpty(versionedFileDir))
            {
                originalFullFilePath = versionedFileDir.Replace(Utils.GetRootRepositoryPath(this.SolutionDirectory), string.Empty);
                string[] splitOriginalFullFilePath = originalFullFilePath.Split(Path.DirectorySeparatorChar);
                var driveLetter = $"{splitOriginalFullFilePath[1]}{Path.VolumeSeparatorChar}{Path.DirectorySeparatorChar}";
                if (!Directory.Exists(driveLetter))
                {
                    shouldTryOldFormat = true;
                }
                else
                {
                    // reconstruct full path, without drive letter
                    originalFullFilePath = string.Join(
                        Path.DirectorySeparatorChar.ToString(),
                        splitOriginalFullFilePath,
                        2,
                        splitOriginalFullFilePath.Length - 2);

                    // reconstruct the drive leter
                    originalFullFilePath = Path.Combine(driveLetter, originalFullFilePath);
                    originalFullFilePath = Path.Combine(originalFullFilePath, fileName);
                    originalFullFilePath = Utils.NormalizePath(originalFullFilePath);

                    if (!File.Exists(originalFullFilePath))
                    {
                        LocalHistoryPackage.LogTrace($"Could not get versionedFileDir for \"{versionedFullFilePath}\". \"{originalFullFilePath}\" does not exist. Will try old format");
                        shouldTryOldFormat = true;
                    }
                }
            }
            else
            {
                return null;
            }

            if (shouldTryOldFormat && !File.Exists(originalFullFilePath))
            {
                // try old format (using non-absolute paths)
                originalFullFilePath = versionedFileDir.Replace(Utils.GetRootRepositoryPath(this.SolutionDirectory), this.SolutionDirectory);
                originalFullFilePath = Path.Combine(originalFullFilePath, fileName);
                originalFullFilePath = Utils.NormalizePath(originalFullFilePath);

                if (File.Exists(originalFullFilePath))
                {
                    LocalHistoryPackage.LogTrace(
                        $"Got original file path for \"{versionedFullFilePath}\" in \"{originalFullFilePath}\" using old format!");
                }
            }

            if (!File.Exists(originalFullFilePath))
            {
                LocalHistoryPackage.Log(
                    $"Failed to retrieve original path for versioned file \"{versionedFullFilePath}\". Will not create {nameof(DocumentNode)}. File \"{originalFullFilePath}\" does not exist.",
                    true);

                return null;
            }

            LocalHistoryPackage.LogTrace(
                $"Creating {nameof(DocumentNode)} for \"{fileName}\" "
                + $"(versionedFullFilePath:\"{versionedFullFilePath}\", originalFullFilePath:\"{originalFullFilePath}\")");

            return new DocumentNode(repositoryPath, originalFullFilePath, fileName, dateFromFileName, label);
        }

        /// <summary>
        /// Returns all DocumentNode objects in the repository for the given project item.
        /// </summary>
        /// <param name="filePath"> The path of the file for wich you want get all revisions.</param>
        /// <returns>the list of all revisons.</returns>
        public IEnumerable<DocumentNode> GetRevisions([CanBeNull] string filePath)
        {
            var revisions = new List<DocumentNode>();
            if (string.IsNullOrEmpty(filePath))
            {
                LocalHistoryPackage.Log(
                    $"Empty {nameof(filePath)}. Returning empty list.");

                return revisions;
            }

            LocalHistoryPackage.Log($"Trying to get revisions for \"{filePath}\"");

            var fileName = Path.GetFileName(filePath);
            var revisionsPath = Utils.GetRepositoryPathForFile(filePath, this.SolutionDirectory);
            var fileBasePath = Path.GetDirectoryName(filePath);
            var oldFormatRevisionsPath = fileBasePath?.Replace(this.SolutionDirectory, this.RepositoryDirectory, StringComparison.InvariantCultureIgnoreCase);

            if (!Directory.Exists(oldFormatRevisionsPath) && !Directory.Exists(revisionsPath))
            {
                return revisions;
            }

            string[] revisionFiles = Array.Empty<string>();
            if (Directory.Exists(revisionsPath))
            {
                revisionFiles = Directory.GetFiles(revisionsPath);
            }

            if (Directory.Exists(oldFormatRevisionsPath))
            {
                revisionFiles = revisionFiles.Union(Directory.GetFiles(oldFormatRevisionsPath)).ToArray();
                LocalHistoryPackage.Log(
                    $"Searching for revisions for \"{fileName}\" in \"{revisionsPath}\" and \"{oldFormatRevisionsPath}\" (using old format)");
            }
            else
            {
                LocalHistoryPackage.Log(
                    $"Searching for revisions for \"{fileName}\" in \"{revisionsPath}\"");
            }

            foreach (var fullFilePath in revisionFiles)
            {
                var normalizedFullFilePath = Utils.NormalizePath(fullFilePath);
                string[] splitFileName = normalizedFullFilePath.Split('$');
                if (splitFileName.Length <= 1)
                {
                    continue;
                }

                normalizedFullFilePath = $"{splitFileName[0]}{splitFileName[1]}"; // remove the label part

                // when running the OnBeforeSave, VS can return the filename as lower
                // i.e., it can ignore the file name's case.
                // Thus, the only way to retrieve everything here is to ignore case
                // Remember that Windows is case insensitive by default, so we can't really
                // have two files with names that differ only in case in the same dir.
                if (!normalizedFullFilePath.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                LocalHistoryPackage.LogTrace($"Found revision \"{fullFilePath}\"");
                var node = this.CreateDocumentNodeForFilePath(fullFilePath);
                if (node != null)
                {
                    revisions.Add(node);
                }
                else
                {
                    LocalHistoryPackage.LogTrace("Not adding revision because node is null.");
                }
            }

            revisions.Reverse();

            return revisions;
        }

        /// <summary>
        /// Validate parameters to check if they are valid.
        /// </summary>
        /// <param name="solutionDirectory">The directory of the solution.</param>
        /// <param name="repositoryDirectory">The directory where to store versioned repository.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="repositoryDirectory"/> or <paramref name="repositoryDirectory"/> are null.</exception>
        /// <exception cref="ArgumentException">If path is path in valid format or <paramref name="solutionDirectory"/> doesn't exist.</exception>
        private static void ValidateParameters([NotNull]string solutionDirectory, [NotNull] string repositoryDirectory)
        {
            if (solutionDirectory is null)
            {
                throw new ArgumentNullException(nameof(solutionDirectory));
            }

            if (repositoryDirectory is null)
            {
                throw new ArgumentNullException(nameof(repositoryDirectory));
            }

            if (Utils.IsValidPath(solutionDirectory))
            {
                throw new ArgumentException("the directory is not valid", nameof(solutionDirectory));
            }

            if (Utils.IsValidPath(repositoryDirectory))
            {
                throw new ArgumentException("the directory is not valid", nameof(repositoryDirectory));
            }

            if (!Directory.Exists(solutionDirectory))
            {
                throw new ArgumentException("the directory doesn't exist", nameof(solutionDirectory));
            }
        }
    }
}