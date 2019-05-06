using System;

namespace DBDiffer.Helpers.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class DiffTypeAttribute : Attribute
    {
        public readonly DiffType DiffType;

        public DiffTypeAttribute(DiffType diffType) => DiffType = diffType;
    }
}
