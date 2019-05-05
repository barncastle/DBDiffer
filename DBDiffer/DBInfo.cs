using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DBDiffer.Helpers;

namespace DBDiffer
{
    internal class DBInfo
    {
        public readonly IDictionary Storage;
        public readonly Dictionary<string, FieldInfo> Fields;
        public readonly Type ValueType;

        private readonly DynamicHashCode _hashCode;

        public DBInfo(IDictionary dictionary)
        {
            Storage = dictionary;
            ValueType = dictionary.GetType().GetGenericArguments()[0];
            Fields = ValueType.GetFields().OrderBy(x => x.Name).ToDictionary(x => x.Name, x => x);

            _hashCode = new DynamicHashCode(ValueType);
        }

        public IEnumerable<int> Keys => Storage.Keys as ICollection<int>;

        public int GetRecordHash(int i) => _hashCode.GetHashCode(Storage[i]);
    }
}
