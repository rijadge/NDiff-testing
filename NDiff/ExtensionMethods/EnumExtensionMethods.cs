using System;
using System.Linq;
using NDiff.CustomAttributes;

namespace NDiff.ExtensionMethods
{
    public static class EnumExtensionMethods
    {
        /// <summary>
        /// Used to read string value of an enum field.
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns>The string value of an <see cref="Enum"/> field.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string GetStringValue(this Enum enumValue)
        {
            if (enumValue == null) return null;

            var type = enumValue.GetType();
            var fieldInfo = type.GetField(enumValue.ToString());
            var attribute = (EnumStringValueAttribute) fieldInfo?.GetCustomAttributes(
                typeof(EnumStringValueAttribute), false).FirstOrDefault();

            return attribute?.StringValue;
        }
    }
}