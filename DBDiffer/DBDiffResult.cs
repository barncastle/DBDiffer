using System.Collections.Generic;

namespace DBDiffer
{
    public class DBDiffResult
    {
        public int[] AddedRecords;
        public int[] RemovedRecords;
        public IDictionary<int, List<Diff>> ChangedRecords;
    }
}
