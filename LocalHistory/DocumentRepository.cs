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

namespace LOSTALLOY.LocalHistory {
    using System;
    using System.Collections.Generic;
    using Alphaleonis.Win32.Filesystem;
    using System.Linq;
    using JetBrains.Annotations;


    internal sealed class DocumentRepository {

        #region Constructors and Destructors

        /// <summary>
        ///     Creates a new <code>DocumentRepository</code> for the given solution and repository.
        /// </summary>
        public DocumentRepository(string solutionDirectory, string repositoryDirectory) {
            SolutionDirectory = solutionDirectory;
            RepositoryDirectory = repositoryDirectory;

            if (!Directory.Exists(RepositoryDirectory)) {
                Directory.CreateDirectory(RepositoryDirectory);
            }

            File.SetAttributes(RepositoryDirectory, System.IO.FileAttributes.Hidden);
        }


        /// <summary>
        ///     Creates a new new revision in the repository for the given project item.
        /// </summary>
        [CanBeNull]
        public DocumentNode CreateRevision(string filePath) {
            LocalHistoryPackage.Log("CreateRevision(" + filePath + ")");

            if (string.IsNullOrEmpty(filePath)) {
                return null;
            }

            filePath = Utils.NormalizePath(filePath);
            DocumentNode newNode = null;

            try {
                var dateTime = DateTime.Now;
                newNode = CreateRevisionNode(filePath, dateTime);
                if (newNode == null) {
                    return null;
                }

                // Create the parent directory if it doesn't exist
                if (!Directory.Exists(newNode.RepositoryPath)) {
                    LocalHistoryPackage.Log($"Creating (because it doesn't exist) \"{newNode.RepositoryPath}\"");
                    Directory.CreateDirectory(newNode.RepositoryPath);
                }

                // Copy the file to the repository
                LocalHistoryPackage.Log($"Copying \"{filePath}\" to \"{newNode.VersionFileFullFilePath}\"");
                File.Copy(filePath, newNode.VersionFileFullFilePath, true);

                if (Control == null) {
                    Control = (LocalHistoryControl)LocalHistoryPackage.Instance.ToolWindow?.Content;
                }

                if (Control?.LatestDocument?.OriginalPath.Equals(newNode.OriginalPath) == true) {
                    Control.DocumentItems.Insert(0, newNode);
                }
            }
            catch (Exception ex) {
                LocalHistoryPackage.Log(ex.Message);
            }

            return newNode;
        }


        /// <summary>
        ///     Creates a new <see cref="DocumentNode" /> for the given file and time.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        [CanBeNull]
        public DocumentNode CreateRevisionNode(string filePath, DateTime dateTime) {
            LocalHistoryPackage.Log(
                $"CreateRevisionNode(filePath:\"{filePath}\", dateTime:{dateTime}), SolutionDirectory:\"{SolutionDirectory}\", RepositoryDirectory:\"{RepositoryDirectory}\"");

            if (string.IsNullOrEmpty(filePath)) {
                LocalHistoryPackage.Log("Empty path. Will not create revision.");
                return null;
            }

            var repositoryPath = Utils.GetRepositoryPathForFile(filePath, SolutionDirectory);

            var originalFilePath = filePath;
            var fileName = Path.GetFileName(filePath);
            LocalHistoryPackage.Log($"filePath:\"{filePath}\", repositoryPath:\"{repositoryPath}\"");
            return new DocumentNode(repositoryPath, originalFilePath, fileName, dateTime);
        }


