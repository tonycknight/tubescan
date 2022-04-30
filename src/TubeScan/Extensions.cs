using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;

namespace TubeScan
{
    internal static class Extensions
    {
        private static readonly TimeZoneInfo _ukTimeZone = GetUkTimeZone();

        [DebuggerStepThrough]
        public static int ToReturnCode(this bool value) => value ? 0 : 2;

        [DebuggerStepThrough]
        public static T ArgNotNull<T>(this T value, string paramName) where T : class
        {
            if (ReferenceEquals(null, value))
            {
                throw new ArgumentNullException(paramName: paramName);
            }
            return value;
        }

        [DebuggerStepThrough]
        public static T InvalidOpArg<T>(this T value, Func<T, bool> predicate, string message)
        {
            predicate.ArgNotNull(nameof(predicate));

            if (ReferenceEquals(null, value) || predicate(value))
            {
                throw new InvalidOperationException(message);
            }

            return value;
        }

        [DebuggerStepThrough]
        public static TResult Pipe<TValue, TResult>(this TValue value, Func<TValue, TResult> selector)
        {
            selector.ArgNotNull(nameof(selector));

            return selector(value);
        }


        [DebuggerStepThrough]
#pragma warning disable CS8601 // Possible null reference assignment.
        public static TResult PipeIfNotNull<TValue, TResult>(this TValue value, Func<TValue, TResult> selector, TResult defaultValue = default)
#pragma warning restore CS8601 // Possible null reference assignment.
                where TValue : class
        {
            selector.ArgNotNull(nameof(selector));

            return value != null
                ? selector(value)
                : defaultValue;
        }

        [DebuggerStepThrough]
        public static string Join(this IEnumerable<string> values, string delimiter) => string.Join(delimiter, values);

        [DebuggerStepThrough]
        public static string Format(this string value, string format) => string.Format(format, value);

        [DebuggerStepThrough]
        public static string GetAttributeValue<T>(this IEnumerable<Attribute> attributes, Func<T, string> selector)
            where T : Attribute
#pragma warning disable CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            => attributes.OfType<T>().FirstOrDefault().PipeIfNotNull(selector);
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

        [DebuggerStepThrough]
        public static Task<T> ToTaskResult<T>(this T value) => Task.FromResult(value);

        [DebuggerStepThrough]
        public static DateTime ToUkDateTime(this DateTime value)
            => System.TimeZoneInfo.ConvertTimeFromUtc(value, _ukTimeZone);


        [ExcludeFromCodeCoverage]
        private static TimeZoneInfo GetUkTimeZone()
        {
            var tzId = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? "Europe/London"
                : "GMT Standard Time";

            return TimeZoneInfo.GetSystemTimeZones().Single(tzi => tzi.Id == tzId);
        }

        [DebuggerStepThrough]
        public static string TrimEnd(this string value, string trailing)
        {
            if (value.EndsWith(trailing))
            {
                var i = value.LastIndexOf(trailing);
                return value.Substring(0, i);
            }
            return value;
        }

        [DebuggerStepThrough]
        public static TValue? GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> values, TKey key)
            where TKey : class
            where TValue : class
        {
            values.ArgNotNull(nameof(values));
            key.ArgNotNull(nameof(key));

            if (values.TryGetValue(key, out var value))
            {
                return value;
            }
            return default(TValue);
        }

        [DebuggerStepThrough]
        public static IEnumerable<T> NullToEmpty<T>(this IEnumerable<T> values)
            => values == null ? Enumerable.Empty<T>() : values;

        [DebuggerStepThrough]
        public static IEnumerable<T> Singleton<T>(this T value)
        {
            yield return value;
        }
    }
}
