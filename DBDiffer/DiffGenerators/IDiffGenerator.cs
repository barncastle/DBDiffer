using System.Collections.Generic;

namespace DBDiffer.DiffGenerators
{
    internal interface IDiffGenerator
    {
        bool HasMatchingFields { get; }

        List<Diff> Generate(object a, object b);
    }
}
