using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using NDiff.ExtensionMethods;
using NDiff.Models;
using NDiff.Services.Analyzers.AttributeAnalyzers.Attributes;

namespace NDiff.Services.Generators
{
    public class OpenApiGenerator
    {
        private readonly IComponentGenerator _componentGenerator;

        public OpenApiGenerator(IComponentGenerator componentGenerator)
        {
            _componentGenerator = componentGenerator;
        }

        public ClassInformation ClassInformation { get; set; }

        /// <summary>
        /// Analyzes the base class.
        /// </summary>
        /// <param name="classInformation"></param>
        /// <returns>The analyzed class information <see cref="ClassInformation"/>.</returns>
        public static ClassInformation AnalyzeBaseClass(ClassInformation classInformation)
        {
            // analyze the class attributes.
            ClassAttributesAnalyzer.AnalyzeClassAttributes(classInformation);

            var methods = classInformation.ClassSymbol.GetHttpMethods();

            foreach (var method in methods)
            {
                if (method.IsOverride || method.IsVirtual || method.IsAbstract)
                {
                    classInformation.OverriddenMethods.Add(method,
                        MethodAttributeAnalyzer.AnalyzeMethodAttributes(method));
                }
                else
                {
                    classInformation.Methods.Add(MethodAttributeAnalyzer.AnalyzeMethodAttributes(method));
                }
            }

            return classInformation;
        }

        public void CreateDocument(OpenApiPaths responses, string dir)
        {
            var document = new OpenApiDocument
            {
                Info = new OpenApiInfo
                {
                    Version = "1.0.0",
                    Title = "Swagger Example (Simple)",
                },
                Servers = new List<OpenApiServer>
                {
                    new OpenApiServer {Url = "https://localhost:5001"}
                },
                Paths = responses,
                Components = _componentGenerator.GenerateComponents()
                /*new OpenApiPaths
            {
                ["/pets"] = new OpenApiPathItem
                {
                    Operations = new Dictionary<OperationType, OpenApiOperation>
                    {
                        [OperationType.Get] = new OpenApiOperation
                        {
                            Description = "Returns all examples from the system that the user has access to",
                            Parameters = 
                            Responses = responses,
                            RequestBody = new OpenApiRequestBody()
                            {
                            },
                            
                        }
                    },
                    Parameters = new List<OpenApiParameter>
                    {
                        new OpenApiParameter()
                        {
                        }
                    }
                }
            }*/
            };


            var outputString = document.Serialize(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json);
            
            var fileName = $"CODE_METRICS{new Random().Next(19999)}.md";
            var fullPath = Path.Combine("./", fileName);
            File.WriteAllTextAsync(
               fullPath,
               outputString).Wait();

            Console.WriteLine("RESULT::::::::::::::::::::::::::::::::::::::" + outputString);

            File.WriteAllText(dir+"/test.json", outputString);
        }

