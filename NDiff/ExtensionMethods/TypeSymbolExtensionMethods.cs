using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using NDiff.Enums;
using NDiff.Helpers;
using NDiff.Helpers.Trackers;
using NDiff.Services.Generators;

namespace NDiff.ExtensionMethods
{
    public static class TypeSymbolExtensionMethods
    {
        /// <summary>
        /// Checks if this symbol extends <see cref="BaseControllerNamespace"/>.
        /// </summary>
        /// <param name="classSymbol"></param>
        /// <returns>True if it extends, otherwise, false.</returns>
        public static bool IsBaseTypeController(this ITypeSymbol classSymbol)
        {
            return classSymbol?.BaseType?.ToString() == BaseControllerNamespace.Controller.GetStringValue() ||
                   classSymbol?.BaseType?.ToString() == BaseControllerNamespace.ControllerBase.GetStringValue();
        }

        /// <summary>
        /// Checks if the symbol does not have a parent (the parent is the default <see cref="object"/> class).
        /// </summary>
        /// <param name="classSymbol"></param>
        /// <returns>True if there is no parent, otherwise false.</returns>
        public static bool IsBaseTypeObject(this ITypeSymbol classSymbol)
        {
            return classSymbol.BaseType.IsObject();
        }

        /// <summary>
        /// Gets the syntax tree of the base (parent) class. 
        /// </summary>
        /// <param name="classSymbol"></param>
        /// <returns>Syntax tree of the class if exists, otherwise, null.</returns>
        public static ClassDeclarationSyntax GetBaseTypeClassDeclarationSyntax(this ITypeSymbol classSymbol)
        {
            return classSymbol?.BaseType?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as
                ClassDeclarationSyntax;
        }

        /// <summary>
        /// Gets all the http methods of this class.
        /// </summary>
        /// <param name="classSymbol"></param>
        /// <returns><see cref="IEnumerable{T}"/> of <see cref="ISymbol"/>.</returns>
        public static IEnumerable<IMethodSymbol> GetHttpMethods(this ITypeSymbol classSymbol)
        {
            return classSymbol.GetMembers().Where(member =>
            {
                if (member is not IMethodSymbol symbol)
                    return false;
                // return those that are not properties.
                return symbol.AssociatedSymbol == null && !symbol.IsStatic &&
                       !symbol.IsGenericMethod &&
                       symbol.DeclaredAccessibility == Accessibility.Public &&
                       symbol.MethodKind != MethodKind.Constructor;
            }).Select(method => method as IMethodSymbol);
        }

        /// <summary>
        /// Gets the attributes of certain <see cref="AttributeType"/> from the current symbol.
        /// </summary>
        /// <param name="classSymbol"></param>
        /// <param name="attributeType">The type we want to filter attributes.</param>
        /// <param name="attributeTypes">Additional types we want to filter attributes.</param>
        /// <param name="predicate">Additional predicate to consider. If null then does not filter any <see cref="AttributeData"/>.</param>
        /// <returns><see cref="IEnumerable{T}"/> of <see cref="AttributeData"/>.</returns>
        public static IEnumerable<AttributeData> GetAttributesOfType(this ISymbol classSymbol,
            AttributeType attributeType, Func<AttributeData, bool> predicate = null,
            params AttributeType[] attributeTypes)
        {
            return classSymbol.GetAttributes()
                .Where(attribute => predicate?.Invoke(attribute) ?? true)
                .Where(attribute => attribute?.AttributeClass?.Name == attributeType.GetStringValue() ||
                                    attributeTypes.Any(inputAttributeType =>
                                        attribute?.AttributeClass?.Name == inputAttributeType.GetStringValue()));
        }

        /// <summary>
        /// Checks if this symbols is required or not.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static bool IsRequired(this ISymbol symbol)
        {
            return symbol.GetAttributesOfType(AttributeType.Required).FirstOrDefault() != null;
        }

        /// <summary>
        /// Reads the fields from <see cref="symbol"/>.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static List<IFieldSymbol> GetValidFields(this ITypeSymbol symbol)
        {
            return symbol.GetMembers().Where(element => element is IFieldSymbol).Cast<IFieldSymbol>().ToList();
        }

        /// <summary>
        /// Reads the properties/fields of this symbol.
        /// The OpenApi Specification shows only Properties of the Class and not the fields.
        /// The properties must be PUBLIC, NOT ABSTRACT, NOT STATIC.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static List<IPropertySymbol> GetValidProperties(this ITypeSymbol symbol)
        {
            if (symbol.IsStatic || symbol.DeclaredAccessibility != Accessibility.Public)
                return null;

            return symbol.GetMembers().Where(IsValidProperty)
                .Select(member => member as IPropertySymbol)
                .ToList();
        }

