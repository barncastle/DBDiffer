using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DBDiffer
{
    internal class DBInfo
    {
        public readonly IDictionary Storage;
        public readonly Dictionary<string, FieldInfo> Fields;
        public readonly Type ValueType;

        public DBInfo(IDictionary dictionary)
        {
            Storage = dictionary;
            ValueType = dictionary.GetType().GetGenericArguments()[0];
            Fields = ValueType.GetFields().OrderBy(x => x.Name).ToDictionary(x => x.Name, x => x);
        }

        public IEnumerable<int> Keys => Storage.Keys as ICollection<int>;
    }
}
