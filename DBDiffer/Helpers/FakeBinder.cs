using System;
using System.Dynamic;

namespace DBDiffer.Helpers
{
    internal class FakeBinder : GetMemberBinder
    {
        public FakeBinder(string name) : base(name, false) { }

        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion) => throw new NotImplementedException();
    }
}
