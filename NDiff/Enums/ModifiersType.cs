using NDiff.CustomAttributes;

namespace NDiff.Enums
{
    /// <summary>
    /// Used to enumerate some of the main modifiers.
    /// </summary>
    public enum ModifiersType : byte
    {
        [EnumStringValue("public")] Public,

        [EnumStringValue("protected")] Protected,

        [EnumStringValue("private")] Private,

        [EnumStringValue("internal")] Internal,

        [EnumStringValue("static")] Static,

        [EnumStringValue("abstract")] Abstract
    }
}