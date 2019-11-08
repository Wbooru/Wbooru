using System;

namespace Wbooru.Settings
{
    [Flags]
    public enum TagFilterTarget
    {
        MainWindow = 2 << 1,
        SearchResultWindow = 2 << 2,
        MarkedWindow = 2 << 3,
        VotedWindow = 2 << 4,
    }
}