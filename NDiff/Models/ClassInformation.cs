using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;

namespace NDiff.Models
{
    public class ClassInformation
    {
        /// <summary>
        /// The name of controller, known as Tag in OpenApi specification. The name does not include "Controller" extension if it exists.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Contains the routes of the controller.
        /// </summary>
        public List<CustomRoute> ControllerRoutes { get; set; }

        /// <summary>
        /// Contains the name of the controller.
        /// </summary>
        public string ControllerName { get; set; }

        /// <summary>
        /// If extendable, then all its derived classes will be controllers (this class as well), without restrictions, if the <see cref="ContainsNonApiAttribute"/> is false.
        /// </summary>
        public bool IsControllerExtendable { get; set; }

        /// <summary>
        /// If contains, then this class and all derived class will NOT be controllers.
        /// </summary>
        public bool ContainsNonApiAttribute { get; set; }

        /// <summary>
        /// If true, then it is a controller class otherwise it is not but still inherited classes can have access on its endpoints (if any).
        /// </summary>
        public bool IsClassController { get; set; }

        /// <summary>
        /// Contains the paths of the controller.
        /// </summary>
        public OpenApiPaths OpenApiPaths { get; set; }

        /// <summary>
        /// Contains the declaration syntax of the class.
        /// </summary>
        public ClassDeclarationSyntax ClassDeclarationSyntax { get; set; }

        /// <summary>
        /// Contains the class symbol.
        /// </summary>
        public ITypeSymbol ClassSymbol { get; set; }

        /// <summary>
        /// Area attribute value of this class. May be overridden by a non-null Area attribute in <see cref="MethodInformation.AreaAttribute"/>.
        /// </summary>
        public string AreaAttribute { get; set; }

        /// <summary>
        /// Contains the Consumes <see cref="MediaTypeCollection"/> that will be used in Request Body of each method.
        /// if there is no [FromBody] or no Complex type inside a Method parameter (e.g. Car class is complex type), then this will not be used.
        /// </summary>
        /// <seealso cref="MediaTypeCollection"/>
        public MediaTypeCollection ConsumesTypes { get; set; }

        /// <summary>
        /// Contains the ContentTypes <see cref="MediaTypeCollection"/> that will be used in <see cref="ResponseTypesWithoutContent"/>.
        /// </summary>
        /// <seealso cref="MediaTypeCollection"/>
        public MediaTypeCollection ContentTypes { get; set; }

        /// <summary>
        /// Contains as <see cref="KeyValuePair"/> the status code and the <see cref="OpenApiMediaType"/>. Then, at the end this will be
        /// matched with the <see cref="ContentTypes"/> to generate the <see cref="OpenApiResponses"/>.
        /// </summary>
        public Dictionary<int, OpenApiMediaType> ResponseTypesWithoutContent { get; set; }

        /// <summary>
        /// Contains the error response type which substitutes all those responses with status code >399 and less than 500 that <b>do not</b> have a type.
        /// In addition, this will also replace the <see cref="MethodInformation.ProducesDefaultResponseType"/>
        /// in case it does not have a type (<see cref="MethodInformation.ContainsDefaultResponseType"/> is true
        /// and <see cref="MethodInformation.ProducesDefaultResponseType"/> is null).
        /// </summary>
        public OpenApiMediaType ErrorResponseType { get; set; }

        /// <summary>
        /// Contains the details of methods that might be overridden within this controller. These methods contain one of <b>abstract</b> or
        /// <b>virtual</b> or <b>override</b> keywords.
        /// </summary>
        public Dictionary<IMethodSymbol, MethodInformation> OverriddenMethods { get; set; } = new();

        /// <summary>
        /// Contains the details of methods within this controller that are not in <see cref="OverriddenMethods"/>.
        /// </summary>
        public List<MethodInformation> Methods { get; set; } = new();

        /// <summary>
        /// If the default response type exists <see cref="Produces"/>, then, overwrite the 200 Status Code (the default one in Produces attribute) with the Produces attribute type.
        /// Produces attribute <b> type </b> takes precedence over all others (except of Produces attribute declared in the Method, which can overwrite this one!)
        /// </summary>
        public OpenApiMediaType Produces { get; set; }
    }
}