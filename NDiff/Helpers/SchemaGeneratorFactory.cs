using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;
using NDiff.Helpers.ComponentSchemasImplementation;
using NDiff.Services.Generators;

namespace NDiff.Helpers
{
    public static class SchemaGeneratorFactory
    {
        public static SchemaGenerator GetSchemaGenerator(ITypeSymbol symbol, IComponentGenerator componentGenerator)
        {
            return symbol switch
            {
                // Actually this one is only for generating components. Class, and Enums
                // for normal classes
                {TypeKind: TypeKind.Enum} => new EnumComponentSchemaGenerator(symbol, componentGenerator),
                _ => new ClassComponentSchemaGenerator(symbol, componentGenerator)
            };
        }
    }

    public abstract class SchemaGenerator
    {
        /// <summary>
        /// Symbol to analyze.
        /// </summary>
        protected readonly ITypeSymbol TypeSymbol;
        /// <summary>
        /// The component generator in <see cref="ComponentSchemasImplementation"/>.
        /// </summary>
        protected readonly IComponentGenerator ComponentGenerator;

        protected SchemaGenerator(ITypeSymbol typeSymbol, IComponentGenerator componentGenerator)
        {
            TypeSymbol = typeSymbol;
            ComponentGenerator = componentGenerator;
        }

        /// <summary>
        /// Generates a single <see cref="OpenApiSchema"/> for a particular symbol <see cref="TypeSymbol"/>.
        /// </summary>
        /// <returns></returns>
        public abstract OpenApiSchema GenerateSchema();
    }
}