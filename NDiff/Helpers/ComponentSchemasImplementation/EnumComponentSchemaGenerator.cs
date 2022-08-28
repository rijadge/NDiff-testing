using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using NDiff.ExtensionMethods;
using NDiff.Services.Generators;

namespace NDiff.Helpers.ComponentSchemasImplementation
{
    public class EnumComponentSchemaGenerator : SchemaGenerator
    {
        public EnumComponentSchemaGenerator(ITypeSymbol typeSymbol, IComponentGenerator componentGenerator) : base(typeSymbol,
            componentGenerator)
        {
        }

        public override OpenApiSchema GenerateSchema()
        {
            var enumFields = TypeSymbol.GetValidFields()
                .Select(field => new OpenApiInteger(int.Parse(field.ConstantValue.ToString())))
                .Cast<IOpenApiAny>()
                .ToList();

            var namedType = (INamedTypeSymbol) TypeSymbol;
            var underlyingSchema = namedType.EnumUnderlyingType.CreateOpenApiSchema();

            var enumSchema = new OpenApiSchema
            {
                Type = underlyingSchema.Type,
                Format = underlyingSchema.Format,
                Enum = enumFields
            };

            return enumSchema;
        }
    }
}