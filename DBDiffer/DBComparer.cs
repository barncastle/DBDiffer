using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DBDiffer.DiffGenerators;
using DBDiffer.DiffResults;
using DBDiffer.Helpers.Attributes;

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

        public IDiffResult Diff(DiffType type)
        {
            var diffs = ObjectDiff(PreviousDB.Keys.Intersect(CurrentDB.Keys));
            return DiffResultFactory(type, diffs);
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
            Type objtype = UseReflection ? typeof(ReflectionDiffGenerator) : typeof(JsonDiffGenerator);
            object[] args = new object[] { PreviousDB, CurrentDB };

            return (IDiffGenerator)Activator.CreateInstance(objtype, args);
        }

        private IDiffResult DiffResultFactory(DiffType type, IDictionary<int, List<Diff>> diffs)
        {
            Type objtype = Assembly.GetExecutingAssembly().GetTypes().First(x => x.GetCustomAttribute<DiffTypeAttribute>()?.DiffType == type);
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            object[] args = new object[] { PreviousDB, CurrentDB, diffs };

            return (IDiffResult)Activator.CreateInstance(objtype, flags, null, args, null);
        }
    }
}
