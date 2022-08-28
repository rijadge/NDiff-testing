using NDiff.CustomAttributes;

namespace NDiff.Enums
{
    /// <summary>
    /// Used to enumerate the namespaces of base Mvc controller classes that create api endpoints. 
    /// </summary>
    public enum BaseControllerNamespace : byte
    {
        [EnumStringValue("Microsoft.AspNetCore.Mvc.Controller")]
        Controller,

        [EnumStringValue("Microsoft.AspNetCore.Mvc.ControllerBase")]
        ControllerBase
    }
}