        /// <summary>
        /// Analyzes the current class based on the data of parent class. 
        /// </summary>
        /// <param name="child">Current class to be analyzed.</param>
        /// <param name="parent">The parent class to refer to.</param>
        /// <returns>The updated current analyzed class.</returns>
        public static ClassInformation AnalyzeChildParentClasses(ClassInformation child, ClassInformation parent)
        {
            ClassAttributesAnalyzer.AnalyzeClassAttributes(child);
            var methods = child.ClassSymbol.GetHttpMethods();

            // If area attribute is not present in child, then take it from parent class.
            if (string.IsNullOrEmpty(child.AreaAttribute))
                child.AreaAttribute = parent.AreaAttribute;

            // if there are no Routes in the child controller then inherit from parent.
            if (child.ControllerRoutes == null || child.ControllerRoutes.Count == 0)
                child.ControllerRoutes = parent.ControllerRoutes;

            // replace consumes types if they do not exist in child!
            if (child.ConsumesTypes == null || !child.ConsumesTypes.Any())
                child.ConsumesTypes = parent.ConsumesTypes;

            // replace content types if they do not exist in child!
            if (child.ContentTypes == null || !child.ContentTypes.Any())
                child.ContentTypes = parent.ContentTypes;

            // replace ErrorResponse types if they do not exist in child!
            child.ErrorResponseType ??= parent.ErrorResponseType;

            // replace produces if they do not exist in child!
            child.Produces ??= parent.Produces;

            // the parent class overrides the child class ProducesResponseType with the <b>SAME STATUS CODE!!!!</b>
            // BUT does not override the child.Methods ProduceResponseType.
            // the order of the parameters here is IMPORTANT.
            child.ResponseTypesWithoutContent =
                MergeDictionaries(child.ResponseTypesWithoutContent, parent.ResponseTypesWithoutContent);


            foreach (var method in methods)
            {
                if (method.IsOverride || method.IsVirtual || method.IsAbstract)
                {
                    var childAnalyzedMethod = MethodAttributeAnalyzer.AnalyzeMethodAttributes(method);

                    // if this method overrides the parent, then update necessary fields.
                    if (method.IsOverride && method.OverriddenMethod != null)
                    {
                        var parentMethod = parent.OverriddenMethods[method.OverriddenMethod];

                        if (string.IsNullOrEmpty(childAnalyzedMethod.AreaAttribute))
                            childAnalyzedMethod.AreaAttribute = parentMethod.AreaAttribute;

                        // if there are no Routes/HttpXXX in the child controller then inherit from parent.
                        if (childAnalyzedMethod.MethodHttpRoutes == null ||
                            childAnalyzedMethod.MethodHttpRoutes.Count == 0)
                            childAnalyzedMethod.MethodHttpRoutes = parentMethod.MethodHttpRoutes;

                        // inherit from parent ConsumeTypes if they do not exist.
                        if (childAnalyzedMethod.ConsumesTypes == null || !childAnalyzedMethod.ConsumesTypes.Any())
                            childAnalyzedMethod.ConsumesTypes = parentMethod.ConsumesTypes;

                        // inherit from parent the ContentTypes if they do not exist.
                        if (childAnalyzedMethod.ContentTypes == null || !childAnalyzedMethod.ContentTypes.Any())
                            childAnalyzedMethod.ContentTypes = parentMethod.ContentTypes;

                        childAnalyzedMethod.ErrorResponseType ??= parentMethod.ErrorResponseType;

                        // default response type
                        if (childAnalyzedMethod.ProducesDefaultResponseType == null &&
                            !childAnalyzedMethod.ContainsDefaultResponseType)
                        {
                            childAnalyzedMethod.ProducesDefaultResponseType =
                                parentMethod.ProducesDefaultResponseType;
                            childAnalyzedMethod.ContainsDefaultResponseType =
                                parentMethod.ContainsDefaultResponseType;
                        }

                        childAnalyzedMethod.Produces ??= parentMethod.Produces;

                        childAnalyzedMethod.ResponseTypesWithoutContent = MergeDictionaries(
                            childAnalyzedMethod.ResponseTypesWithoutContent,
                            parentMethod.ResponseTypesWithoutContent);


                        if (childAnalyzedMethod.OpenApiParameters.Keys.Count !=
                            parentMethod.OpenApiParameters.Keys.Count)
                        {
                            throw new ArgumentException(
                                $"The number of parameters in the base method {parentMethod.MethodName} and " +
                                $"override method {childAnalyzedMethod.MethodName} are not the same!");
                        }

                        // number of parameter locations must be the same for parent and child method.
                        foreach (var (parameterLocation, childOpenApiParameters) in childAnalyzedMethod
                            .OpenApiParameters)
                        {
                            if (childAnalyzedMethod.FromBodyParameterLocation == parameterLocation)
                                continue;

                            // maybe it is enough to take only the first parameter.
                            // if the attribute type of the parameter is [FromBody] and the child has a null AttributeType
                            // then we will have to remove all definitions of childOpenApiParameters at that location.
                            var firstParameterFromILocationInChild = childAnalyzedMethod
                                .OpenApiParameters[parameterLocation]
                                .FirstOrDefault();
                            var firstParameterFromILocationInParent = parentMethod
                                .OpenApiParameters[parameterLocation]
                                .FirstOrDefault();

                            // check if there are two from body attributes created
                            if ((parentMethod.HasParameterLocation(parameterLocation) ||
                                 firstParameterFromILocationInChild.IsParameterImplicitFromBody()) &&
                                childAnalyzedMethod.IsFromBodySet)
                            {
                                throw new ArgumentException(
                                    $"The number of implicit or explicit FromBody attributes is not 1 in method {childAnalyzedMethod.MethodName}");
                            }

                            // if child does not have [FROM...], add the from body from parent.
                            if (parentMethod.FromBodyParameterLocation == parameterLocation &&
                                firstParameterFromILocationInChild.In == null)
                            {
                                childAnalyzedMethod.FromBodyParameterLocation = parentMethod.FromBodyParameterLocation;
                            }

                            // in case the parent has Query parameter location and the child parameter location is null
                            // and is reference, we need to take the de-constructed params from parent to the child.
                            if (ShouldChildGetParentParameters(firstParameterFromILocationInChild,
                                firstParameterFromILocationInParent))
                            {
                                GetParentAnalyzedComplexTypeParameters(childOpenApiParameters, parentMethod,
                                    parameterLocation);
                                continue;
                            }

                            for (var i = 0; i < childOpenApiParameters.Count; i++)
                            {
                                childOpenApiParameters.ElementAt(i).Required |= parentMethod
                                    .OpenApiParameters[parameterLocation].ElementAt(i).Required;

                                // this is valid since all (properties) will have the same IN-Parameter Location!
                                if (firstParameterFromILocationInChild.In == null &&
                                    firstParameterFromILocationInParent.In != null)
                                {
                                    childOpenApiParameters.ElementAt(i).In ??=
                                        firstParameterFromILocationInParent.In;
                                }
                            }
                        }
                    }

                    child.OverriddenMethods.Add(method, childAnalyzedMethod);
                }
                else
                {
                    child.Methods.Add(MethodAttributeAnalyzer.AnalyzeMethodAttributes(method));
                }
            }

            return child;
        }

