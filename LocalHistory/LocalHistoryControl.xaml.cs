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
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using JetBrains.Annotations;
    using Microsoft.VisualBasic.FileIO;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Pri.LongPath;
    using WPFCustomMessageBox;
    using FileSystem = Microsoft.VisualBasic.FileIO.FileSystem;


    /// <summary>
    ///     Interaction logic for MyControl.xaml
    /// </summary>
    public partial class LocalHistoryControl: UserControl, INotifyPropertyChanged {

        #region Fields

        private bool _showOnlyLabeled;

        #endregion


        #region Constructors and Destructors

        public LocalHistoryControl() {
            InitializeComponent();

            DocumentItems = new ObservableCollection<DocumentNode>();

            // PropertyChanged event propagation
            DocumentItems.CollectionChanged += (o, e) => {
                LocalHistoryPackage.LogTrace("DocumentItems collection changed.");
                OnPropertyChanged(nameof(DocumentItems));
                OnPropertyChanged(nameof(ShowOnlyLabeled));
                OnPropertyChanged(nameof(DocumentItemsViewSource));
                OnPropertyChanged(nameof(VisibleItemsCount));
            };

            DocumentItemsViewSource = new CollectionViewSource
            {
                Source = DocumentItems
            };

            DocumentItemsViewSource.Filter -= LabeledOnlyFilter;
            DocumentItemsViewSource.Filter += LabeledOnlyFilter;

            // Set the DataContext for binding properties
            MainPanel.DataContext = this;

            RefreshXamlItemsVisibility();
        }

        #endregion


        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion


        #region Public Properties

        /// <summary>
        ///     xaml binding
        /// </summary>
        public CollectionViewSource DocumentItemsViewSource { get; set; }

        public ObservableCollection<DocumentNode> DocumentItems { get; set; }


        public bool ShowOnlyLabeled {
            get => _showOnlyLabeled;
            [UsedImplicitly]
            set {
                if (_showOnlyLabeled == value) {
                    return;
                }

                LocalHistoryPackage.Log($"Setting {nameof(ShowOnlyLabeled)} to {value}");
                _showOnlyLabeled = value;
                DocumentItemsViewSource.View.Refresh();
                OnPropertyChanged(nameof(ShowOnlyLabeled));
                LocalHistoryPackage.Instance.UpdateToolWindow("", true);
            }
        }

        public int VisibleItemsCount => DocumentItems?.Count == 0
            ? 0
            : DocumentItemsViewSource.View?.Cast<object>().Count() ?? 0;


        public DocumentNode LatestDocument { get; set; }

        [CanBeNull]
        private IVsDifferenceService differenceService;
        [CanBeNull]
        private IVsWindowFrame differenceFrame;

        #endregion


        #region Public Methods and Operators

        public void RefreshXamlItemsVisibility() {
            NoRevisionsToShowLabel.Visibility = VisibleItemsCount == 0 ? Visibility.Visible : Visibility.Collapsed;
            ResultsPanel.Visibility = DocumentItems?.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }


        public void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                LocalHistoryPackage.LogTrace($"{nameof(OnPropertyChanged)}({propertyName})");
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion


        #region Event Handlers

        private void LabeledOnlyFilter(object sender, [NotNull] FilterEventArgs e) {
            var src = e.Item as DocumentNode;
            if (src != null) {
                if (!ShowOnlyLabeled) {
                    e.Accepted = true;
                    return;
                }

                e.Accepted = src.HasLabel;
            } else {
                e.Accepted = false;
            }
        }


        /// <summary>
        ///     Opens a difference window with <code>IVsDifferenceService</code> when a DocumentNode is double clicked.
        /// </summary>
        private void MouseDoubleClickHandler(object sender, MouseButtonEventArgs e) {
            RunDiff();
        }

        private void DocumentNodesScrollView_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            var scrollViewer = sender as ScrollViewer;
            if (e.Delta > 0) {
                scrollViewer?.LineUp();
            }
            else {
                scrollViewer?.LineDown();
            }
            e.Handled = true;
        }

        private void RunDiff() {
            if (DocumentListBox.SelectedItem == null) {
                LocalHistoryPackage.Log("Can't run diff. No history point selected.", false, true);
                return;
            }

            var node = (DocumentNode) DocumentListBox.SelectedItem;
            var diffToolPath = LocalHistoryPackage.Instance.OptionsPage.DiffToolPath;
            var diffToolArgs = LocalHistoryPackage.Instance.OptionsPage.DiffToolArgs;

            var usedBuiltInDiffTool =
                    LocalHistoryPackage.Instance.OptionsPage.UseInternalDiff ||
                    !File.Exists(diffToolPath) ||
                    !diffToolArgs.Contains("{then}") ||
                    !diffToolArgs.Contains("{now}");


            if (!usedBuiltInDiffTool) {
                LocalHistoryPackage.Log("Using external diff tool.", false, true);
                try {
                    var diff = new Process();
                    diff.StartInfo.FileName = diffToolPath;

                    //run merge | then | now | because we often want to copy something from THEN
                    //and we never want to copy anything from NOW to THEN (makes no sense)
                    diff.StartInfo.Arguments = diffToolArgs
                            .Replace("{then}", $"\"{node.VersionFileFullFilePath}\"")
                            .Replace("{now}", $"\"{node.OriginalPath}\"");

                    diff.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                    diff.Start();
                }
                catch (Exception exception) {
                    LocalHistoryPackage.Log($"Caught exception when trying to run external diff tool. Will try the internal tool. Exception:{exception}");
                    usedBuiltInDiffTool = true;
                }
            }

            // ReSharper disable once InvertIf
            if (usedBuiltInDiffTool) {
                LocalHistoryPackage.Log("Using internal diff tool.", false, true);
                if (!LocalHistoryPackage.Instance.OptionsPage.UseInternalDiff) {
                    //warn the user if anything is wrong
                    if (!string.IsNullOrEmpty(diffToolPath)) {
                        LocalHistoryPackage.Log(
                            $"External diff tool path \"{diffToolPath}\" " +
                            "is invalid. Will use internal diff.",
                            true);
                    }

                    if (!diffToolArgs.Contains("{then}") || !diffToolArgs.Contains("{now}")) {
                        LocalHistoryPackage.Log(
                            $"External diff tool args \"{diffToolArgs}\" " +
                            "doesn't contain {{then}} or {{now}}. Will use internal diff.",
                            true);
                    }
                }

                //if we're here, it means that running the external diff tool failed
                //or we didn't set/have invalid tools paths set.

                if (differenceFrame != null) {
                    if (LocalHistoryPackage.Instance.OptionsPage.SingleFrameForInternalDiffTool) {
                        // Close the last comparison because we only want 1 open at a time
                        LocalHistoryPackage.Log("Closing previous internal diff service frame.", false, true);
                        differenceFrame?.CloseFrame((uint) __FRAMECLOSE.FRAMECLOSE_NoSave);
                    }
                } else {
                    // Get the Difference Service we will use to do the comparison
                    if (differenceService == null) {
                        LocalHistoryPackage.Log("Getting internal diff service.", false, true);
                        differenceService = (IVsDifferenceService) Package.GetGlobalService(typeof(SVsDifferenceService));
                    }
                }

                if (differenceService == null) {
                    LocalHistoryPackage.Log($"Could not get {nameof(differenceService)}. Diff will not work.", true, false);
                    return;
                }

                LocalHistoryPackage.Log("Opening internal diff frame.", false, true);
                // Open a comparison between the old file and the current file
                differenceFrame = differenceService.OpenComparisonWindow2(
                    node.VersionFileFullFilePath,
                    node.OriginalPath,
                    $"{node.TimestampAndLabel} vs Now",
                    $"{node.TimestampAndLabel} vs Now",
                    $"{node.TimestampAndLabel}",
                    $"{node.OriginalFileName} Now",
                    $"{node.TimestampAndLabel} vs Now",
                    null,
                    (uint) __VSDIFFSERVICEOPTIONS.VSDIFFOPT_LeftFileIsTemporary
                );
            }
        }

        #endregion


        #region Methods

        private void DocumentNodes_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (DocumentItems.Count == 0) {
                e.Handled = true;
                return;
            }

            var listBox = (ListBox) sender;
            var node = listBox.SelectedItem as DocumentNode;
            var nodePosition = DocumentItems.IndexOf(node);
            var filePath = node?.VersionFileFullFilePath;
            if (filePath == null) {
                //this happens right after removing a label (renaming a file).
                //Should be harmless, so we just ignore it.
                return;
            }

            DocumentListBox.UpdateLayout();
            var shouldReselectNode = false;

            switch (e.Key) {
                case Key.Enter:
                    RunDiff();
                    //this one can just return, we don't have to update anything
                    return;
                case Key.Up:
                case Key.PageUp:
                    if (nodePosition > 0) {
                        listBox.SelectedItem = DocumentItems[nodePosition - 1];
                        DocumentListBoxScrollViewer.LineUp();
                    } else {
                        DocumentListBoxScrollViewer.ScrollToTop();
                    }
                    break;
                case Key.Down:
                case Key.PageDown:
                    if (nodePosition < DocumentItems.Count - 1) {
                        listBox.SelectedItem = DocumentItems[nodePosition + 1];
                        DocumentListBoxScrollViewer.LineDown();
                    } else {
                        DocumentListBoxScrollViewer.ScrollToBottom();
                    }
                    break;
                case Key.End:
                    listBox.SelectedItem = DocumentItems[DocumentItems.Count - 1];
                    break;
                case Key.Home:
                    listBox.SelectedItem = DocumentItems[0];
                    break;
                case Key.Delete:
                    HandleDeleteKeyDown(node, listBox, nodePosition, filePath);
                    break;
                case Key.L:
                    shouldReselectNode = HandleLabelKeyDown(node, nodePosition);
                    break;
            }

            LocalHistoryPackage.Instance.UpdateToolWindow("", true);

            if (shouldReselectNode) {
                //to avoid the selection jumping
                listBox.SelectedItem = node;
            } else {
                var listBoxItem =
                        (ListBoxItem) DocumentListBox?.ItemContainerGenerator?.ContainerFromItem(DocumentListBox.SelectedItem);
                listBoxItem?.Focus();
            }

            e.Handled = true;
        }


        private void HandleDeleteKeyDown(DocumentNode node, ListBox listBox, int nodePosition, string filePath) {
            if (MessageBoxResult.OK != MessageBox.Show(
                    string.Format(LocalHistory.Resources.FileDeleteWindowMoveToRecycleBin, filePath),
                    LocalHistory.Resources.FileDeleteWindowTitle,
                    MessageBoxButton.OKCancel)) {
                return;
            }

            if (File.Exists(filePath)) {
                try {
                    FileSystem.DeleteFile(filePath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }
                catch (Exception) {
                    //
                }
            }

            //always remove. It can happen that we have two nodes with the same name for some reason (saving really quickly)
            DocumentItems.Remove(node);

            //select the next node after deleting the current one
            if (nodePosition != 0 && DocumentItems.Count > nodePosition - 1) {
                listBox.SelectedItem = DocumentItems[DocumentItems.Count > nodePosition ? nodePosition : nodePosition - 1];
            } else if (DocumentItems.Count > 0) {
                listBox.SelectedItem = DocumentItems[0];
            }
        }

        private bool HandleLabelKeyDown(DocumentNode node, int nodePosition) {
            var shouldReselectNode = false;
            var didChangeLabel = false;
            if (node.HasLabel) {
                var messageBoxResult = CustomMessageBox.ShowYesNoCancel(
                    string.Format(LocalHistory.Resources.LabelDeletionWindowFileHasLabelRemoveIt, node.Label),
                    LocalHistory.Resources.LabelChangeWindowTitle,
                    LocalHistory.Resources.Remove,
                    LocalHistory.Resources.Change,
                    LocalHistory.Resources.Cancel);
                switch (messageBoxResult) {
                    case MessageBoxResult.Cancel:
                        return false;
                    case MessageBoxResult.Yes:
                        node.RemoveLabel();
                        didChangeLabel = true;
                        break;
                    case MessageBoxResult.No:
                        didChangeLabel = TryAddingLabel(node);
                        break;
                }
            } else {
                didChangeLabel = TryAddingLabel(node);
            }

            if (didChangeLabel) {
                shouldReselectNode = true;

                //trigger collection changed event
                DocumentItems.RemoveAt(nodePosition);
                DocumentItems.Insert(nodePosition, node);
            }

            return shouldReselectNode;
        }

        private bool TryAddingLabel(DocumentNode node) {
            void LabelInputValidator(object o, InputBoxValidatingArgs e) {
                if (LabelIsValid(e.Text?.Trim())) {
                    return;
                }

                e.Cancel = true;
                e.Message = LocalHistory.Resources.InvalidLabel;
            }

            bool LabelIsValid(string l) {
                return !string.IsNullOrEmpty(l) &&
                       l.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
            }

            var label = InputBox.Show(
                LocalHistory.Resources.AddLabelMsgBoxLabel,
                LocalHistory.Resources.AddLabelMsgBoxTitle,
                string.Empty,
                LabelInputValidator)?.Text ?? string.Empty;

            if (LabelIsValid(label)) {
                if (node.Label == label) {
                    return false;
                }

                try {
                    node.AddLabel(label);
                }
                catch {
                    return false;
                }

                return true;
            }

            if (!string.IsNullOrEmpty(label)) {
                //only show message if not empty
                //this allows the user to just hit "esc" or even just "enter" to cancel
                MessageBox.Show(
                    string.Format(LocalHistory.Resources.InvalidLabelWindowMessage, label),
                    LocalHistory.Resources.InvalidLabelWindowTitle,
                    MessageBoxButton.OK
                );
            }

            return false;
        }

        #endregion

    }
}
