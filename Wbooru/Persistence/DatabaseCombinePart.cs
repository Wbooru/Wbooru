using System;
using System.Collections.Generic;
using System.Text;

namespace Wbooru.Persistence
{
    [Flags]
    public enum DatabaseCombinePart
    {
        Downloads = 1,
        Tags = 2,
        GalleryItems = 4,
        VisitRecords = 8,
        ItemMarks = 16,
        All = Downloads | Tags | GalleryItems | VisitRecords | ItemMarks
    }
}
