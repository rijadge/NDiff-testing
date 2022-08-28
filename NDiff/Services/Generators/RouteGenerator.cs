using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.OpenApi.Models;
using NDiff.Enums;
using NDiff.ExtensionMethods;
using NDiff.Helpers;
using NDiff.Models;

namespace NDiff.Services.Generators
{
    public static class RouteGenerator
    {
        /// <summary>
        /// Generates the <see cref="OpenApiPathItem"/> for all the necessary routes.
        /// </summary>
        /// <param name="classInformation"></param>
        /// <param name="methodInformation"></param>
        /// <param name="apiOperation"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static OpenApiPaths GenerateRoutes(ClassInformation classInformation,
            MethodInformation methodInformation, OpenApiOperation apiOperation, OpenApiPaths paths, OpenApiComponents components)
        {
            var customRoutesList = new List<CustomRoute>(classInformation.ControllerRoutes);
            var defaultOperationsRoutes = new Dictionary<OperationType, List<CustomRoute>>();
            // if the methodRoutes of the method is empty, then, it will be created 
            // a default endpoint for all custom route with all operationTypes.
            // However, if there is a valid endpoint then this will not be taken into account.
            if (methodInformation.MethodHttpRoutes.Count == 0)
            {
                RouteOperationTypeHandler.AddDefaultOperationTypes(customRoutesList, defaultOperationsRoutes);
            }

            foreach (var (operationType, methodCustomRoutes) in methodInformation.MethodHttpRoutes.Concat(
                defaultOperationsRoutes))
            {
                customRoutesList.Clear();
                customRoutesList.AddRange(methodCustomRoutes);

                customRoutesList.ForEach(methodCustomRoute =>
                {
                    if (methodCustomRoute.RoutePattern.RawText.StartsWith("/"))
                    {
                        var (openApiParameters, finalRoute) =
                            GenerateRoutePathAndApiParameters(classInformation,
                                methodInformation,
                                methodCustomRoute.RoutePattern.PathSegments.ToList(), components);

                        var newOpenApiOperation = new OpenApiOperation
                        {
                            Tags = apiOperation.Tags,
                            Responses = apiOperation.Responses,
                            RequestBody = apiOperation.RequestBody,
                            Parameters = openApiParameters
                        };

                        GeneratePathItem(newOpenApiOperation, paths, finalRoute, operationType);
                    }
                    else
                    {
                        foreach (var controllerCustomRoute in classInformation.ControllerRoutes)
                        {
                            var pathSegments = MergePathSegments(controllerCustomRoute, methodCustomRoute);
                            var (openApiParameters, finalRoute) =
                                GenerateRoutePathAndApiParameters(classInformation, methodInformation, pathSegments, components);

                            var newOpenApiOperation = new OpenApiOperation
                            {
                                Tags = apiOperation.Tags,
                                Responses = apiOperation.Responses,
                                RequestBody = apiOperation.RequestBody,
                                Parameters = openApiParameters
                            };

                            GeneratePathItem(newOpenApiOperation, paths, finalRoute, operationType);
                        }
                    }
                });
            }

            return paths;
        }

        /// <summary>
        /// Generates a single path item and adds it to the <see cref="paths"/>.
        /// In case the <see cref="finalRoute"/> exists in <see cref="paths"/> then, to that route is added
        /// another operation.
        /// </summary>
        /// <param name="apiOperation">The current <see cref="OpenApiOperation"/>.</param>
        /// <param name="paths">It will be updated with the new <see cref="OpenApiPathItem"/>.</param>
        /// <param name="finalRoute">The final route to add the <see cref="OperationType"/> and the <see cref="OpenApiOperation"/>.</param>
        /// <param name="operationType">The <see cref="OperationType"/> of the <see cref="finalRoute"/>.</param>
        private static void GeneratePathItem(OpenApiOperation apiOperation, OpenApiPaths paths, string finalRoute,
            OperationType operationType)
        {
            OpenApiPathItem pathItem = new();
            if (paths.ContainsKey(finalRoute))
            {
                paths[finalRoute].AddOperation(operationType, apiOperation);
                return;
            }

            pathItem.AddOperation(operationType, apiOperation);

            paths.Add(finalRoute, pathItem);
        }

        /// <summary>
        /// Generates the final route path from path-segments and creates the <see cref="OpenApiParameter"/> based on analysis.
        /// </summary>
        /// <param name="classInformation">Contains the information of class being analyzed.</param>
        /// <param name="methodInformation">Contains the information of method being analyzed.</param>
        /// <param name="pathSegments">Path segments that need to be analyzed to generate the final route/endpoint.</param>
        private static (List<OpenApiParameter>, string) GenerateRoutePathAndApiParameters(
            ClassInformation classInformation, MethodInformation methodInformation,
            List<RoutePatternPathSegment> pathSegments, OpenApiComponents components)
        {
            List<OpenApiParameter> openApiParameters = new();
            var finalRoute = "";

            finalRoute = pathSegments.Aggregate(finalRoute,
                (current, pathSegment) => AnalyzePathSegment(classInformation, methodInformation, pathSegment, current,
                    openApiParameters));
            var parameterLocation = 1;

            // add those parameters that were not added in AnalyzePathSegment
            foreach (var groupedParam in methodInformation.OpenApiParameters.Values)
            {
                groupedParam.ForEach(parameter =>
                {
                    // skip FromBody
                    if (methodInformation.FromBodyParameterLocation != parameterLocation)
                    {
                        // skip parameters that already exists (and have same name and IN parameter location)
                        // there might be two parameters with same name and different location!
                        // and skip those parameters that are implicitly FromBody
                        if (!parameter.IsParameterImplicitFromBody(components) &&
                            !openApiParameters.Exists(oldParam =>
                                oldParam.Name == parameter.Name &&
                                (oldParam.In == parameter.In || parameter.In == null)))
                        {
                            // create a new one to not update the original value of ParameterLocation.
                            var newParameter = new OpenApiParameter();
                            if (parameter.In == null)
                            {
                                newParameter.In = ParameterLocation.Query;
                            }

                            newParameter.ReplacePropertiesIfDefaultWith(parameter);
                            openApiParameters.Add(newParameter);
                        }
                    }
                });

                parameterLocation++;
            }

            return (openApiParameters, finalRoute);
        }

