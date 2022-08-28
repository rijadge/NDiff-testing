using System.Linq;
using NDiff.Enums;
using NDiff.ExtensionMethods;
using NDiff.Models;
using NDiff.Services.Analyzers.AttributeAnalyzers.Attributes.ContentTypes;
using NDiff.Services.Analyzers.AttributeAnalyzers.Attributes.ResponseTypes;
using NDiff.Services.Analyzers.AttributeAnalyzers.Attributes.Routes;

namespace NDiff.Services.Analyzers.AttributeAnalyzers.Attributes
{
    public class ClassAttributesAnalyzer
    {
        /// <summary>
        /// Analyzes the global class attributes that will be used by methods.
        /// </summary>
        /// <param name="classInformation"></param> 
        public static void AnalyzeClassAttributes(ClassInformation classInformation)
        {
            var classSymbol = classInformation.ClassSymbol;
            var classSyntax = classInformation.ClassDeclarationSyntax;


            classInformation.Tag = classSyntax.EndsWithController()
                ? classInformation.ControllerName.Remove(classInformation.ControllerName.Length - 10)
                : classInformation.ControllerName;


            // analyze controller Area attribute, this is then given to the Route
            var controllerAreaAttributes = classSymbol.GetAttributesOfType(AttributeType.Area);
            var areaAnalyzer = new AreaAnalyzer(controllerAreaAttributes.ToArray());
            areaAnalyzer.AnalyzeAttributes();
            classInformation.AreaAttribute = areaAnalyzer.AreaValue;

            // analyze controller Route attribute
            var controllerRouteAttributes = classSymbol.GetAttributesOfType(AttributeType.Route);
            var cRoutes = new RouteAnalyzer(controllerRouteAttributes.ToArray());
            cRoutes.AnalyzeAttributes();
            classInformation.ControllerRoutes = cRoutes.CustomRoutes;

            // analyze controller Produce attribute
            var controllerProducesAttributes = classSymbol.GetAttributesOfType(AttributeType.Produces);
            var producesAnalyzer =
                new ProducesAnalyzer(controllerProducesAttributes.ToArray());
            producesAnalyzer.AnalyzeAttributes();
            classInformation.ContentTypes = producesAnalyzer.ContentTypes;
            classInformation.Produces = producesAnalyzer.ProducesOpenApiMedia;


            // analyze controller ProduceErrorResponseType
            var controllerErrorResponseTypeAttributes =
                classSymbol.GetAttributesOfType(AttributeType.ProducesErrorResponseType);
            var producesError = new CustomResponseTypeAnalyzer(controllerErrorResponseTypeAttributes.ToArray());
            producesError.AnalyzeAttributes();
            classInformation.ErrorResponseType = producesError.OpenApiMedia;


            // analyze controller ProduceResponseType attribute
            var controllerProducesResponseTypesAttributes =
                classSymbol.GetAttributesOfType(AttributeType.ProducesResponseType).ToList();
            var producesResponseType =
                new ProducesResponseTypeAnalyzer(controllerProducesResponseTypesAttributes.ToArray());
            producesResponseType.AnalyzeAttributes();
            classInformation.ResponseTypesWithoutContent = producesResponseType.MediatTypesForStatusCodes;

            // analyze controller Consumes attribute
            var controllerConsumesAttributes = classSymbol.GetAttributesOfType(AttributeType.Consumes);
            var consumesAnalyzer =
                new ConsumesAnalyzer(controllerConsumesAttributes.ToArray());
            consumesAnalyzer.AnalyzeAttributes();
            classInformation.ConsumesTypes = consumesAnalyzer.ContentTypes;
        }
    }
}