using NDiff.CustomAttributes;

namespace NDiff.Enums
{
    /// <summary>
    /// Used to enumerate the names of different types of classes. 
    /// </summary>
    public enum ClassType : byte
    {
        [EnumStringValue("Object")] Object,

        [EnumStringValue("Controller")] Controller
    }
}