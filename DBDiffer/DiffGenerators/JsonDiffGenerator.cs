using System;
using System.Collections.Generic;
using System.Linq;
using DBDiffer.Helpers.Lcs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DBDiffer.DiffGenerators
{
    internal class JsonDiffGenerator : IDiffGenerator
    {
        public bool UseLCSArrayMatching { get; set; } = false;
        public bool HasMatchingFields => _fieldMap.AddedFields.Length + _fieldMap.RemovedFields.Length == 0;

        private readonly FieldMap _fieldMap;
        

        public JsonDiffGenerator(DBInfo prevDB, DBInfo curDB)
        {
            var intersection = prevDB.Fields.Keys.Intersect(curDB.Fields.Keys);

            _fieldMap = new FieldMap()
            {
                CommonFields = intersection.ToArray(),
                AddedFields = curDB.Fields.Keys.Except(intersection).ToArray(),
                RemovedFields = prevDB.Fields.Keys.Except(intersection).ToArray(),
            };
            _fieldMap.Count = _fieldMap.CommonFields.Length + _fieldMap.AddedFields.Length + _fieldMap.RemovedFields.Length;
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

            var diffs = new List<Diff>(_fieldMap.Count);

            foreach (var prop in _fieldMap.CommonFields)
                AppendChanges(otoken[prop], ntoken[prop], prop, diffs);
            foreach (var prop in _fieldMap.AddedFields)
                diffs.Add(new Diff(DiffOperation.Add, prop, ntoken[prop]));
            foreach (var prop in _fieldMap.RemovedFields)
                diffs.Add(new Diff(DiffOperation.Remove, prop, otoken[prop]));

            return diffs;
        }

        private void AppendChanges(JToken a, JToken b, string path, List<Diff> diffs)
        {
            if (a.Type == JTokenType.Array && b.Type == JTokenType.Array)
            {
                AppendArrayChanges((JArray)a, (JArray)b, path, diffs);
            }
            else if (a is JValue aval && b is JValue bval)
            {
                if (aval.Type == JTokenType.Object || !aval.Equals(bval))
                    diffs.Add(new Diff(DiffOperation.Replace, path, bval.Value, aval.Value));
            }
            else
            {
                diffs.Add(new Diff(DiffOperation.Replace, path, b, a));
            }
        }

        private void AppendArrayChanges(JArray a1, JArray a2, string path, List<Diff> diffs)
        {
            // normalise the array's tokens
            var a1hash = a1.Select(x => x.ToString(Formatting.None)).ToArray();
            var a2hash = a2.Select(x => x.ToString(Formatting.None)).ToArray();

            if(!UseLCSArrayMatching)
            {
                // basic oridinal comparison
                OrdinalCompare(a1hash, a2hash, path, diffs);
            }
            else
            {
                // longest common sequence comparision - to try to "intelligently" detect changes
                // https://perl.plover.com/diff/Manual.html
                var lcsComparisonResult = LcsComparer.Compare(a1hash, a2hash);
                LcsToJson(a1, a2, path, diffs, lcsComparisonResult);
            }
        }

        private void OrdinalCompare(string[] a1, string[] a2, string path, List<Diff> diffs)
        {
            int minLen = Math.Min(a1.Length, a2.Length);

            for (int i = 0; i < minLen; i++)
                if (a1[i] != a2[i])
                    diffs.Add(new Diff(DiffOperation.Replace, $"{path}[{i}]", a2[i], a1[i]));

            if (a1.Length > minLen)
                for (int i = minLen; i < a1.Length; i++)
                    diffs.Add(new Diff(DiffOperation.Remove, $"{path}[{i}]", a1[i]));

            if (a2.Length > minLen)
                for (int i = minLen; i < a2.Length; i++)
                    diffs.Add(new Diff(DiffOperation.Add, $"{path}[{i}]", a2[i]));
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

    }
}
