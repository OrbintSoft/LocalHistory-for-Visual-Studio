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
    using Alphaleonis.Win32.Filesystem;
    using JetBrains.Annotations;


    public class DocumentNode {

        #region Static Fields

        // Epoch used for converting to unix time.
        private static readonly DateTime EPOCH = new DateTime(1970, 1, 1);

        #endregion


        #region Fields

        [NotNull]
        private readonly string repositoryPath;

        [NotNull]
        private readonly string originalPath;

        [NotNull]
        private readonly string originalFileName;

        private readonly string _unixTime;

        [CanBeNull]
        private string _label;

        private DateTime _time;

        #endregion


        #region Constructors and Destructors

        public DocumentNode(
            [NotNull] string repositoryPath,
            [NotNull] string originalPath,
            [NotNull] string originalFileName,
            DateTime time):
            this(repositoryPath, originalPath, originalFileName, ToUnixTime(time).ToString()) { }


        public DocumentNode(
            [NotNull] string repositoryPath,
            [NotNull] string originalPath,
            [NotNull] string originalFileName,
            string unixTime,
            [CanBeNull] string label = null) {
            this.repositoryPath = Utils.NormalizePath(repositoryPath);
            this.originalPath = Utils.NormalizePath(originalPath);
            this.originalFileName = originalFileName;
            _unixTime = unixTime;
            _time = ToDateTime(_unixTime);
            _label = label;
        }

        #endregion


        #region Public Properties

        public bool HasLabel => !string.IsNullOrEmpty(_label);

        public string VersionFileFullFilePath => Path.Combine(RepositoryPath, VersionFileName);
        public string VersionFileName => $"{_unixTime}${originalFileName}{(HasLabel ? $"${_label}" : "")}";

        [NotNull]
        public string RepositoryPath => repositoryPath;

        [NotNull]
        public string OriginalPath => originalPath;

        [NotNull]
        public string OriginalFileName => originalFileName;

        [CanBeNull]
        public string Label => _label;

        /// <summary>
        ///     xaml binding. We only store seconds, so we can't have .f and fiends.
        /// </summary>
        [NotNull]
        [UsedImplicitly]
        public string Timestamp => $"{_time:dd/MM/yyyy HH:mm:ss}";

        public DateTime RawTime => _time;

        [NotNull]
        public string TimestampAndLabel => $"{_time:dd/MM/yyyy HH:mm:ss}{(HasLabel ? $" {_label}" : "")}";

        #endregion


        #region Public Methods and Operators

        /// <inheritdoc />
        public static bool operator ==(DocumentNode left, DocumentNode right) {
            return Equals(left, right);
        }


        /// <inheritdoc />
        public static bool operator !=(DocumentNode left, DocumentNode right) {
            return !Equals(left, right);
        }


        /// <inheritdoc />
        public bool Equals([CanBeNull] DocumentNode other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }
        
            //see comment ID 02:07 11/04/2017 for why label and repositoryPath are not included here
            return string.Equals(originalPath, other.originalPath, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(originalFileName, other.originalFileName, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(_unixTime, other._unixTime, StringComparison.OrdinalIgnoreCase);
        }


        /// <inheritdoc />
        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((DocumentNode)obj);
        }


        /// <inheritdoc />
        public override int GetHashCode() {
            unchecked {
                //comment ID 02:07 11/04/2017
                //label is not part of hashcode.
                //we only care for the original file and the timestamp
                //for this reason, we also don't include the repositoryPath
                //two nodes are the considered the same if they are a version for the same file
                //take at the same time. Nothing more.
                var hashCode = StringComparer.OrdinalIgnoreCase.GetHashCode(originalPath);
                hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(originalFileName);
                hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(_unixTime);
                return hashCode;
            }
        }


        public void AddLabel(string label) {
            var currentFullPath = Path.Combine(RepositoryPath, VersionFileName);
            SetLabel(label);
            var newFullPath = Path.Combine(RepositoryPath, VersionFileName);

            try {
                File.Move(currentFullPath, newFullPath);
            }
            catch (Exception e) {
                LocalHistoryPackage.Log($"Caught exception when moving the file to remove the label.Exception is below\n{e}");
            }
        }


        private void SetLabel(string label) {
            _label = label;
            var newFileName = VersionFileName;
            if (newFileName.Length <= 150) {
                return;
            }

            LocalHistoryPackage.Log($"Label {label} is too long for file named {VersionFileName}. It will be truncated.");

            //truncate to keep it sane
            newFileName = newFileName.Substring(0, 150);

            //timestamp$filename.extension$label
            var splitNewFileName = newFileName.Split('$');
            if (splitNewFileName.Length < 3) {
                return;
            }

            _label = splitNewFileName[2];
        }


        public void RemoveLabel() {
            if (!HasLabel) {
                return;
            }

            var currentFullPath = Path.Combine(RepositoryPath, VersionFileName);
            var fileNameWithoutLabel = VersionFileName.Substring(0, VersionFileName.Length - $"${_label}".Length);
            var newFullPath = Path.Combine(RepositoryPath, fileNameWithoutLabel);

            try {
                File.Move(currentFullPath, newFullPath);
            }
            catch (Exception e) { 
                LocalHistoryPackage.Log($"Caught exception when moving the file to remove the label.Exception is below\n{e}");
            }

            _label = null;
        }

        #endregion


        #region Methods

        private static DateTime ToDateTime(string unixTime) {
            return EPOCH.ToLocalTime().AddSeconds(long.Parse(unixTime));
        }


        private static long ToUnixTime(DateTime dateTime) {
            return (long) (dateTime - EPOCH.ToLocalTime()).TotalSeconds;
        }

        #endregion

    }
}
