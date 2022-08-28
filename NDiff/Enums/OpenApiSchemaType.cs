using NDiff.CustomAttributes;

namespace NDiff.Enums
{
    /// <summary>
    /// Used to enumerate the data-types supported by OpenApi.
    /// Check https://swagger.io/docs/specification/data-models/data-types/
    /// </summary>
    public enum OpenApiSchemaType
    {
        [EnumStringValue("integer")] Int,

        [EnumStringValue("string")] String,

        [EnumStringValue("boolean")] Bool,

        [EnumStringValue("number")] Number,

        [EnumStringValue("array")] Array,

        [EnumStringValue("object")] Object
    }

    /// <summary>
    /// Used to enumerate the Formats supported by OpenApi.
    /// </summary>
    public enum OpenApiSchemaFormat
    {
        [EnumStringValue("uint16")] UInt16,

        [EnumStringValue("uint32")] UInt32,

        [EnumStringValue("uint64")] UInt64,

        [EnumStringValue("int16")] Int16,

        [EnumStringValue("int32")] Int32,

        [EnumStringValue("uint8")] UInt8,
        
        [EnumStringValue("sint8")] SInt8,

        [EnumStringValue("int64")] Int64,

        [EnumStringValue("double")] Double,

        [EnumStringValue("float")] Float,

        [EnumStringValue("date-time")] Datetime
    }
}