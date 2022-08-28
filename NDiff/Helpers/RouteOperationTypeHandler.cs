using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;
using NDiff.Enums;
using NDiff.ExtensionMethods;
using NDiff.Models;
using NDiff.Services.Analyzers.AttributeAnalyzers.Attributes.Routes;

namespace NDiff.Helpers
{
    /// <summary>
    /// Includes methods to analyze the <see cref="OperationType"/> and <see cref="CustomRoute"/> dictionary.
    /// <see cref="Dictionary{TKey,TValue}"/> where the key is <see cref="OperationType"/> and the value is the <see cref="List{T}"/> of <see cref="CustomRoute"/>.
    /// </summary>
    public static class RouteOperationTypeHandler
    {
        /// <summary>
        /// Add the default operation types for Route attributes.
        /// </summary>
        /// <param name="customRoutes">Route attributes that will be matched with default <see cref="OperationType"/>.</param>
        /// <param name="defaultOperationsRoutes">Dictionary of <see cref="OperationType"/> and the <see cref="List{T}"/> of <see cref="CustomRoute"/>.</param>
        /// <remarks>The <see cref="defaultOperationsRoutes"/> will contain the result.</remarks>
        public static void AddDefaultOperationTypes(List<CustomRoute> customRoutes,
            IDictionary<OperationType, List<CustomRoute>> defaultOperationsRoutes)
        {
            var defaultOperationTypes =
                Enum.GetNames<OperationType>().Where(type => type != OperationType.Head.ToString()).ToList();

            AddRoutesToAllHttpOperations(defaultOperationTypes, customRoutes, defaultOperationsRoutes);
        }

        /// <summary>
        /// Adds to the specified <see cref="operationRoutesDictionary"/> all the <see cref="customRoutes"/> and <see cref="httpOperationsStrings"/>.
        /// </summary>
        /// <param name="httpOperationsStrings">All http operations that will be matched with routes.</param>
        /// <param name="operationRoutesDictionary">Dictionary of <see cref="OperationType"/> and the <see cref="List{T}"/> of <see cref="CustomRoute"/>.</param>
        /// <param name="customRoutes"></param>
        /// <remarks>The <see cref="operationRoutesDictionary"/> will contain the result.</remarks>
        public static void AddRoutesToAllHttpOperations(List<string> httpOperationsStrings,
            List<CustomRoute> customRoutes,
            IDictionary<OperationType, List<CustomRoute>> operationRoutesDictionary)
        {
            foreach (var operation in httpOperationsStrings)
            {
                AddRoutesToHttpOperation(operation, customRoutes, operationRoutesDictionary);
            }
        }

        /// <summary>
        /// Adds single <see cref="operation"/> with all <see cref="customRoutes"/> to the operationsCustomRoutes.
        /// </summary>
        /// <param name="operationRoutesDictionary"></param>
        /// <param name="customRoutes">Custom routes to add.</param>
        /// <param name="operation"></param>
        /// <remarks>The <see cref="operationRoutesDictionary"/> will contain the result.</remarks>
        private static void AddRoutesToHttpOperation(string operation, List<CustomRoute> customRoutes,
            IDictionary<OperationType, List<CustomRoute>> operationRoutesDictionary)
        {
            if (!Enum.TryParse(operation, true, out OperationType operationType)) return;

            if (operationRoutesDictionary.ContainsKey(operationType))
                operationRoutesDictionary[operationType].AddRange(new List<CustomRoute>(customRoutes));
            else
                operationRoutesDictionary.Add(operationType, new List<CustomRoute>(customRoutes));
        }

        /// <summary>
        /// It analyzes Http attributes of <see cref="AttributeType"/> that have a template (aka a constructor arguments).
        /// </summary>
        /// <param name="methodSymbol"></param>
        /// <param name="types"></param>
        /// <param name="methodInformation"></param>
        public static void AnalyzeAttributesOfType(IMethodSymbol methodSymbol, IEnumerable<AttributeType> types,
            ref MethodInformation methodInformation)
        {
            foreach (var type in types)
            {
                var methodOperationAttributes = methodSymbol.GetAttributesOfType(type).ToArray();

                var routeAnalyzer = new RouteAnalyzer(methodOperationAttributes);
                routeAnalyzer.AnalyzeAttributes();
                var customRoutes = routeAnalyzer.CustomRoutes;

                if (customRoutes.Count <= 0) continue;

                AddRoutesToHttpOperation(type.ToString(), customRoutes, methodInformation.MethodHttpRoutes);
            }
        }

        /// <summary>
        /// Matches the Route attribute with all HttpXXX() that have an empty template.
        /// Cases:
        /// By Default if there is no HttpGet/POST/... (that do not have Templates) then the Route is considered an HttpGet.
        /// If there is an HttpGet/Route... that do not have Templates, but have Names then a cross product happens and two endpoints are created the default one and the one with ./RouteTemplate.
        /// If the same case as above but there is no Name, then the Default "/" route is not created but only the cross product with Routes.
        /// </summary>
        /// <param name="methodRoutes"></param>
        /// <param name="methodInformation"></param>
        public static void MatchMethodRouteAttributeTemplateWithOperationsWithoutTemplate(
            List<CustomRoute> methodRoutes, ref MethodInformation methodInformation)
        {
            // if there are no attributes without a template, then we have to add the default HTTPXXX operation for all Route attributes.
            var areRouteAttributesMatched = false;

            // httpMethod
            foreach (var (_, methodHttpRoute) in methodInformation.MethodHttpRoutes)
            {
                var toBeDeleted = new List<CustomRoute>();
                var newRoutesToAdd = new List<CustomRoute>();

                // httpRoute
                methodHttpRoute.ForEach(httpRoute =>
                {
                    if (httpRoute.IsInitialized) return;

                    areRouteAttributesMatched = true;

                    // routes (attributes)
                    // if there are no routes, we leave the httpRoute and it will then match with Route that 
                    // comes from Controller.
                    methodRoutes.ForEach(route =>
                    {
                        // delete empty httpRoute, since it is matched with the Route attribute.
                        toBeDeleted.Add(httpRoute);

                        // put the route in the temporary list.
                        newRoutesToAdd.Add(route);
                    });
                });

                // add new endpoints as result of Route-HttpXXXWithoutTemplate match.
                methodHttpRoute.AddRange(newRoutesToAdd.Distinct());
                methodHttpRoute.RemoveAll(route => toBeDeleted.Contains(route));
            }

            // if some route matched, or there were no Route attributes return.
            if (areRouteAttributesMatched || methodRoutes.Count == 0) return;

            // otherwise, we have to add all HTTPXXXX operation type (EXCEPT HEAD) to all Route that are part of this method and could not match 
            // with at least an empty HTTPXXXX of this method!
            AddDefaultOperationTypes(methodRoutes, methodInformation.MethodHttpRoutes);
        }
    }
}