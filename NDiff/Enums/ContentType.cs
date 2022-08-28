using NDiff.CustomAttributes;

namespace NDiff.Enums
{
    /// <summary>
    /// Used to enumerate the content types.
    /// </summary>
    public enum ContentType : byte
    {
        [EnumStringValue("text/json")] TextJson,

        [EnumStringValue("text/plain")] TextPlain,

        [EnumStringValue("application/json")] ApplicationJson
    }
}