        /// <summary>
        /// It clears the <see cref="childOpenApiParameters"/> and adds all the parameters of the parent method at this <see cref="ParameterLocation"/>.
        /// </summary>
        /// <param name="childOpenApiParameters"></param>
        /// <param name="parentMethod"></param>
        /// <param name="parameterLocation"></param>
        private static void GetParentAnalyzedComplexTypeParameters(List<OpenApiParameter> childOpenApiParameters,
            MethodInformation parentMethod,
            int parameterLocation)
        {
            childOpenApiParameters.Clear();
            parentMethod.OpenApiParameters[parameterLocation]
                .ForEach(childOpenApiParameters.Add);
        }

        /// <summary>
        /// Checks if the child is with no parameter location, is a reference type and the parent has an In Query <see cref="ParameterLocation"/>.
        /// </summary>
        /// <param name="firstParameterFromILocationInChild">First parameter from the i-th location. One location might have multiple parameters
        /// in case the initial parameter was of complex type and was considered as FromQuery (it was de-constructed).</param>
        /// <param name="firstParameterFromILocationInParent">First parameter from the i-th location of parent method.</param>
        /// <returns></returns>
        private static bool ShouldChildGetParentParameters(OpenApiParameter firstParameterFromILocationInChild,
            OpenApiParameter firstParameterFromILocationInParent)
        {
            return firstParameterFromILocationInChild.Schema.Reference != null &&
                   firstParameterFromILocationInChild.In == null &&
                   firstParameterFromILocationInParent.In != null;
        }

