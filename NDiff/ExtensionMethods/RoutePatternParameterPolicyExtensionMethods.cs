using System.Collections.Generic;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.OpenApi.Models;
using NDiff.Enums;

namespace NDiff.ExtensionMethods
{
    public static class RoutePatternParameterPolicyExtensionMethods
    {
        /// <summary>
        /// Generates an <see cref="OpenApiSchema"/> from the <see cref="RoutePatternParameterPolicyReference"/> of the
        /// parameter that is part of Route. 
        /// </summary>
        /// <param name="policies"></param>
        /// <returns></returns>
        public static OpenApiSchema GenerateOpenApiSchema(
            this IReadOnlyList<RoutePatternParameterPolicyReference> policies)
        {
            var openApiSchema = new OpenApiSchema();

            if (policies == null || policies.Count == 0)
            {
                return openApiSchema;
            }

            foreach (var policy in policies)
            {
                AddConstraint(policy.Content, openApiSchema);
            }

            return openApiSchema;
        }
        
        /// <summary>
        /// Add the constraint based on its definition.
        /// According to https://swagger.io/docs/specification/data-models/data-types/#file
        /// and https://swagger.io/docs/specification/data-models/data-types/
        /// TODO: handle potential exceptions from not correctly written constraints.
        /// </summary>
        /// <param name="constraint"></param>
        /// <param name="openApiSchema"></param>
        private static void AddConstraint(string constraint, OpenApiSchema openApiSchema)
        {
            switch (constraint)
            {
                case { } when constraint.Equals(ConstraintType.Int.GetStringValue()):
                {
                    openApiSchema.Type = OpenApiSchemaType.Int.GetStringValue();
                    openApiSchema.Format = OpenApiSchemaFormat.Int32.GetStringValue();
                }
                    break;
                case { } when constraint.Equals(ConstraintType.Long.GetStringValue()):
                {
                    openApiSchema.Type = OpenApiSchemaType.Int.GetStringValue();
                    openApiSchema.Format = OpenApiSchemaFormat.Int64.GetStringValue();
                }
                    break;
                case { } when constraint.Equals(ConstraintType.Bool.GetStringValue()):
                {
                    openApiSchema.Type = OpenApiSchemaType.Bool.GetStringValue();
                    openApiSchema.Format = null;
                }
                    break;
                case { } when constraint.Equals(ConstraintType.Decimal.GetStringValue()):
                {
                    openApiSchema.Type = OpenApiSchemaType.Number.GetStringValue();
                    openApiSchema.Format = null;
                }
                    break;
                case { } when constraint.Equals(ConstraintType.Double.GetStringValue()):
                {
                    openApiSchema.Type = OpenApiSchemaType.Number.GetStringValue();
                    openApiSchema.Format = OpenApiSchemaFormat.Double.GetStringValue();
                }
                    break;
                case { } when constraint.Equals(ConstraintType.Float.GetStringValue()):
                {
                    openApiSchema.Type = OpenApiSchemaType.Number.GetStringValue();
                    openApiSchema.Format = OpenApiSchemaFormat.Float.GetStringValue();
                }
                    break;
                case { } when constraint.Equals(ConstraintType.Alpha.GetStringValue()):
                {
                    // TODO: maybe this should have the Format="alpha"? The format can be anything based on openApi spec.
                    openApiSchema.Type = OpenApiSchemaType.String.GetStringValue();
                    openApiSchema.Pattern = "^[a-zA-Z]+$";
                }
                    break;
                case { } when constraint.Equals(ConstraintType.Datetime.GetStringValue()):
                {
                    openApiSchema.Type = OpenApiSchemaType.String.GetStringValue();
                    openApiSchema.Format = OpenApiSchemaFormat.Datetime.GetStringValue();
                }
                    break;
                case { } when constraint.Equals(ConstraintType.Guid.GetStringValue()):
                {
                    openApiSchema.Type = OpenApiSchemaType.String.GetStringValue();
                    openApiSchema.Format = ConstraintType.Guid.GetStringValue();
                }
                    break;
                case { } when constraint.StartsWith(ConstraintType.Length.GetStringValue()):
                {
                    var length = ConstraintType.Length.GetStringValue();
                    var minMaxLength = constraint.Substring(length.Length, constraint.Length - length.Length - 1)
                        .Split(",");
                    switch (minMaxLength.Length)
                    {
                        case 1:
                            openApiSchema.MinLength = int.Parse(minMaxLength[0]);
                            openApiSchema.MinLength = int.Parse(minMaxLength[0]);
                            break;
                        case 2:
                            openApiSchema.MinLength = int.Parse(minMaxLength[0]);
                            openApiSchema.MinLength = int.Parse(minMaxLength[1]);
                            break;
                    }
                }
                    break;
                case { } when constraint.StartsWith(ConstraintType.MaxLength.GetStringValue()) &&
                              constraint.EndsWith(")"):
                {
                    var maxLength = ConstraintType.MaxLength.GetStringValue();
                    openApiSchema.MaxLength =
                        int.Parse(constraint.Substring(maxLength.Length, constraint.Length - maxLength.Length - 1));
                }
                    break;
                case { } when constraint.StartsWith(ConstraintType.Max.GetStringValue()) && constraint.EndsWith(")"):
                {
                    var max = ConstraintType.Max.GetStringValue();
                    openApiSchema.Maximum =
                        int.Parse(constraint.Substring(max.Length, constraint.Length - max.Length - 1));
                }
                    break;
                case { } when constraint.StartsWith(ConstraintType.MinLength.GetStringValue()) &&
                              constraint.EndsWith(")"):
                {
                    var minLength = ConstraintType.MinLength.GetStringValue();
                    openApiSchema.MinLength =
                        int.Parse(constraint.Substring(minLength.Length, constraint.Length - minLength.Length - 1));
                }
                    break;
                case { } when constraint.StartsWith(ConstraintType.Min.GetStringValue()) && constraint.EndsWith(")"):
                {
                    var min = ConstraintType.Min.GetStringValue();
                    openApiSchema.Minimum =
                        int.Parse(constraint.Substring(min.Length, constraint.Length - min.Length - 1));
                }
                    break;
                case { } when constraint.StartsWith(ConstraintType.Range.GetStringValue()) && constraint.EndsWith(")"):
                {
                    var range = ConstraintType.Range.GetStringValue();
                    var numbers = constraint.Substring(range.Length, constraint.Length - range.Length - 1).Split(",");
                    if (numbers.Length != 2)
                        break;
                    openApiSchema.Minimum = int.Parse(numbers[0]);
                    openApiSchema.Maximum = int.Parse(numbers[1]);
                }
                    break;
                case { } when constraint.StartsWith(ConstraintType.Regex.GetStringValue()) && constraint.EndsWith(")"):
                {
                    var regex = ConstraintType.Regex.GetStringValue();
                    openApiSchema.Pattern =
                        constraint.Substring(regex.Length, constraint.Length - regex.Length - 1);
                }
                    break;
            }
        }
    }
}