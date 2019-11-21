namespace LOSTALLOY.LocalHistory
{
    using System;

    public class PathChangedEventArgs: EventArgs
    {
        public string OldPath { get; set; }

        public string NewPath { get; set; }
    }
}
