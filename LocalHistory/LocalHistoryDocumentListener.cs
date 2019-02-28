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

// ReSharper disable IdentifierTypo
namespace LOSTALLOY.LocalHistory {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using Utilities;


    internal class LocalHistoryDocumentListener: IVsRunningDocTableEvents3Adapter {

        #region Fields

        private readonly IVsRunningDocumentTable documentTable;
        private readonly DocumentRepository documentRepository;
        private readonly HashSet<uint> _dirtyDocCookie = new HashSet<uint>();

        #endregion


        #region Constructors and Destructors

        public LocalHistoryDocumentListener(IVsRunningDocumentTable documentTable, DocumentRepository documentRepository) {
            this.documentTable = documentTable;
            this.documentRepository = documentRepository;
        }

        #endregion


        #region Public Methods and Operators

        

        /// <summary>
        ///     When this event is triggered on a project item, a copy of the file is saved to the
        ///     <see cref="documentRepository" />.
        /// </summary>
        public override int OnBeforeSave(uint docCookie) {
            LocalHistoryPackage.Log($"Entering {nameof(OnBeforeSave)}() on {nameof(LocalHistoryDocumentListener)}");
            uint pgrfRDTFlags;
            uint pdwReadLocks;
            uint pdwEditLocks;
            string pbstrMkDocument;
            IVsHierarchy ppHier;
            uint pitemid;
            IntPtr ppunkDocData;

            documentTable.GetDocumentInfo(
                docCookie,
                out pgrfRDTFlags,
                out pdwReadLocks,
                out pdwEditLocks,
                out pbstrMkDocument,
                out ppHier,
                out pitemid,
                out ppunkDocData);

            var filePath = Utils.NormalizePath(pbstrMkDocument);

            if (LocalHistoryPackage.Instance.OptionsPage.CreateRevisionOnlyIfDirty) {
                lock (_dirtyDocCookie) {
                    if (!_dirtyDocCookie.Contains(docCookie)) {
                        LocalHistoryPackage.Log($"File \"{filePath}\" is not dirty. Will not create version.");
                        return VSConstants.S_OK;
                    }

                    _dirtyDocCookie.Remove(docCookie);
                }
            }

            LocalHistoryPackage.Log($"Creating version for file \"{filePath}\" (is dirty).");
            var revNode = documentRepository.CreateRevision(filePath);

            var content = LocalHistoryPackage.Instance.ToolWindow?.Content as LocalHistoryControl;

            //only insert if this revision is different from all the others
            //otherwise we would be inserting duplicates in the list
            //remember that DocumentNode has its own GetHashCode
            ObservableCollection<DocumentNode> items = content?.DocumentItems;
            if (revNode != null && items?.Contains(revNode) == false) {
                LocalHistoryPackage.Log($"Adding file \"{filePath}\" to list.");
                items.Insert(0, revNode);
                LocalHistoryPackage.Instance.UpdateToolWindow("", true);
                return VSConstants.S_OK;
            }

            LocalHistoryPackage.Log($"File \"{filePath}\" is already in list. Doing nothing.");
            return VSConstants.S_OK;
        }

        /// <inheritdoc />
        public override int OnAfterAttributeChangeEx(
            uint docCookie,
            uint grfAttribs,
            IVsHierarchy pHierOld,
            uint itemidOld,
            string pszMkDocumentOld,
            IVsHierarchy pHierNew,
            uint itemidNew,
            string pszMkDocumentNew) {
            if (!LocalHistoryPackage.Instance.OptionsPage.CreateRevisionOnlyIfDirty) {
                return VSConstants.S_OK;
            }

            uint target = (uint)(__VSRDTATTRIB.RDTA_DocDataIsDirty);
            if (0 != (target & grfAttribs)) {
                lock (_dirtyDocCookie) {
                    _dirtyDocCookie.Add(docCookie);
                }
            } else {
                lock (_dirtyDocCookie) {
                    _dirtyDocCookie.Remove(docCookie);
                }
            }

            return VSConstants.S_OK;
        }

        #endregion

    }
}
