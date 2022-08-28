using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;
using NDiff.Helpers;
using NDiff.Models;

namespace NDiff.Services.Analyzers.AttributeAnalyzers.Attributes.Routes
{
    public class AcceptAnalyzer : Attribute
    {
        internal Dictionary<OperationType, List<CustomRoute>> OperationRoutesDictionary { get; set; } =
            new Dictionary<OperationType, List<CustomRoute>>();

        public AcceptAnalyzer(AttributeData[] attributeData) : base(attributeData)
        {
        }

        protected override void Analyze()
        {
            foreach (var acceptRoute in AttributeData)
            {
                if (!IsValidAttribute(acceptRoute))
                    continue;

                AnalyzeAcceptAttribute(acceptRoute);
            }
        }

        /// <summary>
        /// Handles the creation of <see cref="CustomRoute"/> with <see cref="OperationType"/>.
        /// </summary>
        /// <param name="acceptRoute">The accept verb in a method.</param>
        private void AnalyzeAcceptAttribute(AttributeData acceptRoute)
        {
            var routes = new List<CustomRoute>();
            var httpOperations = acceptRoute.ConstructorArguments[0];
            var namedArguments = acceptRoute.NamedArguments;

            CustomRoute customRoute = null;
            string name = null, order = null;
            List<string> httpOperationsStrings = new();

            switch (httpOperations.Kind)
            {
                case TypedConstantKind.Primitive:
                    httpOperationsStrings.Add(httpOperations.Value?.ToString());
                    break;
                case TypedConstantKind.Array:
                    httpOperationsStrings.AddRange(httpOperations.Values
                        .Select(val => val.Value.ToString()));
                    break;
            }

            foreach (var (key, value) in namedArguments)
            {
                switch (key)
                {
                    case "Name":
                        name = value.Value?.ToString();
                        break;
                    case "Order":
                        order = value.Value?.ToString();
                        break;
                    case "Route":
                        customRoute = new CustomRoute(value.Value?.ToString() ?? "");
                        break;
                }
            }
            
            // if true, it is empty AcceptAttribute
            if (name is null && order is null && customRoute is null && httpOperationsStrings.Count == 0)
                return;

            customRoute ??= new CustomRoute(null);
            customRoute.Order = order;
            customRoute.OperationId = name;

            routes.Add(customRoute);

            RouteOperationTypeHandler.AddRoutesToAllHttpOperations(httpOperationsStrings, routes,
                OperationRoutesDictionary);

            if (httpOperationsStrings.Count == 0 && customRoute.IsInitialized)
            {
                RouteOperationTypeHandler.AddDefaultOperationTypes(routes, OperationRoutesDictionary);
            }
        }

        /// <summary>
        /// Checks if the AcceptVerbsAttribute is defined properly.
        /// </summary>
        /// <param name="attributeData">The accept route attribute.</param>
        /// <returns>True if it is valid; otherwise, false.</returns>
        protected virtual bool IsValidAttribute(AttributeData attributeData)
        {
            return attributeData.ConstructorArguments[0].Kind != TypedConstantKind.Array ||
                   attributeData.ConstructorArguments[0].Values.Length != 0 || attributeData.NamedArguments.Length != 0;
        }

        protected override bool IsValid() => true;
    }
}