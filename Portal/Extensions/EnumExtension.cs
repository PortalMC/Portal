using System;
using System.Linq;

namespace Portal.Extensions
{
    public static class EnumExtension
    {
        [AttributeUsage(AttributeTargets.Field)]
        public class ValueAttribute : Attribute
        {
            internal string Value { get; }

            public ValueAttribute(string value)
            {
                Value = value;
            }
        }

        public static string GetValue(this Enum value)
        {
            return value.GetAttribute<ValueAttribute>()?.Value;
        }

        public static TAttribute GetAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            var attributes = fieldInfo.GetCustomAttributes(typeof(TAttribute), false).Cast<TAttribute>();
            var enumerable = attributes as TAttribute[] ?? attributes.ToArray();
            return !enumerable.Any() ? null : enumerable.First();
        }
    }
}