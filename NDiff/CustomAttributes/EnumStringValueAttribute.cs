using System;

namespace NDiff.CustomAttributes
{
    /// <summary>
    /// Attribute used to represent a string value in an enum.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class EnumStringValueAttribute : Attribute
    {
        public string StringValue { get; }

        /// <summary>
        /// Constructor used to init a StringValue attribute.
        /// </summary>
        /// <param name="stringValue">Value.</param>
        public EnumStringValueAttribute(string stringValue)
        {
            StringValue = stringValue;
        }
    }
}