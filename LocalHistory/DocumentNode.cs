// Copyright 2017 LOSTALLOY
// Copyright 2013 Intel Corporation
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
    using System.IO;
    using System.Regex;
    using JetBrains.Annotations;

    /// <summary>
    /// A Document Node reesent a versioned file.
    /// </summary>
    public class DocumentNode
    {
        [NotNull]
        private readonly string repositoryPath;

        [NotNull]
        private readonly string originalPath;

        [NotNull]
        private readonly string originalFileName;

        [NotNull]
        private readonly string unixTime;

        private readonly DateTime time;

        [CanBeNull]
        private string label;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentNode"/> class.
        /// </summary>
        /// <param name="repositoryPath">The repository path.</param>
        /// <param name="originalPath">The original path.</param>
        /// <param name="originalFileName">The original file name.</param>
        /// <param name="time">The time.</param>
        public DocumentNode(
            [NotNull] string repositoryPath,
            [NotNull] string originalPath,
            [NotNull] string originalFileName,
            DateTime time)
            : this(repositoryPath, originalPath, originalFileName, Utils.ToUnixTime(time).ToString())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentNode"/> class.
        /// </summary>
        /// <param name="repositoryPath">The repository path.</param>
        /// <param name="originalPath">The original path.</param>
        /// <param name="originalFileName">The original file name.</param>
        /// <param name="unixTime">The time in unix timestamp format.</param>
        /// <param name="label">The label.</param>
        public DocumentNode(
            [NotNull] string repositoryPath,
            [NotNull] string originalPath,
            [NotNull] string originalFileName,
            [NotNull] string unixTime,
            [CanBeNull] string label = null)
        {
            ValidateParameters(repositoryPath, originalPath, originalFileName, unixTime, label);

            this.repositoryPath = Utils.NormalizePath(repositoryPath);
            this.originalPath = Utils.NormalizePath(originalPath);
            this.originalFileName = originalFileName;
            this.unixTime = unixTime;
            this.time = Utils.ToDateTime(long.Parse(this.unixTime));
            this.label = label;
        }

        /// <summary>
        /// Gets a value indicating whether a Label has been set.
        /// </summary>
        public bool HasLabel => !string.IsNullOrEmpty(this.label);

        /// <summary>
        /// Gets the full path of the versioned file.
        /// </summary>
        public string VersionFileFullFilePath => Path.Combine(this.RepositoryPath, this.VersionFileName);

        /// <summary>
        /// Gets the name of the versioned file.
        /// </summary>
        public string VersionFileName => $"{this.unixTime}${this.originalFileName}{(this.HasLabel ? $"${this.label}" : string.Empty)}";

        /// <summary>
        /// Gets the path of the repository.
        /// </summary>
        [NotNull]
        public string RepositoryPath => this.repositoryPath;

        /// <summary>
        /// Gets the original path.
        /// </summary>
        [NotNull]
        public string OriginalPath => this.originalPath;

        /// <summary>
        /// Gets the original file name.
        /// </summary>
        [NotNull]
        public string OriginalFileName => this.originalFileName;

        /// <summary>
        /// Gets the label.
        /// </summary>
        [CanBeNull]
        public string Label => this.label;

        /// <summary>
        ///     Gets xaml binding. We only store seconds, so we can't have .f and fiends.
        /// </summary>
        [NotNull]
        [UsedImplicitly]
        public string Timestamp => $"{this.time:dd/MM/yyyy HH:mm:ss}";

        /// <summary>
        /// Gets the timestamp with the label.
        /// </summary>
        [NotNull]
        public string TimestampAndLabel => $"{this.time:dd/MM/yyyy HH:mm:ss}{(this.HasLabel ? $" {this.label}" : string.Empty)}";

        public static bool operator ==(DocumentNode left, DocumentNode right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DocumentNode left, DocumentNode right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Check with the other document equals to this.
        /// </summary>
        /// <param name="other">The other document.</param>
        /// <returns>True if the document is equal.</returns>
        public bool Equals([CanBeNull] DocumentNode other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            // see comment ID 02:07 11/04/2017 for why label and repositoryPath are not included here
            return string.Equals(this.originalPath, other.originalPath, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(this.originalFileName, other.originalFileName, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(this.unixTime, other.unixTime, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((DocumentNode)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                // comment ID 02:07 11/04/2017
                // label is not part of hashcode.
                // we only care for the original file and the timestamp
                // for this reason, we also don't include the repositoryPath
                // two nodes are the considered the same if they are a version for the same file
                // take at the same time. Nothing more.
                var hashCode = StringComparer.OrdinalIgnoreCase.GetHashCode(this.originalPath);
                hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(this.originalFileName);
                hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(this.unixTime);
                return hashCode;
            }
        }

        /// <summary>
        /// Add a label to the document.
        /// </summary>
        /// <param name="label">The label.</param>
        public void AddLabel(string label)
        {
            var currentFullPath = Path.Combine(this.RepositoryPath, this.VersionFileName);
            this.label = label;
            var newFullPath = Path.Combine(this.RepositoryPath, this.VersionFileName);
            File.Move(currentFullPath, newFullPath);
        }

        /// <summary>
        /// Remove the label.
        /// </summary>
        public void RemoveLabel()
        {
            if (!this.HasLabel)
            {
                return;
            }

            var currentFullPath = Path.Combine(this.RepositoryPath, this.VersionFileName);
            var fileNameWithoutLabel = this.VersionFileName.Substring(0, this.VersionFileName.Length - $"${this.label}".Length);
            var newFullPath = Path.Combine(this.RepositoryPath, fileNameWithoutLabel);
            File.Move(currentFullPath, newFullPath);
            this.label = null;
        }

        /// <summary>
        /// Validate parameters to check if they are valid.
        /// </summary>
        /// <param name="repositoryPath">The path of repository, it should be a valid string path.</param>
        /// <param name="originalPath">The original path, it should be a valid string path.</param>
        /// <param name="originalFileName">The original file name, it should be a valid file name.</param>
        /// <param name="unixTime">The unix timestamp, it should be a valid timestamp. </param>
        /// <param name="label"> The label should contains valid chars. </param>
        private static void ValidateParameters(string repositoryPath, string originalPath, string originalFileName, string unixTime, string label)
        {
            if (repositoryPath is null)
            {
                throw new ArgumentNullException(nameof(repositoryPath));
            }

            if (originalPath is null)
            {
                throw new ArgumentNullException(nameof(originalPath));
            }

            if (originalFileName is null)
            {
                throw new ArgumentNullException(nameof(originalFileName));
            }

            if (unixTime is null)
            {
                throw new ArgumentNullException(nameof(unixTime));
            }

            if (!Utils.IsValidPath(repositoryPath))
            {
                throw new ArgumentException("Path is invalid", nameof(repositoryPath));
            }

            if (!Utils.IsValidPath(originalPath))
            {
                throw new ArgumentException("Path is invalid", nameof(originalPath));
            }

            if (!Utils.IsValidFilename(originalFileName))
            {
                throw new ArgumentException("File name is invalid ", nameof(originalFileName));
            }

            if (!long.TryParse(unixTime, out _))
            {
                throw new ArgumentException("Unix timestamp format is invalid ", nameof(unixTime));
            }

            if (label != null)
            {
                // TODO: check if label is valid
            }
        }
    }
}