02/28/2019 v2.1.2 (beta)
* Added proper support for long file paths. This is a workaround until we have fixed #4. More info on this issue: #10
* Fixed a few cases where the tool window wasn't being updated. In particular, this might help when you have projects that are lazy-loaded.

06/28/2018 v2.1.1 (beta)
* Fixed history not being saved when the file is not in a subdir relative to the sln file's dir. More info on this issue: #3

02/14/2018 v2.0.2
* Fixed internal diff not opening.
* Added option to force the internal diff tool to use a single window.

02/05/2018
First public release of v2.0.0
Main features
* settings page (Tools -> Options -> LocalHistory)
* external diff tool support
* labels (select the history item to add/change label, then use the L key)
* filtering for "labeled only" in the history panel
