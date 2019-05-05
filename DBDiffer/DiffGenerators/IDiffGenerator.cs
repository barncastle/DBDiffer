using System;
using System.Collections.Generic;
using System.Text;

namespace DBDiffer.DiffGenerators
{
    internal interface IDiffGenerator
    {
        bool HasMatchingFields { get; }

        List<Diff> Generate(object a, object b);
    }
}
