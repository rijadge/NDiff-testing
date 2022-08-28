using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;
using NDiff.ExtensionMethods;

namespace NDiff.Services.Generators
{
    public static class ParametersGenerator
    {
        /// <summary>
        /// Generates all the parameters for the <see cref="symbols"/>.
        /// </summary>
        /// <param name="symbols">All parameter symbols of a method.</param>
        /// <returns>The <see cref="List{T}"/> of all <see cref="OpenApiParameter"/>.</returns>
        public static (Dictionary<int, List<OpenApiParameter>>, int) GenerateOpenApiParameters(
            this List<IParameterSymbol> symbols)
        {
            var fromBodyParameterPosition = -1;

            var openApiParametersDict = new Dictionary<int, List<OpenApiParameter>>();

            var parameterPosition = 1;

            symbols.ForEach(symbol =>
            {
                var openApiParameters = new List<OpenApiParameter>();
                var isExplicitFromBody = symbol.GenerateOpenApiParameter(openApiParameters, "", true, null);
                if (isExplicitFromBody)
                {
                    fromBodyParameterPosition = parameterPosition;
                }

                openApiParametersDict.Add(parameterPosition, openApiParameters);
                parameterPosition++;
            });

            return (openApiParametersDict, fromBodyParameterPosition);
        }

        /// <summary>
        /// Generates the OpenApiParameter for the particular <see cref="symbol"/>.
        /// TODO: FIRST REFACTOR
        /// TODO: How to handle the Required Attributes? If the Parent is required should everything be required?!
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="openApiParameters"></param>
        /// <param name="currentParameterAccessName">The name the is used to access parameters on complex types.</param>
        /// <param name="isDirectParentClass">Set to true if this is a direct parent class. Used in IS A relationship to check for Required attributes and used in HAS A relationship.</param>
        /// <param name="currentInParameterLocation">Contains the <see cref="ParameterLocation"/>. In case there is a HAS A relationship we need to keep track of <see cref="ParameterLocation"/>
        /// by sending <see cref="currentInParameterLocation"/> to the properties of that complex type (custom created classes).</param>
        /// <returns></returns>
        private static bool GenerateOpenApiParameter(this ISymbol symbol,
            List<OpenApiParameter> openApiParameters, string currentParameterAccessName, bool isDirectParentClass,
            ParameterLocation? currentInParameterLocation)
        {
            // TODO: what if parameter is a class and it is REQUIRED? Should all its properties be REQUIRED as well?
            // because in that case ONLY the instance is necessary to be created and not necessarily all the properties to be set!
            // initial open api parameter
            var openApiParameter = new OpenApiParameter
            {
                Required = isDirectParentClass && symbol.IsRequired(), // || (IsParentRequired?),
                Name = string.IsNullOrEmpty(currentParameterAccessName)
                    ? symbol.Name
                    : currentParameterAccessName + "." + symbol.Name,
                In = currentInParameterLocation
            };

            // if the symbol is part of parameter read the Parameter Location (FROM ATTRIBUTES)!
            // the Required is already read in initial openApiParameter!
            if (symbol is IParameterSymbol {Type: INamedTypeSymbol parameterNamedTypeSymbol} parameterSymbol)
            {
                openApiParameter.In = symbol.FindParameterLocation(out var isFromBody);
                // consider as from body
                if ((openApiParameter.In == null && parameterNamedTypeSymbol.IsCustomClassType()) || isFromBody)
                {
                    openApiParameter.Schema = parameterSymbol.Type.CreateOpenApiSchema();
                    openApiParameters.Add(openApiParameter);
                    return isFromBody;
                }

                if (parameterNamedTypeSymbol.IsCustomClassType())
                {
                    var firstParentClass = true;
                    var currentType = parameterNamedTypeSymbol;
                    // de-construct properties of this class first.
                    var classProperties = parameterNamedTypeSymbol.GetValidProperties();

                    // analyze the de-constructed properties.
                    classProperties.GenerateOpenApiParameterFromProperties(currentParameterAccessName,
                        openApiParameters, true, openApiParameter.In);

                    // analyzing parent class (IS A) properties as well. This will run for all parents up to the one that inherits from Object.
                    while (!currentType.IsBaseTypeObject() &&
                           currentType.GetBaseTypeClassDeclarationSyntax() is not null && currentType != null)
                    {
                        currentType.BaseType.GetValidProperties()
                            .GenerateOpenApiParameterFromProperties(currentParameterAccessName, openApiParameters,
                                firstParentClass, openApiParameter.In);

                        firstParentClass = false;
                        currentType = currentType.BaseType;
                    }

                    return false;
                }

                // if the parameter is a custom class - means it should be de-constructed
                openApiParameter.Schema =
                    parameterSymbol.Type.CreateOpenApiSchema();
            }
            else if (symbol is IPropertySymbol {Type: INamedTypeSymbol propertyNamedTypeSymbol} propertySymbol)
            {
                // if it is a property, we are actually inside the class and we analyze that property!
                if (propertyNamedTypeSymbol.IsCustomClassType())
                {
                    propertyNamedTypeSymbol.GetValidProperties().GenerateOpenApiParameterFromProperties(
                        currentParameterAccessName,
                        openApiParameters, isDirectParentClass, openApiParameter.In);

                    return false;
                }

                openApiParameter.Schema =
                    propertySymbol.Type.CreateOpenApiSchema();
            }
            // for IArrayTypeSymbol we need to create a schema.
            else if (symbol is IPropertySymbol otherPropertySymbol)
            {
                openApiParameter.Schema = otherPropertySymbol.Type.CreateOpenApiSchema();
            }
            else if (symbol is IParameterSymbol otherParameterSymbol)
            {
                openApiParameter.In = symbol.FindParameterLocation(out var isFromBody);
                openApiParameter.Schema = otherParameterSymbol.Type.CreateOpenApiSchema();
                openApiParameters.Add(openApiParameter);

                return isFromBody;
            }

            openApiParameters.Add(openApiParameter);
            return false;
        }

        /// <summary>
        /// Generates <see cref="OpenApiParameter"/> from the properties of a class.
        /// </summary>
        /// <param name="classProperties">The class properties/fields.</param>
        /// <param name="currentParameterAccessName">Contains the access name that is derived from parameter.
        /// If this is a direct child of a complex parameter (pure class, not lists or similar) the value will be empty ("").</param>
        /// <param name="openApiParameters">The list of <see cref="OpenApiParameter"/> that the method contains.</param>
        /// <param name="isImmediatePropertyOfClass">Is true when these <see cref="classProperties"/> are immediate
        /// properties of the class.</param>
        /// <param name="currentParameterLocation">The current parameter location (all properties will inherit it!).
        /// </param>
        private static void GenerateOpenApiParameterFromProperties(this IList<IPropertySymbol> classProperties,
            string currentParameterAccessName, List<OpenApiParameter> openApiParameters,
            bool isImmediatePropertyOfClass,
            ParameterLocation? currentParameterLocation)
        {
            if (classProperties == null)
                return;

            foreach (var property in classProperties)
            {
                var propertyName = property.Name;
                var newParam = "";
                // if the property is a class then we simply change its name to access it.
                if (property.Type is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsCustomClassType())
                {
                    if (string.IsNullOrEmpty(currentParameterAccessName))
                        newParam = propertyName;
                    else
                        newParam = currentParameterAccessName + "." + propertyName;
                }
                else
                {
                    newParam = currentParameterAccessName;
                }

                property.GenerateOpenApiParameter(openApiParameters, newParam, isImmediatePropertyOfClass,
                    currentParameterLocation);
            }
        }
    }
}