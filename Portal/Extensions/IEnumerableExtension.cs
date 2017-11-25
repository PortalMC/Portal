using System;
using System.Collections.Generic;
using System.Linq;

namespace Portal.Extensions
{
    public static class IEnumerableExtension
    {
        public static string JoinString<T>(this IEnumerable<T> values, string glue, Func<T, string> converter = null)
        {
            return converter != null ? string.Join(glue, values.Select(converter)) : string.Join(glue, values);
        }
    }
}