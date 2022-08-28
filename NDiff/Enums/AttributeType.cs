using NDiff.CustomAttributes;

namespace NDiff.Enums
{
    /// <summary>
    /// Used to enumerate attributes that can be used in a Web API.
    /// </summary>
    public enum AttributeType : byte
    {
        [EnumStringValue("ApiController")] ApiController,

        [EnumStringValue("NonController")] NonController,

        [EnumStringValue("RouteAttribute")] Route,

        [EnumStringValue("ProducesAttribute")] Produces,

        [EnumStringValue("ProducesResponseTypeAttribute")] ProducesResponseType,

        [EnumStringValue("ProducesErrorResponseTypeAttribute")] ProducesErrorResponseType,

        [EnumStringValue("ProducesDefaultResponseTypeAttribute")] ProducesDefaultResponseType,

        [EnumStringValue("ConsumesAttribute")] Consumes,

        [EnumStringValue("JsonExtensionDataAttribute")] JsonExtensionData,

        [EnumStringValue("AreaAttribute")] Area,

        [EnumStringValue("HttpPostAttribute")] Post,

        [EnumStringValue("HttpGetAttribute")] Get,

        [EnumStringValue("HttpPatchAttribute")] Patch,

        [EnumStringValue("HttpDeleteAttribute")] Delete,

        [EnumStringValue("HttpPutAttribute")] Put,

        [EnumStringValue("HttpOptionsAttribute")] Options,

        [EnumStringValue("HttpHeadAttribute")] Head,

        [EnumStringValue("FromBodyAttribute")] FromBody,

        [EnumStringValue("FromQueryAttribute")] FromQuery,

        [EnumStringValue("FromFormAttribute")] FromForm,

        [EnumStringValue("FromHeaderAttribute")] FromHeader,

        [EnumStringValue("FromRouteAttribute")] FromRoute,

        [EnumStringValue("RequiredAttribute")] Required,

        [EnumStringValue("AcceptVerbsAttribute")] AcceptVerbs,

        [EnumStringValue("ApiVersionAttribute")] ApiVersion
    }
}