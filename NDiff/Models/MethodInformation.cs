using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;

namespace NDiff.Models
{
    public class MethodInformation
    {
        /// <summary>
        /// Contains the name of the method.
        /// </summary>
        public string MethodName { get; init; }
        
        /// <summary>
        /// Contains the routes of the method.
        /// </summary>
        public Dictionary<OperationType, List<CustomRoute>> MethodHttpRoutes { get; set; } = new();

        /// <summary>
        /// Contains all <see cref="OpenApiParameters"/> of the method.
        /// The key is the Parameter Position whereas the value is the list of <see cref="OpenApiParameter"/> for that
        /// parameter. This will be used to compare the parameters of the Child method that overrides
        /// the one in parent class.
        /// If <see cref="OpenApiParameter.In"/> is null and <see cref="IsFromBodySet"/> it is a [FromBody] attribute.
        /// If <see cref="OpenApiParameter.In"/> is null and is a complexType (object, array or has reference) and <see cref="IsFromBodySet"/> is false
        /// then, it is an implicit [FromBody].
        /// If <see cref="OpenApiParameter"/> is null and it is a Custom Class (has <see cref="OpenApiSchema.Reference"/>, then this is de-constructed and
        /// that parameter location has a list of <see cref="OpenApiParameter"/>.
        /// </summary>
        public Dictionary<int, List<OpenApiParameter>> OpenApiParameters { get; set; }
        
        /// <summary>
        /// Contains the index of the explicit FromBody parameter in <see cref="OpenApiParameters"/>.
        /// </summary>
        public int FromBodyParameterLocation { get; set; }

        /// <summary>
        /// Contains the value of Area attribute. It will overwrite the one of controller <see cref="ClassInformation.AreaAttribute"/>.
        /// </summary>
        public string AreaAttribute { get; set; }

        /// <summary>
        /// Contains the ReturnType written in method signature. 
        /// </summary>
        public ITypeSymbol ReturnType { get; set; }

        /// <summary>
        /// Includes all consumes types. It will be integrated with <see cref="ClassInformation.ConsumesTypes"/> and afterwards
        /// it will be put in the <see cref="OpenApiRequestBody"/>. If there is no complex type in the method arguments and if there
        /// is no [FromBody] attribute in one of the arguments then <see cref="ConsumesTypes"/> will be irrelevant.
        /// </summary>
        public MediaTypeCollection ConsumesTypes { get; set; }

        /// <summary>
        /// Content types of this method. Content types of the <see cref="ClassInformation.ContentTypes"/>
        /// will be integrated with <see cref="ResponseTypesWithoutContent"/>.
        /// </summary>
        public MediaTypeCollection ContentTypes { get; set; }

        /// <summary>
        /// Contains as <see cref="KeyValuePair"/> the status code and the <see cref="OpenApiMediaType"/>. Then, at the end this will be
        /// matched with the <see cref="ContentTypes"/> to generate the <see cref="OpenApiResponses"/>.
        /// </summary>
        public Dictionary<int, OpenApiMediaType> ResponseTypesWithoutContent { get; set; }

        /// <summary>
        /// Contains the error response type which substitutes all those responses with status code >399 and less than 500 that <b>do not</b> have a type.
        /// In addition, this will also replace the <see cref="ProducesDefaultResponseType"/>
        /// in case it does not have a type (<see cref="ContainsDefaultResponseType"/> is true
        /// and <see cref="ProducesDefaultResponseType"/> is null). 
        /// </summary>
        public OpenApiMediaType ErrorResponseType { get; set; }

        /// <summary>
        /// Contains the default response type. The status code will be "default".
        /// If the parent method has a default response type and child does not have then it takes from the parent.
        /// If the child method has default response type it ignores the others!
        /// </summary>
        public OpenApiMediaType ProducesDefaultResponseType { get; set; }

        /// <summary>
        /// A flag used to represent that the method has a ProducesDefaultResponseType which may have a Type set at <see cref="ProducesDefaultResponseType"/>
        /// or not (when the <see cref="ProducesDefaultResponseType"/> is null).
        /// </summary>
        public bool ContainsDefaultResponseType { get; set; }

        /// <summary>
        /// If the default response type exists, then, overwrite the 200 Status Code (the default one in Produces attribute) with the Produces attribute type.
        /// Produces attribute <b> type </b> takes precedence over all others (if one exists in Controller, this overwrites it).
        /// </summary>
        public OpenApiMediaType Produces { get; set; }

        /// <summary>
        /// Returns if the <see cref="FromBodyParameterLocation"/> is set and not equal to -1.
        /// </summary>
        public bool IsFromBodySet => FromBodyParameterLocation != -1;
        
        /// <summary>
        /// Returns if the <see cref="FromBodyParameterLocation"/> is equal to <paramref name="parameterLocation"/>.
        /// </summary>
        /// <param name="parameterLocation">Parameter location to assert to.</param>
        /// <returns>True if equal; otherwise false.</returns>
        public bool HasParameterLocation(int parameterLocation) => FromBodyParameterLocation == parameterLocation;
    }
 
}