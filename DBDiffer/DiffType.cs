using System;
using System.Collections.Generic;
using System.Text;

namespace DBDiffer
{
    public enum DiffType
    {
        /// <summary>
        /// Returns a list of Ids for Added/Removed records and a collection of diffs
        /// </summary>
        Simple,
        /// <summary>
        /// Returns a list of structures for Added/Removed/Changed records and a seperate collection for diffs
        /// </summary>
        Extended,
        /// <summary>
        /// Don't ask...
        /// </summary>
        WoWTools,
    }
}
