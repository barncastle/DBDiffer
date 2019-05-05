using System;
using System.Collections.Generic;
using System.Linq;

namespace DBDiffer.DiffGenerators
{
    internal class ReflectionDiffGenerator : IDiffGenerator
    {
        public bool HasMatchingFields => _fieldMap.AddedFields.Length + _fieldMap.RemovedFields.Length == 0;

        private readonly FieldMap _fieldMap;
        private readonly DBInfo _prevDB;
        private readonly DBInfo _curDB;

        public ReflectionDiffGenerator(DBInfo prevDB, DBInfo curDB)
        {
            _prevDB = prevDB;
            _curDB = curDB;
            _fieldMap = new FieldMap(prevDB.Fields.Keys, curDB.Fields.Keys);
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
            var diffs = new List<Diff>(_fieldMap.Count);

            foreach (var prop in _fieldMap.CommonFields)
                AppendChanges(a, b, prop, diffs);
            foreach (var prop in _fieldMap.AddedFields)
                diffs.Add(new Diff(DiffOperation.Add, prop, _curDB.GetFieldValue(b, prop)));
            foreach (var prop in _fieldMap.RemovedFields)
                diffs.Add(new Diff(DiffOperation.Remove, prop, _prevDB.GetFieldValue(a, prop)));

            return diffs;
        }

        private void AppendChanges(object a, object b, string path, List<Diff> diffs)
        {
            bool isArrayA = _prevDB.Fields[path].FieldType.IsArray;
            bool isArrayB = _curDB.Fields[path].FieldType.IsArray;

            if (isArrayA && isArrayB)
            {
                AppendArrayChanges(a, b, path, diffs);
            }
            else if (isArrayA != isArrayB)
            {
                throw new NotImplementedException("Field changed to/from array");
            }
            else
            {
                string valueA = _prevDB.GetFieldValue(a, path);
                string valueB = _curDB.GetFieldValue(b, path);

                if (!valueA.Equals(valueB))
                    diffs.Add(new Diff(DiffOperation.Replace, path, valueB, valueA));
            }
        }

        private void AppendArrayChanges(object a, object b, string path, List<Diff> diffs)
        {
            var a1 = _prevDB.Fields[path].GetValue(a) as Array;
            var a2 = _curDB.Fields[path].GetValue(b) as Array;

            int minLen = Math.Min(a1.Length, a2.Length);

            // direct compare of matching indicies
            string valueA, valueB;
            for (int i = 0; i < minLen; i++)
            {
                valueA = a1.GetValue(i).ToString();
                valueB = a2.GetValue(i).ToString();

                if (valueA != valueB)
                    diffs.Add(new Diff(DiffOperation.Replace, $"{path}[{i}]", valueB, valueA));
            }

            // array was made smaller
            if (a1.Length > minLen)
                for (int i = minLen; i < a1.Length; i++)
                    diffs.Add(new Diff(DiffOperation.Remove, $"{path}[{i}]", a1.GetValue(i).ToString()));

            // array was made bigger
            if (a2.Length > minLen)
                for (int i = minLen; i < a2.Length; i++)
                    diffs.Add(new Diff(DiffOperation.Add, $"{path}[{i}]", a2.GetValue(i).ToString()));
        }

    }
}
