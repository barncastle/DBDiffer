using System.Collections.Generic;
using System.IO;
using System.Linq;
using DBDiffer.Helpers.Attributes;
using Newtonsoft.Json;

namespace DBDiffer.DiffResults
{
    [DiffType(DiffType.Simple)]
    public class SimpleDiffResult : IDiffResult
    {
        public readonly int[] AddedRecords;
        public readonly int[] RemovedRecords;
        public readonly IDictionary<int, List<Diff>> ChangedRecords;

        internal SimpleDiffResult(DBInfo PreviousDB, DBInfo CurrentDB, IDictionary<int, List<Diff>> diffs)
        {
            var intersection = PreviousDB.Keys.Intersect(CurrentDB.Keys);

            AddedRecords = CurrentDB.Keys.Except(intersection).ToArray();
            RemovedRecords = PreviousDB.Keys.Except(intersection).ToArray();
            ChangedRecords = diffs;
        }


        public void Save(string path, Formatting formatting = Formatting.None)
        {
            using (var fs = File.CreateText(path))
                fs.Write(JsonConvert.SerializeObject(this, formatting));
        }

        public string ToJSONString(Formatting formatting = Formatting.None)
        {
            return JsonConvert.SerializeObject(this, formatting);
        }
    }
}
