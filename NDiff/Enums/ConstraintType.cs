using NDiff.CustomAttributes;

namespace NDiff.Enums
{
    /// <summary>
    /// Used to enumerate the constraint data-type supported in Route parameters.
    /// Check https://docs.microsoft.com/en-us/aspnet/web-api/overview/web-api-routing-and-actions/attribute-routing-in-web-api-2#code-try-13
    /// </summary>
    public enum ConstraintType
    {
        [EnumStringValue("int")] Int,

        [EnumStringValue("long")] Long,

        [EnumStringValue("bool")] Bool,

        [EnumStringValue("decimal")] Decimal,

        [EnumStringValue("double")] Double,

        [EnumStringValue("float")] Float,

        [EnumStringValue("datetime")] Datetime,

        [EnumStringValue("alpha")] Alpha,

        [EnumStringValue("guid")] Guid,

        [EnumStringValue("length(")] Length,

        [EnumStringValue("max(")] Max,

        [EnumStringValue("maxlength(")] MaxLength,

        [EnumStringValue("min(")] Min,

        [EnumStringValue("minlength(")] MinLength,

        [EnumStringValue("range(")] Range,

        [EnumStringValue("regex(")] Regex
    }
}