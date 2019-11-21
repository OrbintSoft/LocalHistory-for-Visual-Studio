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
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using EnvDTE;

    using JetBrains.Annotations;

    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Pri.LongPath;


    /// <summary>
    ///     This is the class that implements the package exposed by this assembly.
    ///     The minimum requirement for a class to be considered a valid package for Visual Studio
    ///     is to implement the IVsPackage interface and register itself with the shell.
    ///     This package uses the helper classes defined inside the Managed Package Framework (MPF)
    ///     to do it: it derives from the Package class that provides the implementation of the
    ///     IVsPackage interface and uses the registration attributes defined in the framework to
    ///     register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]

    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]

    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]

    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(LocalHistoryToolWindow))]
    [Guid(GuidList.guidLocalHistoryPkgString)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)] //if this is used, the OnAfterOpenSolution won't fire
    [ProvideOptionPage(
        typeof(OptionsPage),
        "Sugoi LocalHistory",
        "General",
        0,
        0,
        true,
        new[]
        {
            "localhistory",
            "history",
            "version",
            "sugoi",
            "lostalloy"
        })]
    public sealed class LocalHistoryPackage: AsyncPackage, IVsSelectionEvents {

        #region Static Fields

        [NotNull]
        public static LocalHistoryPackage Instance;

        [CanBeNull]
        private static DTE dte;

        [CanBeNull]
        private static SolutionEvents solutionEvents;

        [CanBeNull]
        private static OutputWindowPane _outputWindowPane;

        private static List<string> _lateLogs = new List<string>();

        #endregion


        #region Fields

        [CanBeNull]
        public LocalHistoryToolWindow ToolWindow;
        private uint rdtCookie;
        private uint selectionCookie;
        [CanBeNull]
        private DocumentRepository documentRepository;
        private LocalHistoryDocumentListener documentListener;

        #endregion


        #region Properties

        internal OptionsPage OptionsPage { get; private set; }

        #endregion


        #region Public Methods and Operators

        public static void LogTrace(string message) {
            Log(message, false, true);
        }


        public static void Log(string message, bool forced = false, bool trace = false) {
            return; //Log should be changed.
            if (string.IsNullOrEmpty(message)) {
                return;
            }

            if (!forced && Instance.OptionsPage?.EnableDebug != true) {
                return;
            }

            if (trace && Instance.OptionsPage?.EnableTraceLog != true) {
                return;
            }

            var formattedMessage = $"[{DateTime.Now:HH:mm:ss.ffff}] {message}\n";

            if (dte == null) {
                Debug.WriteLine("DTE is null! Can't log!");
                Debug.WriteLine(formattedMessage);
                _lateLogs.Add(formattedMessage);
                return;
            }

            if (_outputWindowPane == null) {
                _lateLogs.Add(formattedMessage);
                return;
            }

            if (_lateLogs.Any()) {
                _outputWindowPane.OutputString($"Found {_lateLogs.Count} late logs. Beginning output...\n");
                foreach (var lateLog in _lateLogs) {
                    _outputWindowPane.OutputString($"{lateLog}");
                }

                _outputWindowPane.OutputString("Finished outputting late logs.\n");

                _lateLogs.Clear();
            }

            _outputWindowPane.OutputString(formattedMessage);
            Debug.WriteLine(formattedMessage);
        }


        /// <summary>
        ///     When a solution is opened, this function creates a new <code>DocumentRepository</code> and
        ///     registers the <code>LocalHistoryDocumentListener</code> to listen for save events.
        /// </summary>
        private void HandleSolutionOpen()
        {
            Log(nameof(HandleSolutionOpen), false, true);

            // The solution name can be empty if the user opens a file without opening a solution
            var maybeSolution = dte?.Solution;
            if (maybeSolution != null && File.Exists(maybeSolution.FullName)) {
                RegisterDocumentListener();
                RegisterSelectionListener();

                //on initialization, we check if the user has anything open
                //if something is open, we update the window to show that item
                if (dte.ActiveDocument != null)
                {
                    HandleDocumentOpen(dte.ActiveDocument);
                }
                else
                {
                    UpdateToolWindow();
                }
            } else {
                Log($"Did not register document listener. dte.Solution==null? {(maybeSolution == null ? "YES" : $"NO (dte.Solution.FullName: {maybeSolution?.FullName})")}, dte.Solution.FullName:\"{maybeSolution?.FullName ?? "EMPTY"}\"");
            }


        }


        /// <summary>
        ///     When a solution is closed, this function creates unsubscribed to documents and selection events.
        /// </summary>
        public void HandleSolutionClose() {
            Log(nameof(HandleSolutionClose), false, true); UnregisterDocumentListener();
            UnregisterDocumentListener();
            UnregisterSelectionListener();
            UpdateToolWindow();
        }

        private void HandleDocumentOpen(Document document)
        {
            Log(nameof(HandleDocumentOpen), false, true);
            var docFullName = document?.FullName;
            Log($"nameof(HandleDocumentOpen) with {nameof(docFullName)}={docFullName}", false, true);

            if (docFullName != null && File.Exists(docFullName))
            {
                Log($"HandleDocumentOpen with doc {docFullName}");
                UpdateToolWindow(docFullName, false);
            }
        }

        public void RegisterDocumentListener() {
            Log(nameof(RegisterDocumentListener), false, true);
            var documentTable = (IVsRunningDocumentTable) GetGlobalService(typeof(SVsRunningDocumentTable));
            if (documentTable == null)
            {
                Log($"Failed to get solution in {nameof(SVsRunningDocumentTable)} service in {nameof(RegisterDocumentListener)}");
                return;
            }
            var maybeSolution = dte?.Solution;
            if (maybeSolution == null)
            {
                Log($"Failed to get solution in {nameof(RegisterDocumentListener)}");
                return;
            }
            Log($"dte.Solution.FullName: \"{maybeSolution?.FullName}\"", false, true);

            // Create a new document repository for the solution
            var solutionDirectory = Path.GetDirectoryName(maybeSolution?.FullName);
            Debug.Assert(solutionDirectory != null, $"{nameof(solutionDirectory)} != null");
            var repositoryDirectory = Utils.GetRootRepositoryPath(solutionDirectory);
            Log(
                $"Creating {nameof(DocumentRepository)} "
                + $"with {nameof(solutionDirectory)}: \"{solutionDirectory}\" "
                + $"and {nameof(repositoryDirectory)}: \"{repositoryDirectory}\"", false);
            documentRepository = new DocumentRepository(solutionDirectory, repositoryDirectory);

            // Create and register a document listener that will handle save events
            documentListener = new LocalHistoryDocumentListener(documentTable, documentRepository);

            var adviseResult = documentTable.AdviseRunningDocTableEvents(documentListener, out rdtCookie);
            if (adviseResult != VSConstants.S_OK) {
                Log($"Failed to AdviseRunningDocTableEvents. Error code is: {adviseResult}");
            }
        }


        public void UnregisterDocumentListener() {
            var documentTable = (IVsRunningDocumentTable) GetGlobalService(typeof(SVsRunningDocumentTable));

            var unadivise = documentTable.UnadviseRunningDocTableEvents(rdtCookie);
            if (unadivise != VSConstants.S_OK) {
                Log($"Failed to UnadviseRunningDocTableEvents. Error code is: {unadivise}");
            }
        }


        public void RegisterSelectionListener() {
            Log(nameof(RegisterSelectionListener), false, true);
            var selectionMonitor = (IVsMonitorSelection) GetGlobalService(typeof(SVsShellMonitorSelection));

            var adviseResult = selectionMonitor.AdviseSelectionEvents((IVsSelectionEvents)this, out selectionCookie);
            if (adviseResult != VSConstants.S_OK) {
                Log($"Failed to AdviseSelectionEvents. Error code is: {adviseResult}");
            }
        }


        public void UnregisterSelectionListener() {
            Log(nameof(UnregisterSelectionListener), false, true);
            var selectionMonitor = (IVsMonitorSelection) GetGlobalService(typeof(SVsShellMonitorSelection));

            var unadivise = selectionMonitor.UnadviseSelectionEvents(selectionCookie);
            if (unadivise != VSConstants.S_OK) {
                Log($"Failed to UnadviseSelectionEvents. Error code is: {unadivise}");
            }
        }


        int IVsSelectionEvents.OnSelectionChanged(
            IVsHierarchy pHierOld,
            uint itemidOld,
            IVsMultiItemSelect pMISOld,
            ISelectionContainer pSCOld,
            IVsHierarchy pHierNew,
            uint itemidNew,
            IVsMultiItemSelect pMISNew,
            ISelectionContainer pSCNew) {
            Log(nameof(IVsSelectionEvents.OnSelectionChanged), false, true);
            // The selected item can be a Solution, Project, meta ProjectItem or file ProjectItem

            // Don't update the tool window if the selection has not changed
            if (itemidOld == itemidNew) {
                return VSConstants.S_OK;
            }

            // Don't update the tool window if it doesn't exist
            if (ToolWindow == null) {
                ShowToolWindow(false);
            }

            if (ToolWindow == null) {
                return VSConstants.S_OK;
            }

            // Don't update the tool window if it isn't visible
            var windowFrame = (IVsWindowFrame) ToolWindow.Frame;
            if (windowFrame.IsVisible() == VSConstants.S_FALSE) {
                Log("Tool window is not visible. Will not update.");
                return VSConstants.S_OK;
            }

            Log($"Selection change! Previous selection: {itemidOld} -> New selection: {itemidNew}");
            return HandleItemSelection();
        }

        private int HandleItemSelection()
        {
            Log(nameof(HandleItemSelection), false, true);
            var selectedItems = dte?.SelectedItems.Item(1);
            var projectItem = selectedItems?.ProjectItem;

            // Solutions and Projects don't have ProjectItems
            if (projectItem != null && projectItem.FileCount != 0)
            {
                var filePath = projectItem.FileNames[0];

                // Only update for project items that exist (Not all of them do).
                if (File.Exists(filePath)) {
                    UpdateToolWindow(filePath);

                    return VSConstants.S_OK;
                }
            }

            UpdateToolWindow();

            return VSConstants.S_OK;
        }


        public void UpdateToolWindow([CanBeNull] string filePath = "", bool fileCountOnly = false) {
            Log(nameof(UpdateToolWindow), false, true);
            if (ToolWindow == null || filePath == null) {
                return;
            }


            if (documentRepository == null) {
                Log($"{nameof(documentRepository)} wasn't set yet. Can't update Window.");
                return;
            }

            var control = (LocalHistoryControl) ToolWindow.Content;

            if (!fileCountOnly && filePath != "") {
                filePath = Utils.NormalizePath(filePath);

                // Remove all revisions from the revision list that belong to the previous document 
                control.DocumentItems.Clear();

                var revisions = documentRepository.GetRevisions(filePath);
                foreach (var revision in revisions) {
                    LogTrace($"Adding revision \"{revision.VersionFileFullFilePath}\"");
                    control.DocumentItems.Add(revision);
                }

                // Add the project item and its history to the revision list
                var repositoryPath = Utils.GetRepositoryPathForFile(
                    filePath,
                    documentRepository.SolutionDirectory);

                Log(
                    "Setting LatestDocument to: "
                    + $"repositoryPath:\"{repositoryPath}\", "
                    + $"filePath:\"{filePath}\", "
                    + $"filename\"{Path.GetFileName(filePath)}\"");

                control.LatestDocument = new DocumentNode(
                    repositoryPath,
                    filePath,
                    Path.GetFileName(filePath),
                    DateTime.Now);

                ToolWindow?.SetWindowCaption($" - {Path.GetFileName(filePath)} ({control.VisibleItemsCount})");
            } else if (fileCountOnly) {
                var currentCaption = ToolWindow.Caption;

                //regex to replace the current file count in () with the current one
                var regex = new Regex(@"\(([^)]*)\)[^(]*$");
                ToolWindow.Caption = regex.Replace(currentCaption, $"({control.VisibleItemsCount})");
            } else {
                //we just switched the selection to something else (like the "Properties" or "References"
                //or maybe we unloaded the solution
                //we need to clear the list so it will show the empty message propertly
                control.LatestDocument = null;
                control.DocumentItems.Clear();
            }

            control.RefreshXamlItemsVisibility();
        }


        int IVsSelectionEvents.OnCmdUIContextChanged(uint dwCmdUICookie, int fActive)
        {
            return VSConstants.S_OK;
        }


        int IVsSelectionEvents.OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
        {
            return VSConstants.S_OK;
        }

        #endregion


        #region Event Handlers

        /// <summary>
        ///     This function is the callback used to execute a command when the a menu item is clicked.
        ///     See the Initialize method to see how the menu item is associated to this function using
        ///     the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ProjectItemContextMenuHandler(object sender, EventArgs e) {
            Log(nameof(ProjectItemContextMenuHandler), false, true);
            var filePath = dte?.SelectedItems.Item(1).ProjectItem.FileNames[0];
            Log($"Processing right-click command for {nameof(filePath)}:\"{filePath}\"");

            if (File.Exists(filePath)) {
                Log("Showing window (right-click command)");
                ShowToolWindow(true);
                UpdateToolWindow(filePath);
            } else {
                Log($"File \"{filePath}\" does not exist. Will not activate the tool window.");
            }
        }


        /// <summary>
        ///     This function is called when the user clicks the menu item that shows the
        ///     tool window. See the Initialize method to see how the menu item is associated to
        ///     this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ToolWindowMenuItemHandler(object sender, EventArgs e) {
            Log(nameof(ToolWindowMenuItemHandler), false, true);
            ShowToolWindow(true);
        }

        #endregion


        #region Methods

        #region Package Members

        /// <summary>
        ///     Initialization of the package; this method is called right after the package is sited.
        /// </summary>
        protected override async System.Threading.Tasks.Task InitializeAsync(
           CancellationToken cancellationToken,
           IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            //Log needs the OptionsPage
            OptionsPage = (OptionsPage)GetDialogPage(typeof(OptionsPage));
            Instance = this;

            //Log needs the dte object
            dte = GetGlobalService(typeof(DTE)) as DTE;
            if (dte == null) {
                //this log will only log with Debug.WriteLine, since we failed to get the DTE for some reason
                Log("Could not get DTE object. Will not initialize.");
                return;
            }

            //This is the earliest we can safely log (that will go the output window)
            //previous logs will be Debug.WriteLine only
            Log($"Entering {nameof(InitializeAsync)}");

            var isSlnLoaded = await IsSolutionLoadedAsync();
            if (isSlnLoaded)
            {
                //already loaded, so we need to handle it asap
                HandleSolutionOpen();
            }

            //it's recommended to keep refs to Events objects to avoid the GC eating them up
            //https://stackoverflow.com/a/32600629/2573470
            solutionEvents = dte.Events.SolutionEvents;
            if (solutionEvents == null)
            {
                Log("Could not get te.Events.SolutionEvents. Will not initialize.");
                return;
            }

            solutionEvents.Opened += HandleSolutionOpen;
            solutionEvents.BeforeClosing += HandleSolutionClose;

            // Add our command handlers for menu (commands must exist in the .vsct file)
            Log("Adding tool menu handler.");
            var mcs = await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (mcs != null)
            {
                // Create the command for the menu item.
                var menuCommandID = new CommandID(GuidList.guidLocalHistoryCmdSet, (int) PkgCmdIDList.cmdidLocalHistoryMenuItem);
                var menuItem = new MenuCommand(ProjectItemContextMenuHandler, menuCommandID);
                mcs.AddCommand(menuItem);
                Log("Added context menu command.");

                // Create the command for the tool window
                var toolwndCommandID = new CommandID(GuidList.guidLocalHistoryCmdSet, (int) PkgCmdIDList.cmdidLocalHistoryWindow);
                var menuToolWin = new MenuCommand(ToolWindowMenuItemHandler, toolwndCommandID);
                mcs.AddCommand(menuToolWin);
                Log("Added menu command.");
            } else {
                Log("Could not get IMenuCommandService. Tool menu will not work.");
            }

            ShowToolWindow(false, cancellationToken);
            dte.Events.DocumentEvents.DocumentOpened += HandleDocumentOpen;
        }

        private async Task<bool> IsSolutionLoadedAsync()
        {
            Log(nameof(IsSolutionLoadedAsync), false, true);
            var slnService = await GetServiceAsync(typeof(SVsSolution)) as IVsSolution;
            if (slnService == null)
            {
                Log($"Failed to get SVsSolution service. Will not initialize.");
                return false;
            }

            ErrorHandler.ThrowOnFailure(slnService.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out var value));

            return value is bool isSlnOpen && isSlnOpen;
        }

        #endregion


        private async void ShowToolWindow(bool setVisible, CancellationToken cancellationToken = default)
        {
            Log(nameof(ShowToolWindow), false, true);
            if (OptionsPage.EnableDebug  && _outputWindowPane == null) {
                Log("Creating output Window.");
                // ReSharper disable once PossibleNullReferenceException
                var window = dte.Windows.Item(EnvDTEConstants.vsWindowKindOutput);
                var outputWindow = (OutputWindow) window?.Object;
                _outputWindowPane = outputWindow?.OutputWindowPanes.Add(Resources.ToolWindowTitle);
            }

            if (ToolWindow == null) {
                Log("Tool window is null. Searching for it...");
                // Get the instance number 0 of this tool window. This window is single instance so this instance
                // is actually the only one.
                ToolWindow = await FindToolWindowAsync(
                    typeof(LocalHistoryToolWindow),
                    0,
                    true,
                    cancellationToken) as LocalHistoryToolWindow;
                if (ToolWindow?.Frame == null) {
                    Log("Can not create tool window.");
                    return;
                }

                ToolWindow.SetWindowCaption();
            }

            if (ToolWindow == null) {
                Log("Failed to create tool window.");
                return;
            }

            if (!setVisible) {
                return;
            }

            Log("Showing tool window.");
            // Make sure the tool window is visible to the user
            // ReSharper disable once PossibleNullReferenceException
            var windowFrame = (IVsWindowFrame) ToolWindow.Frame;
            var result = windowFrame.Show();
            if (result != VSConstants.S_OK) {
                Log($"Failed to show Window. Error code is: {result}");
            }

            if (windowFrame.IsVisible() != VSConstants.S_OK) {
                Log("Failed to show Window. This should never happen.");
            }
        }

        #endregion

    }
}