        /// <summary>
        /// Merges two <see cref="RoutePatternPathSegment"/>.
        /// </summary>
        /// <param name="controllerCustomRoute"></param>
        /// <param name="methodCustomRoute"></param>
        /// <returns></returns>
        private static List<RoutePatternPathSegment> MergePathSegments(CustomRoute controllerCustomRoute,
            CustomRoute methodCustomRoute)
        {
            var pathSegments = controllerCustomRoute.RoutePattern.PathSegments.ToList();
            pathSegments.AddRange(methodCustomRoute.RoutePattern.PathSegments);

            return pathSegments;
        }

        /// <summary>
        /// It analyzes a path segment and generates the route and parameters for that segment path.
        /// </summary>
        /// <param name="classInformation">Contains the information of class being analyzed.</param>
        /// <param name="pathSegment">The <see cref="RoutePatternPathSegment"/> being analyzed.</param>
        /// <param name="finalRoute">The route that will be finalized for this segment.</param>
        /// <param name="methodInformation">Contains the information of method being analyzed.</param>
        /// <param name="openApiParameters">A <see cref="List{T}"/> of <see cref="OpenApiParameter"/> parameters that are part of the specific Route.
        /// An updated <see cref="List{T}"/> may be returned.</param>
        /// <returns>The final route path for this <see cref="pathSegment"/>.</returns>
        private static string AnalyzePathSegment(ClassInformation classInformation, MethodInformation methodInformation,
            RoutePatternPathSegment pathSegment, string finalRoute, IList<OpenApiParameter> openApiParameters)
        {
            finalRoute += "/";
            var allParameters = methodInformation.OpenApiParameters
                .Where(keyValue => !methodInformation.HasParameterLocation(keyValue.Key))
                .SelectMany(el => el.Value).ToList();

            foreach (var pathSegmentPart in pathSegment.Parts)
            {
                if (pathSegmentPart.IsLiteral)
                {
                    var literalPathSegment = (RoutePatternLiteralPart) pathSegmentPart;
                    finalRoute += AnalyzeLiteralPathSegment(classInformation, literalPathSegment, methodInformation);
                }
                else if (pathSegmentPart.IsParameter)
                {
                    var parameterPathSegment = (RoutePatternParameterPart) pathSegmentPart;
                    finalRoute += $"{{{parameterPathSegment.Name}}}";

                    var routeParameterOpenApiSchema =
                        parameterPathSegment.ParameterPolicies.GenerateOpenApiSchema();

                    var openApiRouteParameter = new OpenApiParameter()
                    {
                        In = ParameterLocation.Path,
                        Required = !parameterPathSegment.IsOptional,
                        Name = parameterPathSegment.Name
                    };

                    var parameterPartOfRoute = allParameters
                        .SingleOrDefault(parameter => parameter.Name == parameterPathSegment.Name);

                    if (parameterPartOfRoute is {In: null or ParameterLocation.Path})
                    {
                        openApiRouteParameter.Required |= parameterPartOfRoute.Required;
                        routeParameterOpenApiSchema.ReplacePropertiesIfDefaultWith(parameterPartOfRoute.Schema);
                        openApiRouteParameter.Schema = routeParameterOpenApiSchema;
                        // it will be added as part of route TODO: refactor this!
                        // take into consideration the different types of the same parameter name!
                        openApiParameters.Remove(parameterPartOfRoute);
                    }
                    else
                    {
                        // set type to string in case there is a route parameter without any type policies.
                        routeParameterOpenApiSchema.Type ??= OpenApiSchemaType.String.GetStringValue();
                        openApiRouteParameter.Schema = routeParameterOpenApiSchema;
                    }

                    openApiParameters.Add(openApiRouteParameter);
                }
            }

            return finalRoute;
        }

        /// <summary>
        /// Analyzes a literal by replacing the [controller], [area]... with information from <paramref name="classInformation"/>.
        /// </summary>
        /// <param name="classInformation">Information of current class that is being analyzed.</param>
        /// <param name="literalPathSegment">The literal path segment that may have different tags.</param>
        /// <param name="method">The current method that is being analyzed.</param>
        /// <returns></returns>
        private static string AnalyzeLiteralPathSegment(ClassInformation classInformation,
            RoutePatternLiteralPart literalPathSegment,
            MethodInformation method)
        {
            var updateSegment =
                literalPathSegment.Content.Replace("[controller]",
                    classInformation.Tag);

            updateSegment = updateSegment.Replace("[area]",
                string.IsNullOrEmpty(method.AreaAttribute) ? classInformation.AreaAttribute : method.AreaAttribute);
            // TODO: Consider the [apiVersion] literal as well.

            return updateSegment;
        }
    }
}