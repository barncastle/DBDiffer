using System.Collections.Generic;
using System.IO;
using System.Linq;
using DBDiffer.Helpers.Attributes;
using Newtonsoft.Json;

namespace DBDiffer.DiffResults
{
    [DiffType(DiffType.Extended)]
    public class ExtendedDiffResult : IDiffResult
    {
        public readonly IDictionary<int, object> AddedRecords;
        public readonly IDictionary<int, object> RemovedRecords;
        public readonly IDictionary<int, object> ChangedRecords;
        public readonly IDictionary<int, List<Diff>> Diffs;

        internal ExtendedDiffResult(DBInfo PreviousDB, DBInfo CurrentDB, IDictionary<int, List<Diff>> diffs)
        {
            var intersection = PreviousDB.Keys.Intersect(CurrentDB.Keys);
            var added = CurrentDB.Keys.Except(intersection).Select(x => new { Id = x, Rec = CurrentDB.Storage[x] });
            var removed = PreviousDB.Keys.Except(intersection).Select(x => new { Id = x, Rec = PreviousDB.Storage[x] });

            AddedRecords = added.ToDictionary(x => x.Id, x => x.Rec);
            RemovedRecords = removed.ToDictionary(x => x.Id, x => x.Rec);
            ChangedRecords = diffs.ToDictionary(x => x.Key, x => PreviousDB.Storage[x.Key]);
            Diffs = diffs;
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
