using Microsoft.CodeAnalysis;
using NDiff.Enums;

namespace NDiff.Models
{
    public class Parameter
    {
        /// <summary>
        /// Contains the attribute type of the parameter.
        /// One of:
        /// <see cref="Enums.AttributeType.FromRoute"/>,
        /// <see cref="Enums.AttributeType.FromBody"/>,
        /// <see cref="Enums.AttributeType.FromQuery"/>,
        /// <see cref="Enums.AttributeType.FromForm"/>,
        /// <see cref="Enums.AttributeType.FromHeader"/>.
        /// </summary>
        public AttributeType? AttributeType { get; set; }
        
        /// <summary>
        /// If the parameter has <see cref="Enums.AttributeType.Required"/>, it is set to true.
        /// </summary>
        public bool IsRequired { get; set; }
        
        /// <summary>
        /// If the parameter is a complex type (e.g. class), it is set to true.
        /// </summary>
        public bool IsComplexParameter { get; set; }
        
        /// <summary>
        /// If the parameter is a reference type, it is set to true.
        /// </summary>
        public bool IsReferenceType { get; set; }
        
        /// <summary>
        /// Contains the parameter symbol.
        /// </summary>
        public IParameterSymbol ParameterSymbol { get; set; }
        
        /// <summary>
        /// Returns if the parameter is from Route or does not have an <see cref="AttributeType"/>.
        /// </summary>
        public bool HasNullOrRouteAttributeType => AttributeType is null or Enums.AttributeType.Route;
    }
}