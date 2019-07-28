Local History for Visual Studio
===========================

Local History for Visual Studio automatically creates a history of your files every time they are saved. The history can be compared with the current version.


Requirements
----------------------------------------------
This is an experimental version for visual studio 2019, I don't intend to suppport older versions.


Usage
----------------------------------------------
- Install from the [marketplace](https://marketplace.visualstudio.com/items?itemName=lostalloy.LocalHistory-for-Visual-Studio#overview)
- Open the Local History from the View -> Other Windows menu OR
- Open the Local History by right clicking on a project item to display the history
- Double-click the revision to compare it with the latest version.
- Add/remove/edit labels by selecting the history item then using the L key
- Settings page in Tools -> Options -> LocalHistory


Notes
----------------------------------------------
This is not a substitute for source control or backups. Take care of your code files.


Contributing
----------------------------------------------
PR and issue reports are welcome.


Acknowledgments
----------------------------------------------
This project is based on the excellent [local-history-visual-studio](https://github.com/curzona/local-history-visual-studio).
Since the original repository has little activity, I decided to fork it and add a few features (and support for newer VisualStudio versions).


History
----------------------------------------------
See [changelog](LocalHistory/CHANGELOG.MD)


License
----------------------------------------------
[Apache License Version 2.0](LocalHistory/License.txt)