        public void GenerateOpenApi(OpenApiPaths paths)
        {
            //var paths = new OpenApiPaths();

            var components = _componentGenerator.GenerateComponents();

            foreach (var method in ClassInformation.Methods.Concat(ClassInformation.OverriddenMethods.Values))
            {
                // read contents.
                var contents = ClassInformation.ContentTypes.Concat(method.ContentTypes).Distinct().ToList();

                // read consumes that will have to be matched with RequestBody params.
                var consumes = ClassInformation.ConsumesTypes.Concat(method.ConsumesTypes).Distinct().ToList();

                // read status codes and media types.
                var mediaTypesForStatusCodes = MergeDictionaries(ClassInformation.ResponseTypesWithoutContent,
                    method.ResponseTypesWithoutContent);

                OpenApiRequestBody requestBody = null;

                // read produces
                var producesOpenApiMediaType = GetProduces(ClassInformation.Produces, method.Produces);

                // read produces error
                var producesErrorOpenApiMediaType =
                    GetProduces(ClassInformation.ErrorResponseType, method.ErrorResponseType);

                // CHECK THE 102 from other project, WHY it does not have the Media/type in Swagger 
                // but does have in our app!!!!
                var openApiResponses = ResponseGenerator.GenerateOpenApiResponses(mediaTypesForStatusCodes, contents,
                    producesOpenApiMediaType, producesErrorOpenApiMediaType, method.ContainsDefaultResponseType,
                    method.ProducesDefaultResponseType);

                // is there explicit from body param.
                if (method.IsFromBodySet)
                {
                    requestBody = new OpenApiRequestBody
                    {
                        Content = ContentGenerator.GenerateContent(
                            ContentGenerator.GenerateOpenApiMedia(method
                                .OpenApiParameters[method.FromBodyParameterLocation].First().Schema), consumes)
                    };
                }
                else
                {
                    // check for complex types that do not have a parameter location set.
                    var requestBodyParameter = method.OpenApiParameters
                        .Where(parameters => parameters.Value.Count == 1)
                        .SelectMany(el => el.Value)
                        .FirstOrDefault(parameter => parameter.IsParameterImplicitFromBody(components));

                    if (requestBodyParameter != null)
                    {
                        requestBody = new OpenApiRequestBody
                        {
                            Content = ContentGenerator.GenerateContent(
                                ContentGenerator.GenerateOpenApiMedia(requestBodyParameter.Schema), consumes)
                        };
                    }
                }

                var apiOperation = GenerateInitialOpenApiOperation(openApiResponses, requestBody);

                RouteGenerator.GenerateRoutes(ClassInformation, method, apiOperation, paths, components);
            }

            
        }

        /// <summary>
        /// Generates a new <see cref="OpenApiOperation"/> with a <see cref="List{T}"/> of <see cref="OpenApiResponse"/>
        /// and the <see cref="OpenApiRequestBody"/>.
        /// </summary>
        /// <param name="openApiResponses"></param>
        /// <param name="requestBody"></param>
        /// <returns></returns>
        private OpenApiOperation GenerateInitialOpenApiOperation(OpenApiResponses openApiResponses,
            OpenApiRequestBody requestBody)
        {
            return new OpenApiOperation
            {
                Tags = new List<OpenApiTag>()
                {
                    new()
                    {
                        Name = ClassInformation.Tag
                    }
                },
                Responses = openApiResponses,
                RequestBody = requestBody
            };
        }

        /// <summary>
        /// It merges two dictionaries and in case the key exists on first then it replaces it.
        /// </summary>
        /// <param name="first">The first dictionary.</param>
        /// <param name="second">The second dictionary.</param>
        /// <returns>The merged dictionary where in case two same keys exists it takes from the <see cref="second"/> <see cref="Dictionary{TKey,TValue}"/>.</returns>
        private static Dictionary<int, OpenApiMediaType> MergeDictionaries(Dictionary<int, OpenApiMediaType> first,
            Dictionary<int, OpenApiMediaType> second)
        {
            var mergedDictionary = new Dictionary<int, OpenApiMediaType>();
            foreach (var (key, value) in first.Concat(second))
            {
                mergedDictionary[key] = value;
            }

            return mergedDictionary;
        }

        /// <summary>
        /// Finds the best match for Produces attribute.
        /// </summary>
        /// <param name="classProduces"></param>
        /// <param name="methodProduces"></param>
        /// <returns>Returns the <see cref="methodProduces"/> if set; otherwise the <see cref="classProduces"/> if set; otherwise null.</returns>
        private static OpenApiMediaType GetProduces(OpenApiMediaType classProduces, OpenApiMediaType methodProduces)
        {
            return methodProduces is {Schema: { }} ? methodProduces : classProduces;
        }
    }
}
