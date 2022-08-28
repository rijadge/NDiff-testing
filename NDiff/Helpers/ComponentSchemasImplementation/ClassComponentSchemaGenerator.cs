using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;
using NDiff.Enums;
using NDiff.ExtensionMethods;
using NDiff.Services.Generators;

namespace NDiff.Helpers.ComponentSchemasImplementation
{
    /// <summary>
    /// Used to create the schema for types.
    /// </summary>
    public class ClassComponentSchemaGenerator : SchemaGenerator
    {
        public ClassComponentSchemaGenerator(ITypeSymbol typeSymbol, IComponentGenerator componentGenerator) : base(
            typeSymbol,
            componentGenerator)
        {
        }

        public override OpenApiSchema GenerateSchema()
        {
            var properties = TypeSymbol.GetValidProperties();

            var classSchema = new OpenApiSchema
            {
                Type = OpenApiSchemaType.Object.GetStringValue(),
                Required = properties?.Where(prop => prop.IsRequired()).Select(el => el.Name.ToString()).ToHashSet(),
                AdditionalProperties = null,
                Properties = GeneratePropertiesSchema(properties)
            };

            if (TypeSymbol.BaseType.MustAnalyzeType())
            {
                if (!ComponentGenerator.AnalyzedTypesContains(TypeSymbol.BaseType))
                {
                    ComponentGenerator.AddToNextAnalyzedTypes(TypeSymbol.BaseType);
                }

                classSchema.AllOf = new List<OpenApiSchema>()
                {
                    TypeSymbol.BaseType.CreateOpenApiSchema(true, ComponentGenerator)
                };
            }

            return classSchema;
        }

        /// <summary>
        /// Generates the schema for all properties that are provided.
        /// </summary>
        /// <param name="properties">Properties to be analyzed.</param>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> where the key is the name of property and the value is the schema of property.</returns>
        private Dictionary<string, OpenApiSchema> GeneratePropertiesSchema(List<IPropertySymbol> properties)
        {
            var propertiesSchema = new Dictionary<string, OpenApiSchema>();

            properties?.ForEach(property =>
            {
                var propertySchema = property.Type.CreateOpenApiSchema(true, ComponentGenerator);
                propertySchema.ReadOnly = property.IsReadOnly;
                propertySchema.Nullable = property.Type.IsNullable();
                propertiesSchema.Add(property.Name, propertySchema);
            });

            return propertiesSchema;
        }
    }
}