        /// <summary>
        ///     Creates a <see cref="DocumentNode" /> for a given <see cref="versionedFullFilePath" />
        /// </summary>
        [CanBeNull]
        public DocumentNode CreateDocumentNodeForFilePath([CanBeNull] string versionedFullFilePath) {
            if (versionedFullFilePath == null) {
                return null;
            }

            LocalHistoryPackage.LogTrace($"Trying to create {nameof(DocumentNode)} for \"{versionedFullFilePath}\"");

            versionedFullFilePath = Utils.NormalizePath(versionedFullFilePath);
            string[] parts = Path.GetFileName(versionedFullFilePath).Split('$');
            if (parts.Length <= 1) { 
                LocalHistoryPackage.Log($"Will not create {nameof(DocumentNode)} because filename \"{versionedFullFilePath}\" is in the wrong format", true);
                return null;
            }

            var dateFromFileName = parts[0];
            var fileName = parts[1];
            var label = parts.Length == 3 ? parts[2] : null;
            
            string originalFullFilePath = null;
            var repositoryPath = Path.GetDirectoryName(versionedFullFilePath) ?? RepositoryDirectory;
            var versionedFileDir = Utils.NormalizePath(Path.GetDirectoryName(versionedFullFilePath));
            var shouldTryOldFormat = false;
            if (!string.IsNullOrEmpty(versionedFileDir)) {
                originalFullFilePath = versionedFileDir.Replace(Utils.GetRootRepositoryPath(SolutionDirectory), string.Empty);
                string[] splitOriginalFullFilePath = originalFullFilePath.Split(Path.DirectorySeparatorChar);
                var driveLetter = $"{splitOriginalFullFilePath[1]}{Path.VolumeSeparatorChar}{Path.DirectorySeparatorChar}";
                if (!Directory.Exists(driveLetter)) {
                    LocalHistoryPackage.LogTrace($"Could not get versionedFileDir for \"{versionedFullFilePath}\". \"{driveLetter}\" is not a valid drive leter. Will try old format");

                    shouldTryOldFormat = true;
                } else {
                    //reconstruct full path, without drive letter
                    originalFullFilePath = string.Join(
                        Path.DirectorySeparatorChar.ToString(),
                        splitOriginalFullFilePath,
                        2,
                        splitOriginalFullFilePath.Length - 2);

                    //reconstruct the drive leter
                    originalFullFilePath = Path.Combine(driveLetter, originalFullFilePath);
                    originalFullFilePath = Path.Combine(originalFullFilePath, fileName);
                    originalFullFilePath = Utils.NormalizePath(originalFullFilePath);

                    if (!File.Exists(originalFullFilePath)) {
                        LocalHistoryPackage.LogTrace($"Could not get versionedFileDir for \"{versionedFullFilePath}\". \"{originalFullFilePath}\" does not exist. Will try old format");
                        shouldTryOldFormat = true;
                    }
                }
            } else {
                LocalHistoryPackage.Log($"Could not get versionedFileDir for \"{versionedFullFilePath}\". Will not create {nameof(DocumentNode)}.", true);
                return null;
            }


            if (shouldTryOldFormat && !File.Exists(originalFullFilePath)) {
                LocalHistoryPackage.LogTrace($"Trying to get original file path for \"{versionedFullFilePath}\". using old format.");

                //try old format (using non-absolute paths)
                originalFullFilePath = versionedFileDir.Replace(Utils.GetRootRepositoryPath(SolutionDirectory), SolutionDirectory);
                originalFullFilePath = Path.Combine(originalFullFilePath, fileName);
                originalFullFilePath = Utils.NormalizePath(originalFullFilePath);

                if (File.Exists(originalFullFilePath)) {
                    LocalHistoryPackage.LogTrace(
                        $"Got original file path for \"{versionedFullFilePath}\" in \"{originalFullFilePath}\" using old format!");
                }
            }

            if (!File.Exists(originalFullFilePath)) {
                LocalHistoryPackage.Log(
                    $"Failed to retrieve original path for versioned file \"{versionedFullFilePath}\". Will not create {nameof(DocumentNode)}. File \"{originalFullFilePath}\" does not exist.",
                    true);

                return null;
            }

            LocalHistoryPackage.LogTrace(
                $"Creating {nameof(DocumentNode)} for \"{fileName}\" "
                + $"(versionedFullFilePath:\"{versionedFullFilePath}\", originalFullFilePath:\"{originalFullFilePath}\")"
            );

            return new DocumentNode(repositoryPath, originalFullFilePath, fileName, dateFromFileName, label);
        }

