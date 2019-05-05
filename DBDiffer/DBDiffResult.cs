using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace DBDiffer
{
    public class DBDiffResult
    {
        public int[] AddedRecords;
        public int[] RemovedRecords;
        public IDictionary<int, List<Diff>> ChangedRecords;

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
