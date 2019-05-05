using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DBDiffer.Helpers
{
    internal class DynamicHashCode
    {
        private readonly Delegate _hashFunc;
        private readonly Type _type;
        private readonly FieldInfo[] _fields;

        public DynamicHashCode(Type type)
        {
            _type = type;
            _fields = type.GetFields();
            _hashFunc = BuildDelegate().Compile();
        }

        public int GetHashCode(object o) =>(int)_hashFunc.DynamicInvoke(o);

        private LambdaExpression BuildDelegate()
        {
            var miGetArrHashCode = GetType().GetMethod("GetArrHashCode", BindingFlags.NonPublic | BindingFlags.Instance);

            var arg = Expression.Parameter(_type);

            var body = _fields.Select<FieldInfo, Expression>(x =>
            {
                if (x.FieldType.IsArray)
                {
                    var toArray = Expression.Convert(Expression.Field(arg, x), typeof(Array));
                    return Expression.Call(Expression.Constant(this), miGetArrHashCode, toArray);
                }
                else
                {
                    var toString = Expression.Call(Expression.Field(arg, x), "ToString", Type.EmptyTypes);
                    return Expression.Call(toString, "GetHashCode", Type.EmptyTypes);
                }
            })
            .Aggregate((x, y) => Expression.ExclusiveOr(Expression.Multiply(x, Expression.Constant(16777619)), y));

            var prime = Expression.ExclusiveOr(Expression.Constant(int.MaxValue), body);

            return Expression.Lambda(prime, arg);
        }

        private int GetArrHashCode(Array items)
        {
            unchecked
            {
                int tmp = 0;
                foreach (var item in items)
                    tmp = ((tmp << 5) + tmp) ^ item.ToString().GetHashCode(); // based on System.Tuple
                return tmp;
            }
        }
    }
}
