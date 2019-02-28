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

namespace LOSTALLOY.LocalHistory {
    using System;
    using System.ComponentModel;
    using Microsoft.VisualStudio.Shell;


    internal class OptionsPage: DialogPage {

        #region Fields

        private bool _createRevisionOnlyIfDirty = true;
        // ReSharper disable once RedundantDefaultMemberInitializer
        private bool _enableDebug = false;
        // ReSharper disable once RedundantDefaultMemberInitializer
        private bool _enableTraceLog = false;
        private bool _useInternalDiff = true;
        private bool _singleFrameForInternalDiffTool = true;
        private string _diffToolPath;
        private string _diffToolArgs = "{then} {now}";
        private bool _enforceShortPathsWithExternalDiffTool = true;

        #endregion


        #region Public Properties

        [Category("1. General")]
        [DisplayName("Create revision only if dirty")]
        [Description(
            "If this is enabled, a file revision will only be created if the file is dirty (modified) in the editor.\n"
            + "A possible reason to have this disabled is if the files are frequently modified externally.")]
        public bool CreateRevisionOnlyIfDirty {
            get => _createRevisionOnlyIfDirty;
            set => _createRevisionOnlyIfDirty = value;
        }

        [Category("2. Diff")]
        [DisplayName("1. Use internal diff")]
        [Description("Ignore the set diff tool and use the internal VS tool.")]
        public bool UseInternalDiff {
            get => _useInternalDiff;
            set => _useInternalDiff = value;
        }

        [Category("2. Diff")]
        [DisplayName("2. Single internal diff frame")]
        [Description("Allow only one internal diff frame to be opened.\n" +
                     "Will close the previous frame when opening a new one.")]
        public bool SingleFrameForInternalDiffTool {
            get => _singleFrameForInternalDiffTool;
            set => _singleFrameForInternalDiffTool = value;
        }

        [Category("2. Diff")]
        [DisplayName("3. Diff tool path")]
        [Description("Path to your custom diff tool. If empty or invalid, the built-in diff from VS will be used.")]
        public string DiffToolPath {
            get => _diffToolPath;
            set => _diffToolPath = value.Replace("\"", string.Empty);
        }

        [Category("2. Diff")]
        [DisplayName("4. Diff tool arguments")]
        [Description("{now} is replaced with the current file, and {then} is replaced with version to diff.\n" +
                     "If those are not present, the default VS diff will be used.")]
        public string DiffToolArgs {
            get => _diffToolArgs;
            set => _diffToolArgs = value;
        }        

        [Category("2. Diff")]
        [DisplayName("4. Pass Short Paths with Diff Tool")]
        [Description("If this is enabled, a copy of the versioned file in the repository will be made in the temp dir. This copy will always have a short path (truncated, if necessary).\n"
                     + "This avoids issues with programs that don't support long file paths.")]
        public bool EnforceShortPathsWithExternalDiffTool {
            get => _enforceShortPathsWithExternalDiffTool;
            set => _enforceShortPathsWithExternalDiffTool = value;
        }

        [Category("3. Troubleshooting")]
        [DisplayName("Enable debug")]
        [Description("Toggle this to display debug information to the output window.\n" +
                     "For normal usage, keep this disabled.")]
        public bool EnableDebug {
            get => _enableDebug;
            set => _enableDebug = value;
        }


        [Category("3. Troubleshooting")]
        [DisplayName("Enable trace logging")]
        [Description(
            "This will display trace logs in the output window. It logs a lot.\n" +
            "For normal usage, keep this disabled.")]
        public bool EnableTraceLog {
            get => _enableTraceLog;
            set => _enableTraceLog = value;
        }

        #endregion

    }
}
