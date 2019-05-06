using System.Collections.Generic;
using System.Linq;

namespace DBDiffer.DiffGenerators
{
    internal class FieldMap
    {
        public string[] CommonFields;
        public string[] AddedFields;
        public string[] RemovedFields;
        public int Count;


        public FieldMap() { }

        public FieldMap(IEnumerable<string> prevfields, IEnumerable<string> curfields)
        {
            var intersection = prevfields.Intersect(curfields);

            CommonFields = intersection.ToArray();
            AddedFields = curfields.Except(intersection).ToArray();
            RemovedFields = prevfields.Except(intersection).ToArray();

            Count = CommonFields.Length + AddedFields.Length + RemovedFields.Length;
        }
    }
}
