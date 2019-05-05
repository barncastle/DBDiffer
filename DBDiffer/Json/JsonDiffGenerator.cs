using System;
using System.Collections.Generic;
using System.Linq;
using DBDiffer.Json.Lcs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DBDiffer.Json
{
    internal class JsonDiffGenerator
    {
        private readonly FieldMap fieldMap;

        public bool HasMatchingFields => fieldMap.AddedFields.Length + fieldMap.RemovedFields.Length == 0;

        public JsonDiffGenerator(DBInfo prevDB, DBInfo curDB)
        {
            var intersection = prevDB.Fields.Keys.Intersect(curDB.Fields.Keys);

            fieldMap = new FieldMap()
            {
                CommonFields = intersection.ToArray(),
                AddedFields = curDB.Fields.Keys.Except(intersection).ToArray(),
                RemovedFields = prevDB.Fields.Keys.Except(intersection).ToArray(),
            };
            fieldMap.Count = fieldMap.CommonFields.Length + fieldMap.AddedFields.Length + fieldMap.RemovedFields.Length;
        }

        public List<Diff> Generate(object a, object b)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));
            if (b == null)
                throw new ArgumentNullException(nameof(b));

            return GetChanges(a, b);
        }

        private List<Diff> GetChanges(object a, object b)
        {
            var otoken = (JObject)JToken.FromObject(a);
            var ntoken = (JObject)JToken.FromObject(b);

            var diffs = new List<Diff>(fieldMap.Count);

            foreach (var prop in fieldMap.CommonFields)
                AppendChanges(otoken[prop], ntoken[prop], prop, diffs);
            foreach (var prop in fieldMap.AddedFields)
                diffs.Add(new Diff(DiffOperation.Add, prop, ntoken[prop]));
            foreach (var prop in fieldMap.RemovedFields)
                diffs.Add(new Diff(DiffOperation.Remove, prop, otoken[prop]));

            return diffs;
        }

        private void AppendChanges(JToken a, JToken b, string path, List<Diff> diffs)
        {
            if (a.Type == JTokenType.Array && b.Type == JTokenType.Array)
            {
                AppendArrayChanges((JArray)a, (JArray)b, path, diffs);
            }
            else if (a is JValue && b is JValue)
            {
                AppendValueChanges((JValue)a, (JValue)b, path, diffs);
            }
            else
            {
                diffs.Add(new Diff(DiffOperation.Replace, path, b, a));
            }
        }

        private void AppendArrayChanges(JArray a1, JArray a2, string path, List<Diff> diffs)
        {
            var a1hash = a1.Select(x => x.ToString(Formatting.None)).ToArray();
            var a2hash = a2.Select(x => x.ToString(Formatting.None)).ToArray();
            var lcsComparisonResult = LcsComparer.Compare(a1hash, a2hash);

            LcsToJson(a1, a2, path, diffs, lcsComparisonResult);
        }

        private void LcsToJson(JArray a1, JArray a2, string path, List<Diff> diffs, LcsComparisonResult lcsComparisonResult)
        {
            var offset = 0;
            LcsComparer.Reduce(lcsComparisonResult, (lcsOperation, i, j) =>
            {
                var currentPathSegment = $"{path}[{j + offset}]";

                if (lcsOperation == LcsOperation.Remove)
                {
                    var lastDiff = diffs.Count > 0 ? diffs[diffs.Count - 1] : null;
                    var pathToPreviousIndex = $"{path}[{j + offset - 1}]";

                    if (lastDiff != null && lastDiff.Operation == DiffOperation.Add && lastDiff.Property == pathToPreviousIndex)
                    {
                        // Coalesce adjacent remove + add into replace
                        lastDiff.Operation = DiffOperation.Replace;
                        lastDiff.PreviousValue = ((JValue)a1[j + offset - 1]).Value;
                    }
                    else
                    {
                        diffs.Add(new Diff(DiffOperation.Remove, currentPathSegment));
                    }

                    offset -= 1;
                }
                else if (lcsOperation == LcsOperation.Add)
                {
                    diffs.Add(new Diff(DiffOperation.Add, currentPathSegment, a2[i]));
                    offset += 1;
                }
                else if (j < a1.Count && i < a2.Count)
                {
                    AppendChanges(a1[j], a2[i], currentPathSegment, diffs);
                }
            });
        }

        private void AppendValueChanges(JToken a, JValue b, string path, List<Diff> diffs)
        {
            if (a.Type == JTokenType.Object || !((JValue)a).Equals(b))
                diffs.Add(new Diff(DiffOperation.Replace, path, b.Value, ((JValue)a).Value));
        }

        private class FieldMap
        {
            public string[] CommonFields;
            public string[] AddedFields;
            public string[] RemovedFields;
            public int Count;
        }
    }
}
