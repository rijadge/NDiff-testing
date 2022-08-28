using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using NDiff.Models;

namespace NDiff.Services.Analyzers.AttributeAnalyzers.Attributes.Routes
{
    public class RouteAnalyzer : Attribute
    {
        internal List<CustomRoute> CustomRoutes { get; set; } = new List<CustomRoute>();

        public RouteAnalyzer(AttributeData[] attributeData) : base(attributeData)
        {
        }

        protected override void Analyze()
        {
            foreach (var attributeData in AttributeData)
            {
                if (!IsValidAttribute(attributeData))
                    continue;
                try
                {
                    CustomRoutes.Add(AnalyzeRoute(attributeData));
                }
                catch (Exception exception)
                {
                    // this route is ignored
                }
            }
        }

        /// <summary>
        /// Analyzes a single Route.
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        private static CustomRoute AnalyzeRoute(AttributeData route)
        {
            var newRoute = new CustomRoute(route.ConstructorArguments.FirstOrDefault().Value?.ToString());

            foreach (var (key, value) in route.NamedArguments)
            {
                switch (key)
                {
                    case "Name":
                        newRoute.OperationId = value.Value?.ToString();
                        break;
                    case "Order":
                        newRoute.Order = value.Value?.ToString();
                        break;
                }
            }

            return newRoute;
        }

        /// <summary>
        /// Checks if the attribute data is valid. The default comparer is valid for Route attributes.
        /// </summary>
        /// <param name="attributeData">The attribute.</param>
        /// <returns>True if the number of constructor arguments is 1 and the number of named arguments is 0, 1, or 2. Otherwise, false.</returns>
        protected virtual bool IsValidAttribute(AttributeData attributeData)
        {
            return attributeData.ConstructorArguments.Length is 1 && attributeData.NamedArguments.Length is 0 or 1 or 2;
        }

        protected override bool IsValid() => true;
    }
}