        #endregion


        #region Public Properties

        public string SolutionDirectory { get; set; }

        public string RepositoryDirectory { get; set; }

        // TODO: remove this
        public LocalHistoryControl Control { get; set; }

        #endregion


        #region Public Methods and Operators

        /// <summary>
        ///     Returns all DocumentNode objects in the repository for the given project item.
        /// </summary>
        public IEnumerable<DocumentNode> GetRevisions([CanBeNull] string filePath) {
            var revisions = new List<DocumentNode>();
            if (string.IsNullOrEmpty(filePath)) {
                LocalHistoryPackage.Log(
                    $"Empty {nameof(filePath)}. Returning empty list.");

                return revisions;
            }

            LocalHistoryPackage.Log($"Trying to get revisions for \"{filePath}\"");

            var fileName = Path.GetFileName(filePath);
            var revisionsPath = Utils.GetRepositoryPathForFile(filePath, SolutionDirectory);
            var fileBasePath = Path.GetDirectoryName(filePath);
            var oldFormatRevisionsPath = fileBasePath?.Replace(SolutionDirectory, RepositoryDirectory, StringComparison.InvariantCultureIgnoreCase);

            if (!Directory.Exists(oldFormatRevisionsPath) && !Directory.Exists(revisionsPath)) { 
                LocalHistoryPackage.LogTrace($"Neither revisionsPath \"{revisionsPath}\" nor oldFormatRevisionsPath \"{oldFormatRevisionsPath}\" exist." + " Returning empty list.");
                return revisions;
            }

            string[] revisionFiles = Directory.GetFiles(revisionsPath);
            if (Directory.Exists(oldFormatRevisionsPath)) {
                revisionFiles = revisionFiles.Union(Directory.GetFiles(oldFormatRevisionsPath)).ToArray();
                LocalHistoryPackage.Log(
                    $"Searching for revisions for \"{fileName}\" in \"{revisionsPath}\" and \"{oldFormatRevisionsPath}\" (using old format)");
            } else {
                LocalHistoryPackage.Log(
                    $"Searching for revisions for \"{fileName}\" in \"{revisionsPath}\"");
            }

            foreach (var fullFilePath in revisionFiles) {
                var normalizedFullFilePath = Utils.NormalizePath(fullFilePath);
                string[] splitFileName = normalizedFullFilePath.Split('$');
                if (splitFileName.Length <= 1) {
                    LocalHistoryPackage.LogTrace($"Ignoring revision \"{normalizedFullFilePath}\" because it is not in the correct format.");
                    continue;
                }

                normalizedFullFilePath = $"{splitFileName[0]}{splitFileName[1]}";//remove the label part

                //when running the OnBeforeSave, VS can return the filename as lower
                //i.e., it can ignore the file name's case.
                //Thus, the only way to retrieve everything here is to ignore case
                //Remember that Windows is case insensitive by default, so we can't really
                //have two files with names that differ only in case in the same dir.
                if (!normalizedFullFilePath.EndsWith(fileName, StringComparison.OrdinalIgnoreCase)) {
                    //LocalHistoryPackage.Log($"Not a revision:\"{normalizedFullFilePath}\"");
                    continue;
                }

                LocalHistoryPackage.LogTrace($"Found revision \"{fullFilePath}\"");
                var node = CreateDocumentNodeForFilePath(fullFilePath);
                if (node != null) {
                    revisions.Add(node);
                } else {
                    LocalHistoryPackage.LogTrace("Not adding revision because node is null.");
                }
            }

            revisions.Reverse();

            return revisions;
        }

        #endregion

    }
}
