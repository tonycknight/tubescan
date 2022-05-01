using System.Diagnostics;
using System.Reflection;

namespace TubeScan
{
    internal static class Extensions
    {
        [DebuggerStepThrough]
        public static int ToReturnCode(this bool value) => value ? 0 : 2;

        [DebuggerStepThrough]
        public static string GetAttributeValue<T>(this IEnumerable<Attribute> attributes, Func<T, string> selector)
            where T : Attribute
#pragma warning disable CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            => attributes.OfType<T>().Select(selector).FirstOrDefault();
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.


        [DebuggerStepThrough]
        public static IEnumerable<(MemberInfo, T)> GetMemberAttributePairs<T>(this Type type)
            where T : Attribute
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            => type.GetMembers().Select(mi => (mi, mi.GetCustomAttributes()
                    .OfType<T>()
                    .FirstOrDefault()))
                    .Where(t => t.Item2 != null);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.

    }
}
