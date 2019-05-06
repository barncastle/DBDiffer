using Newtonsoft.Json;

namespace DBDiffer.DiffResults
{
    public interface IDiffResult
    {
        void Save(string path, Formatting formatting = Formatting.None);
        string ToJSONString(Formatting formatting = Formatting.None);
    }
}
