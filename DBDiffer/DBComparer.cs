using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DBDiffer.Json;

namespace DBDiffer
{
    public class DBComparer
    {
        private readonly DBInfo PreviousDB;
        private readonly DBInfo CurrentDB;

        public DBComparer(IDictionary previousdb, IDictionary currentdb)
        {
            PreviousDB = new DBInfo(previousdb);
            CurrentDB = new DBInfo(currentdb);
        }

        public DBDiffResult Diff()
        {
            var intersection = PreviousDB.Keys.Intersect(CurrentDB.Keys);

            DBDiffResult result = new DBDiffResult()
            {
                AddedRecords = CurrentDB.Keys.Except(intersection).ToArray(),
                RemovedRecords = PreviousDB.Keys.Except(intersection).ToArray(),
                ChangedRecords = ObjectDiff(intersection)
            };

            return result;
        }

        private IDictionary<int, List<Diff>> ObjectDiff(IEnumerable<int> keys)
        {
            var differ = new JsonDiffGenerator(PreviousDB, CurrentDB);

            var resultset = new Dictionary<int, List<Diff>>(keys.Count());
            Parallel.ForEach(keys, key =>
            {
                var diffs = differ.Generate(PreviousDB.Storage[key], CurrentDB.Storage[key]);
                if (diffs.Count == 0)
                    return;

                lock (resultset)
                    resultset.Add(key, diffs);
            });

            return resultset;
        }
    }
}
