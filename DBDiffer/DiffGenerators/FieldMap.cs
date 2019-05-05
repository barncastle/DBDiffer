using System;
using System.Collections.Generic;
using System.Text;

namespace DBDiffer.DiffGenerators
{
    class FieldMap
    {
        public string[] CommonFields;
        public string[] AddedFields;
        public string[] RemovedFields;
        public int Count;
    }
}
