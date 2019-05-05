using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DBDiffer.DiffGenerators;

namespace DBDiffer
{
    public class DBComparer
    {
        public bool UseReflection { get; set; } = true;

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
            var differ = GeneratorFactory();
            bool useHashCode = !UseReflection && differ.HasMatchingFields;


            var resultset = new Dictionary<int, List<Diff>>(keys.Count());
            Parallel.ForEach(keys, key =>
            {
                // shortcut for Json comparison to reduce the amount of iterations required
                // the difference is negligable with reflection
                if (useHashCode && PreviousDB.GetRecordHash(key) == CurrentDB.GetRecordHash(key))
                    return;

                var diffs = differ.Generate(PreviousDB.Storage[key], CurrentDB.Storage[key]);
                if (diffs.Count == 0)
                    return;

                lock (resultset)
                    resultset.Add(key, diffs);
            });

            return resultset;
        }

        private IDiffGenerator GeneratorFactory()
        {
            if (UseReflection)
                return (IDiffGenerator)Activator.CreateInstance(typeof(ReflectionDiffGenerator), PreviousDB, CurrentDB);

            return (IDiffGenerator)Activator.CreateInstance(typeof(JsonDiffGenerator), PreviousDB, CurrentDB);
        }
    }
}
