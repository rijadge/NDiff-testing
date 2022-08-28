using System.Linq;
using Microsoft.CodeAnalysis;
using NDiff.Enums;
using NDiff.ExtensionMethods;
using NDiff.Helpers;
using NDiff.Models;
using NDiff.Services.Analyzers.AttributeAnalyzers.Attributes.ContentTypes;
using NDiff.Services.Analyzers.AttributeAnalyzers.Attributes.ResponseTypes;
using NDiff.Services.Analyzers.AttributeAnalyzers.Attributes.Routes;
using NDiff.Services.Generators;

namespace NDiff.Services.Analyzers.AttributeAnalyzers.Attributes
{
    public class MethodAttributeAnalyzer
    {
        public static MethodInformation AnalyzeMethodAttributes(IMethodSymbol methodSymbol)
        {
            MethodInformation methodInformation = new()
            {
                ReturnType = methodSymbol.ReturnType,
                MethodName = methodSymbol.Name
            };

            // analyze controller Area attribute, this is then given to the Route
            var controllerAreaAttributes = methodSymbol.GetAttributesOfType(AttributeType.Area);
            var areaAnalyzer = new AreaAnalyzer(controllerAreaAttributes.ToArray());
            areaAnalyzer.AnalyzeAttributes();
            methodInformation.AreaAttribute = areaAnalyzer.AreaValue;

            // get the arguments as well as attributes
            // analyze them, one method cannot have more than one [FromBody] attribute// we will have to analyze all one by one.
            var methodArguments = methodSymbol.Parameters.ToList();
            var (openApiParameters, fromBodyParameterLocation) = methodArguments.GenerateOpenApiParameters();
            methodInformation.OpenApiParameters = openApiParameters;
            methodInformation.FromBodyParameterLocation = fromBodyParameterLocation;
            //ComponentGenerator.GenerateComponents(referencedClasses);

            // consumes attribute analysis.
            var methodConsumes = methodSymbol.GetAttributesOfType(AttributeType.Consumes);
            var consumesAnalyzer =
                new ConsumesAnalyzer(methodConsumes.ToArray());
            consumesAnalyzer.AnalyzeAttributes();
            methodInformation.ConsumesTypes = consumesAnalyzer.ContentTypes;

            // produces attribute analysis.
            var methodProduces = methodSymbol.GetAttributesOfType(AttributeType.Produces);
            var producesAnalyzer =
                new ProducesAnalyzer(methodProduces.ToArray());
            producesAnalyzer.AnalyzeAttributes();
            methodInformation.ContentTypes = producesAnalyzer.ContentTypes;
            methodInformation.Produces = producesAnalyzer.ProducesOpenApiMedia;

            // analyze method ProduceErrorResponseType
            var methodErrorResponseTypeAttributes =
                methodSymbol.GetAttributesOfType(AttributeType.ProducesErrorResponseType);
            var producesError = new CustomResponseTypeAnalyzer(methodErrorResponseTypeAttributes.ToArray());
            producesError.AnalyzeAttributes();
            methodInformation.ErrorResponseType = producesError.OpenApiMedia;

            // analyze method ProduceDefaultResponseType
            var methodDefaultResponseTypeAttributes =
                methodSymbol.GetAttributesOfType(AttributeType.ProducesDefaultResponseType).ToList();
            var producesDefault = new CustomResponseTypeAnalyzer(methodDefaultResponseTypeAttributes.ToArray());
            producesDefault.AnalyzeAttributes();
            methodInformation.ProducesDefaultResponseType = producesDefault.OpenApiMedia;
            methodInformation.ContainsDefaultResponseType = methodDefaultResponseTypeAttributes.Count == 1;

            // analyze method ProduceResponseType attribute
            var methodProducesResponseTypesAttributes =
                methodSymbol.GetAttributesOfType(AttributeType.ProducesResponseType).ToList();
            var producesResponseType =
                new ProducesResponseTypeAnalyzer(
                    methodProducesResponseTypesAttributes.ToArray());
            producesResponseType.AnalyzeAttributes();
            methodInformation.ResponseTypesWithoutContent = producesResponseType.MediatTypesForStatusCodes;


            // analyze methods and potentially create a model. 
            var methodRoutes = methodSymbol.GetAttributesOfType(AttributeType.Route).ToList();
            var cRoutes = new RouteAnalyzer(methodRoutes.ToArray());
            cRoutes.AnalyzeAttributes();
            var analyzedMethodRoutes = cRoutes.CustomRoutes;

            // return the Route from accept verbs and return the HTTPXXX methods.
            var acceptVerbs = methodSymbol.GetAttributesOfType(AttributeType.AcceptVerbs).ToList();
            var acceptAnalyzer = new AcceptAnalyzer(acceptVerbs.ToArray());
            acceptAnalyzer.AnalyzeAttributes();
            methodInformation.MethodHttpRoutes = acceptAnalyzer.OperationRoutesDictionary;

            RouteOperationTypeHandler.AnalyzeAttributesOfType(methodSymbol, GetDefaultOperationTypes(),
                ref methodInformation);

            RouteOperationTypeHandler.MatchMethodRouteAttributeTemplateWithOperationsWithoutTemplate(
                analyzedMethodRoutes,
                ref methodInformation);

            return methodInformation;
        }

        private static AttributeType[] GetDefaultOperationTypes()
        {
            return new[]
            {
                AttributeType.Put,
                AttributeType.Get,
                AttributeType.Post,
                AttributeType.Patch,
                AttributeType.Head,
                AttributeType.Options,
                AttributeType.Delete
            };
        }
    }
}