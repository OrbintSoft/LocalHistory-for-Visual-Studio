﻿// Copyright 2019 OrbintSoft - Stefano Balzarotti
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
    using System.Globalization;
    using JetBrains.Annotations;
    using Pri.LongPath;

    /// <summary>
    /// A Document Node represent a versioned file.
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
            : this(repositoryPath, originalPath, originalFileName, Utils.ToUnixTime(time).ToString(CultureInfo.InvariantCulture))
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
        /// <exception cref="ArgumentNullException">If you pass null to a [NotNull] parameter.</exception>
        /// <exception cref="ArgumentException">If you pass an invalid argument.</exception>
        /// <exception cref="ArgumentOutOfRangeException">if you pass a <paramref name="unixTime"/> out of range.</exception>
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
            this.time = Utils.ToDateTime(long.Parse(this.unixTime, CultureInfo.InvariantCulture));
            this.label = label;
        }

        /// <summary>
        /// This event is raised if label has changed.
        /// </summary>
        public event EventHandler VersionFilePathChanged;

        /// <summary>
        /// Gets a value indicating whether a Label has been set.
        /// </summary>
        public bool HasLabel => !string.IsNullOrEmpty(this.label);

        /// <summary>
        /// Gets the full path of the versioned file.
        /// </summary>
        [NotNull]
        public string VersionFileFullFilePath => Path.Combine(this.RepositoryPath, this.VersionFileName);

        /// <summary>
        /// Gets the name of the versioned file.
        /// </summary>
        [NotNull]
        public string VersionFileName => $"{this.unixTime.EscapeFileVersionSeparator()}{ConfigCostants.FileVersionFieldSeparator}{this.originalFileName.EscapeFileVersionSeparator()}{(this.HasLabel ? $"{ConfigCostants.FileVersionFieldSeparator}{this.label.EscapeFileVersionSeparator()}" : string.Empty)}";

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

        // TODO: Timestamp should respect user culture.

        /// <summary>
        ///  Gets xaml binding. We only store seconds, so we can't have .f and fiends.
        /// </summary>
        [NotNull]
        [UsedImplicitly]
        public string Timestamp => $"{this.time:yyyy-MM-dd HH:mm:ss}";

        /// <summary>
        /// Gets the timestamp with the label.
        /// </summary>
        [NotNull]
        public string TimestampAndLabel => $"{this.Timestamp}{(this.HasLabel ? $" {this.label}" : string.Empty)}";

        public static bool operator ==([CanBeNull]DocumentNode left, [CanBeNull]DocumentNode right)
        {
            return Equals(left, right);
        }

        public static bool operator !=([CanBeNull]DocumentNode left, [CanBeNull] DocumentNode right)
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
        public override bool Equals([CanBeNull]object obj)
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
        public void AddLabel([NotNull]string label)
        {
            if (label is null)
            {
                throw new ArgumentNullException(label);
            }

            var currentFullPath = this.VersionFileFullFilePath;
            this.label = label;
            var newFullPath = this.VersionFileFullFilePath;
            this.VersionFilePathChanged?.Invoke(this, EventArgs.Empty);

            // TODO: file management should be handled by document repository.
            if (File.Exists(currentFullPath))
            {
                File.Move(currentFullPath, newFullPath);
            }
        }

        /// <summary>
        /// Remove the label.
        /// </summary>
        public void RemoveLabel()
        {
            if (this.HasLabel)
            {
                var currentFullPath = this.VersionFileFullFilePath;
                this.label = null;
                var newFullPath = this.VersionFileFullFilePath;
                this.VersionFilePathChanged?.Invoke(this, EventArgs.Empty);

                // TODO: file management should be handled by document repository.
                if (File.Exists(currentFullPath))
                {
                    File.Move(currentFullPath, newFullPath);
                }
            }
        }

        /// <summary>
        /// Validate parameters to check if they are valid.
        /// </summary>
        /// <param name="repositoryPath">The path of repository, it should be a valid string path.</param>
        /// <param name="originalPath">The original path, it should be a valid string path.</param>
        /// <param name="originalFileName">The original file name, it should be a valid file name.</param>
        /// <param name="unixTime">The unix timestamp, it should be a valid timestamp. </param>
        /// <param name="label"> The label should contains valid chars. </param>
        /// <exception cref="ArgumentNullException">If one of [NotNull] parameter is null.</exception>
        /// <exception cref="ArgumentException">If you pass a wrong path or a bad unixTime.</exception>
        private static void ValidateParameters([NotNull]string repositoryPath, [NotNull]string originalPath, [NotNull]string originalFileName, [NotNull] string unixTime, [CanBeNull] string label)
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

            if (!Utils.IsValidFileName(originalFileName))
            {
                throw new ArgumentException("File name is invalid ", nameof(originalFileName));
            }

            if (!long.TryParse(unixTime, out _))
            {
                throw new ArgumentException("Unix timestamp format is invalid ", nameof(unixTime));
            }

            if (label != null && !Utils.IsValidFileName(label))
            {
                throw new ArgumentException("This label can't be used because contains invalid chars", nameof(label));
            }
        }
    }
}