        /// <summary>
        /// Conditions for being a valid property of a type to be considered for analysis.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns>Valid properties.</returns>
        private static bool IsValidProperty(this ISymbol symbol)
        {
            return symbol is IPropertySymbol
                   {
                       IsStatic: false, DeclaredAccessibility: Accessibility.Public, IsAbstract: false, IsIndexer: false
                   } propertySymbol && propertySymbol.Type.TypeKind != TypeKind.Delegate &&
                   (propertySymbol.Type.IsPrimitive() || propertySymbol.Type is INamedTypeSymbol namedTypeSymbol &&
                       (namedTypeSymbol.IsReferenceType || namedTypeSymbol.IsStruct())) &&
                   !propertySymbol.GetAttributesOfType(AttributeType.JsonExtensionData).Any();
        }

        /// <summary>
        /// Finds the parameter location by considering the <b>first</b> [FROMXXXX] attributes and returning <see cref="ParameterLocation"/>.
        /// </summary>
        /// <param name="symbol">The symbol from which are read the attributes.</param>
        /// <param name="isFromBody">In case the <see cref="AttributeType"/> is <see cref="AttributeType.FromBody"/> it returns true.</param>
        /// <returns>Returns null if no proper match for <see cref="ParameterLocation"/> is found. Otherwise, returns the <see cref="ParameterLocation"/>.</returns>
        public static ParameterLocation? FindParameterLocation(this ISymbol symbol, out bool isFromBody)
        {
            isFromBody = false;
            var fromAttributes = symbol.GetAttributesOfTypeThatStartsWith("From").ToList();

            foreach (var fromAttribute in fromAttributes)
            {
                // if the first one is valid then take it otherwise take the next valid one.
                if (Enum.TryParse(fromAttribute?.AttributeClass?.Name.Replace("Attribute", ""),
                    out AttributeType fromAttributeType))
                {
                    if (fromAttributeType == AttributeType.FromBody)
                        isFromBody = true;
                    return GetParameterLocation(fromAttributeType);
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the attributes of certain <see cref="AttributeType"/> from the current symbol.
        /// </summary>
        /// <param name="classSymbol"></param>
        /// <param name="startsWith">The type we want to filter attributes.</param>
        /// <returns><see cref="IEnumerable{T}"/> of <see cref="AttributeData"/>.</returns>
        private static IEnumerable<AttributeData> GetAttributesOfTypeThatStartsWith(this ISymbol classSymbol,
            string startsWith)
        {
            return classSymbol.GetAttributes()
                .Where(attribute => attribute?.AttributeClass?.Name.StartsWith(startsWith) ?? false);
        }

        /// <summary>
        /// Checks if this <see cref="namedTypeSymbol"/> is a user defined class.
        /// </summary>
        /// <param name="namedTypeSymbol"></param>
        /// <returns>True if it is user defined; otherwise, false.</returns>
        public static bool IsCustomClassType(this INamedTypeSymbol namedTypeSymbol)
        {
            return (!namedTypeSymbol.IsGenericArray() && !namedTypeSymbol.IsGenericObject() &&
                    !namedTypeSymbol.IsNullableGeneric() &&
                    !namedTypeSymbol.IsNonGenericArray() &&
                    !namedTypeSymbol.IsNonGenericObject() && (namedTypeSymbol.IsReferenceType ||
                                                              namedTypeSymbol.IsStruct())) &&
                   !namedTypeSymbol.IsPrimitive();
        }

        /// <summary>
        /// Checks if the given type is generic INullable.
        /// </summary>
        /// <param name="namedTypeSymbol"></param>
        /// <returns></returns>
        private static bool IsNullableGeneric(this INamedTypeSymbol namedTypeSymbol)
        {
            return namedTypeSymbol.IsGenericType && namedTypeSymbol.OriginalDefinition.ToString() ==
                OriginalDefinitionType.GenericNullable.GetStringValue();
        }

        /// <summary>
        /// Checks if the given symbol is nullable.
        /// </summary>
        /// <param name="typeSymbol"></param>
        /// <returns></returns>
        public static bool IsNullable(this ITypeSymbol typeSymbol)
        {
            return !typeSymbol.IsBuiltInValueType();
        }

        /// <summary>
        /// Checks if this <see cref="namedTypeSymbol"/> is a struct.
        /// </summary>
        /// <param name="namedTypeSymbol"></param>
        /// <returns>True if it is user defined; otherwise, false.</returns>
        private static bool IsStruct(this INamedTypeSymbol namedTypeSymbol)
        {
            return namedTypeSymbol.TypeKind == TypeKind.Struct && !namedTypeSymbol.IsPrimitive();
        }

        /// <summary>
        /// This generates the <see cref="OpenApiSchema"/> of a particular <see cref="typeSymbol"/>.
        /// It takes into consideration all the types.
        /// </summary>
        /// <param name="typeSymbol"></param>
        /// <param name="isFromComponents">Set to true if the open api schema is created from Components.</param>
        /// <param name="componentGenerator">Used to add types that are not analyzed and were referenced somehow from a class
        /// (for example if a class has a complex type property that was not analyzed, then it should be added to the list for next iteration).</param>
        /// <returns></returns>
        public static OpenApiSchema CreateOpenApiSchema(this ITypeSymbol typeSymbol, bool isFromComponents = false,
            IComponentGenerator componentGenerator = null)
        {
            var openApiSchema = new OpenApiSchema();

            // handles arrays []int...
            if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
            {
                openApiSchema.Type = OpenApiSchemaType.Array.GetStringValue();
                openApiSchema.Items =
                    arrayTypeSymbol.ElementType.CreateOpenApiSchema(isFromComponents, componentGenerator);

                return openApiSchema;
            }

            var namedTypeSymbol = (INamedTypeSymbol) typeSymbol;

            if (namedTypeSymbol.IsNullableGeneric())
            {
                openApiSchema = namedTypeSymbol.TypeArguments.First()
                    .CreateOpenApiSchema(isFromComponents, componentGenerator);
                openApiSchema.Nullable = true;
            }
            else if (namedTypeSymbol.IsGenericArray())
            {
                openApiSchema.Type = OpenApiSchemaType.Array.GetStringValue();
                openApiSchema.Items =
                    namedTypeSymbol.TypeArguments[0].CreateOpenApiSchema(isFromComponents, componentGenerator);

                if (namedTypeSymbol.OriginalDefinition.ToString() ==
                    OriginalDefinitionType.GenericHashSet.GetStringValue())
                    openApiSchema.UniqueItems = true;
            }
            else if (namedTypeSymbol.IsGenericObject())
            {
                openApiSchema.Type = OpenApiSchemaType.Object.GetStringValue();
                openApiSchema.AdditionalProperties = new OpenApiSchema()
                {
                    Properties = new Dictionary<string, OpenApiSchema>()
                    {
                        ["key"] = namedTypeSymbol.TypeArguments[0]
                            .CreateOpenApiSchema(isFromComponents, componentGenerator),
                        ["value"] = namedTypeSymbol.TypeArguments[1]
                            .CreateOpenApiSchema(isFromComponents, componentGenerator),
                    }
                };
            }
            // those that are not generic but are part of the collections (with objects type)
            else if (namedTypeSymbol.IsNonGenericArray())
            {
                openApiSchema.Type = OpenApiSchemaType.Array.GetStringValue();
                // empty object
                openApiSchema.Items = new OpenApiSchema();
            }
            else if (namedTypeSymbol.IsNonGenericObject())
            {
                openApiSchema.Type = OpenApiSchemaType.Object.GetStringValue();
                // empty
                openApiSchema.AdditionalProperties = new OpenApiSchema();
            }
            // user defined class
            else if (namedTypeSymbol.IsCustomClassType() ||
                     namedTypeSymbol.TypeKind == TypeKind.Enum)
            {
                if (!isFromComponents)
                {
                    ReferenceTracker.AddIfNotExists(namedTypeSymbol);
                }
                else if (!componentGenerator.AnalyzedTypesContains(namedTypeSymbol) &&
                         namedTypeSymbol.MustAnalyzeType())
                {
                    componentGenerator.AddToNextAnalyzedTypes(namedTypeSymbol);
                }

                openApiSchema = new OpenApiSchema
                {
                    Reference = new OpenApiReference
                    {
                        Id = namedTypeSymbol.GetTypeDefinitionName(),
                        Type = ReferenceType.Schema
                    }
                };
            }
            else
            {
                // primitive type schema
                openApiSchema = new OpenApiSchema()
                {
                    Type = OpenApiSchemaPropertyMapper.TypeMapper[namedTypeSymbol.SpecialType].GetStringValue(),
                    Format = OpenApiSchemaPropertyMapper.FormatMapper[namedTypeSymbol.SpecialType].GetStringValue()
                };
            }

            return openApiSchema;
        }

        /// <summary>
        /// Mapping <see cref="AttributeType"/> to <see cref="ParameterLocation"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static ParameterLocation? GetParameterLocation(AttributeType? type) => type switch
        {
            AttributeType.FromQuery => ParameterLocation.Query,
            AttributeType.FromHeader => ParameterLocation.Header,
            AttributeType.FromRoute => ParameterLocation.Path,
            AttributeType.FromForm => ParameterLocation.Query,
            _ => null
        };

        /// <summary>
        /// Checks if the <see cref="namedTypeSymbol"/> is a generic array specified in <see cref="OriginalDefinitionType"/>.
        /// GenericList, GenericHashSet, GenericQueue, GenericStack, GenericLinkedList, GenericSortedSet, GenericSortedList, or LinkedList.
        /// </summary>
        /// <param name="namedTypeSymbol"></param>
        /// <returns></returns>
        private static bool IsGenericArray(this INamedTypeSymbol namedTypeSymbol)
        {
            return namedTypeSymbol.IsGenericType && (namedTypeSymbol.OriginalDefinition.ToString() ==
                                                     OriginalDefinitionType.GenericList.GetStringValue() ||
                                                     namedTypeSymbol.OriginalDefinition.ToString() ==
                                                     OriginalDefinitionType.GenericHashSet.GetStringValue() ||
                                                     namedTypeSymbol.OriginalDefinition.ToString() ==
                                                     OriginalDefinitionType.GenericQueue.GetStringValue() ||
                                                     namedTypeSymbol.OriginalDefinition.ToString() ==
                                                     OriginalDefinitionType.GenericStack.GetStringValue() ||
                                                     namedTypeSymbol.OriginalDefinition.ToString() ==
                                                     OriginalDefinitionType.GenericLinkedList.GetStringValue() ||
                                                     namedTypeSymbol.OriginalDefinition.ToString() ==
                                                     OriginalDefinitionType.GenericSortedSet.GetStringValue() ||
                                                     namedTypeSymbol.OriginalDefinition.ToString() ==
                                                     OriginalDefinitionType.GenericIList.GetStringValue() ||
                                                     namedTypeSymbol.OriginalDefinition.ToString() ==
                                                     OriginalDefinitionType.GenericIEnumerable.GetStringValue() ||
                                                     namedTypeSymbol.OriginalDefinition.ToString() ==
                                                     OriginalDefinitionType.GenericEnumerable.GetStringValue());
        }

        /// <summary>
        /// Checks if the <see cref="namedTypeSymbol"/> is a non-generic array specified in <see cref="OriginalDefinitionType"/>.
        /// ArrayList, LinkedList, BitArray, SortedList, Stack, or Queue.
        /// </summary>
        /// <param name="namedTypeSymbol"></param>
        /// <returns></returns>
        private static bool IsNonGenericArray(this INamedTypeSymbol namedTypeSymbol)
        {
            return !namedTypeSymbol.IsGenericType && (namedTypeSymbol.OriginalDefinition.ToString() ==
                                                      OriginalDefinitionType.ArrayList.GetStringValue() ||
                                                      namedTypeSymbol.OriginalDefinition.ToString() ==
                                                      OriginalDefinitionType.BitArray.GetStringValue() ||
                                                      namedTypeSymbol.OriginalDefinition.ToString() ==
                                                      OriginalDefinitionType.SortedList.GetStringValue() ||
                                                      namedTypeSymbol.OriginalDefinition.ToString() ==
                                                      OriginalDefinitionType.Stack.GetStringValue() ||
                                                      namedTypeSymbol.OriginalDefinition.ToString() ==
                                                      OriginalDefinitionType.Queue.GetStringValue() ||
                                                      namedTypeSymbol.OriginalDefinition.ToString() ==
                                                      OriginalDefinitionType.LinkedList.GetStringValue() ||
                                                      namedTypeSymbol.OriginalDefinition.ToString() ==
                                                      OriginalDefinitionType.GenericObservableCollection
                                                          .GetStringValue());
        }

        /// <summary>
        /// Checks if the <see cref="namedTypeSymbol"/> is a generic GenericDictionary, or GenericSortedDictionary.
        /// </summary>
        /// <param name="namedTypeSymbol"></param>
        /// <returns></returns>
        private static bool IsGenericObject(this INamedTypeSymbol namedTypeSymbol)
        {
            return namedTypeSymbol.IsGenericType && (namedTypeSymbol.OriginalDefinition.ToString() ==
                                                     OriginalDefinitionType.GenericDictionary.GetStringValue() ||
                                                     namedTypeSymbol.OriginalDefinition.ToString() ==
                                                     OriginalDefinitionType.GenericSortedList.GetStringValue() ||
                                                     namedTypeSymbol.OriginalDefinition.ToString() ==
                                                     OriginalDefinitionType.GenericKeyValuePair.GetStringValue() ||
                                                     namedTypeSymbol.OriginalDefinition.ToString() ==
                                                     OriginalDefinitionType.GenericIDictionary.GetStringValue() ||
                                                     namedTypeSymbol.OriginalDefinition.ToString() ==
                                                     OriginalDefinitionType.GenericSortedDictionary.GetStringValue());
        }

        /// <summary>
        /// Checks if the given <see cref="namedTypeSymbol"/> is non-generic Hashtable.
        /// According to https://swagger.io/docs/specification/data-models/dictionaries/ HashTables will create an a schema of type object
        /// with no additionalProperties.
        /// </summary>
        /// <param name="namedTypeSymbol"></param>
        /// <returns></returns>
        private static bool IsNonGenericObject(this INamedTypeSymbol namedTypeSymbol)
        {
            return namedTypeSymbol.OriginalDefinition.ToString() ==
                   OriginalDefinitionType.Hashtable.GetStringValue() ||
                   namedTypeSymbol.OriginalDefinition.ToString() ==
                   OriginalDefinitionType.KeyValuePair.GetStringValue();
        }

        /// <summary>
        /// Checks if the type is of primitive type by considering <see cref="SpecialType"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsPrimitive(this ITypeSymbol type) =>
            type.IsBuiltInReferenceType() || type.IsBuiltInValueType();


        /// <summary>
        /// Checks if the symbol is built in value type.
        /// </summary>
        /// <param name="typeSymbol"></param>
        /// <returns></returns>
        private static bool IsBuiltInValueType(this ITypeSymbol typeSymbol) =>
            typeSymbol.SpecialType switch
            {
                SpecialType.System_Boolean => true,
                SpecialType.System_SByte => true,
                SpecialType.System_Int16 => true,
                SpecialType.System_Int32 => true,
                SpecialType.System_Int64 => true,
                SpecialType.System_Byte => true,
                SpecialType.System_UInt16 => true,
                SpecialType.System_UInt32 => true,
                SpecialType.System_UInt64 => true,
                SpecialType.System_Single => true,
                SpecialType.System_Double => true,
                SpecialType.System_Char => true,
                SpecialType.System_DateTime => true,
                _ => false
            };

        /// <summary>
        /// Checks if the symbol is a built in reference type.
        /// </summary>
        /// <param name="typeSymbol"></param>
        /// <returns></returns>
        private static bool IsBuiltInReferenceType(this ITypeSymbol typeSymbol) => typeSymbol.SpecialType switch
        {
            SpecialType.System_String => true,
            SpecialType.System_Object => true,
            _ => false
        };


        /// <summary>
        /// Checks if the given type is an Object.
        /// </summary>
        /// <param name="typeSymbol">Type to check.</param>
        /// <returns>True if it is an object; otherwise, false.</returns>
        private static bool IsObject(this ITypeSymbol typeSymbol)
        {
            return typeSymbol?.Name == ClassType.Object.GetStringValue();
        }

        /// <summary>
        /// Checks if this type must be analyzed and is not null and is not object and is a custom class type (user defined class). 
        /// </summary>
        /// <param name="typeSymbol">The symbol to check.</param>
        /// <returns>True if it should be analyzed (so it is not a ValueType object of enum/struct, or a default Object); otherwise, false.</returns>
        public static bool MustAnalyzeType(this ITypeSymbol typeSymbol)
        {
            return typeSymbol != null && !typeSymbol.IsObject() && typeSymbol.TypeKind is not TypeKind.Delegate &&
                   typeSymbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsCustomClassType();
        }

        /// <summary>
        /// Returns the definition name. For generics it returns the name of generic plus the types.
        /// </summary>
        /// <example>
        /// For generics it will be similar to: Controllers.GenericModel<T1, T2, T3>stringintint.
        /// </example>
        /// <param name="typeSymbol"></param>
        /// <returns>The name of that type.</returns>
        public static string GetTypeDefinitionName(this ITypeSymbol typeSymbol)
        {
            if (typeSymbol is not INamedTypeSymbol {IsGenericType: true} namedType)
                return typeSymbol.OriginalDefinition.ToString();

            return typeSymbol.OriginalDefinition + namedType.TypeArguments
                .Select(argument => argument.GetTypeDefinitionName())
                .Aggregate("", (current, type) => current + type);
        }
    }
}