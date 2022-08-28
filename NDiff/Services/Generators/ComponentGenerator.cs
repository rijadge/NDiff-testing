using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;
using NDiff.ExtensionMethods;
using NDiff.Helpers;
using NDiff.Helpers.Trackers;

namespace NDiff.Services.Generators
{
    public class ComponentGenerator : IComponentGenerator
    {
        /// <summary>
        /// Contains the current types that have to be analyzed. Initially will contain <see cref="ReferencedTypes"/>.
        /// Then, will take the members of <see cref="NextTypesToAnalyze"/>.
        /// </summary>
        private HashSet<ITypeSymbol> CurrentTypesToAnalyze { get; set; }

        /// <summary>
        /// Contains the types that were referenced inside the type in <see cref="ReferenceTracker.ReferencedTypes"/> but were not
        /// explicitly referenced in method parameters or class/method attributes.
        /// </summary>
        /// <example>
        /// Class A has been used in a method attribute. However, that class extends from another class B and will
        /// refer to it using <see cref="OpenApiSchema.AllOf"/> property. This class will not be part of initial
        /// <see cref="ReferenceTracker.ReferencedTypes"/> therefore we need to add it to the <see cref="NextTypesToAnalyze"/> for next iteration.
        /// </example>
        private HashSet<ITypeSymbol> NextTypesToAnalyze { get; set; } = new HashSet<ITypeSymbol>();

        /// <summary>
        /// Contains all the schemas for all classes.
        /// </summary>
        private readonly Dictionary<string, OpenApiSchema> _classesSchema = new Dictionary<string, OpenApiSchema>();

        /// <summary>
        /// Generates the components that are part of the solution.
        /// </summary>
        public OpenApiComponents GenerateComponents()
        {
            var components = new OpenApiComponents();

            CurrentTypesToAnalyze = new HashSet<ITypeSymbol>(ReferenceTracker.ReferencedTypes);

            while (CurrentTypesToAnalyze.Any())
            {
                GenerateSchemas(_classesSchema);
                CurrentTypesToAnalyze = new HashSet<ITypeSymbol>(NextTypesToAnalyze);
                NextTypesToAnalyze.Clear();
            }

            components.Schemas = _classesSchema;

            return components;
        }

        /// <summary>
        /// Generates schemas for a single <see cref="CurrentTypesToAnalyze"/> iteration.
        /// </summary>
        /// <param name="classesSchema"></param>
        private void GenerateSchemas(IDictionary<string, OpenApiSchema> classesSchema)
        {
            foreach (var symbolComponent in CurrentTypesToAnalyze)
            {
                var schemaGenerator = SchemaGeneratorFactory.GetSchemaGenerator(symbolComponent, this);

                var schema = schemaGenerator.GenerateSchema();
                
                var componentName = symbolComponent.GetTypeDefinitionName();

                if (!classesSchema.ContainsKey(componentName))
                    classesSchema.Add(componentName, schema);
            }
        }

        public bool AnalyzedTypesContains(ITypeSymbol symbol)
        {
            return _classesSchema.ContainsKey(symbol.GetTypeDefinitionName());
        }

        public void AddToNextAnalyzedTypes(ITypeSymbol symbol)
        {
            NextTypesToAnalyze.Add(symbol);
        }
    }
}