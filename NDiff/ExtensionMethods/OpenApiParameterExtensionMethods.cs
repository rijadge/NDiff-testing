using Microsoft.OpenApi.Models;

namespace NDiff.ExtensionMethods
{
    public static class OpenApiParameterExtensionMethods
    {
        /// <summary>
        /// Checks if the parameter is implicit from body; the In is null and the Schema is a complex type.
        /// </summary>
        /// <param name="parameter">The parameter to analyze.</param>
        /// <param name="components">Already analyzed components.</param>
        /// <returns>If parameter is implicit from body returns true; otherwise, false.</returns>
        public static bool IsParameterImplicitFromBody(this OpenApiParameter parameter,
            OpenApiComponents components = null)
        {
            var isNotEnum = true;

            if (components != null && parameter?.Schema?.Reference?.Id != null)
            {
                isNotEnum = components.Schemas[parameter.Schema.Reference.Id].Enum.Count == 0;
            }

            return isNotEnum && parameter.In is null && parameter.Schema.IsComplexType();
        }

        /// <summary>
        /// It replaces the properties of <see cref="source"/> with <see cref="other"/> when <see cref="source"/> properties are default.
        /// The changes are saved on <see cref="source"/>. 
        /// Default definition: null or false.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="other"></param>
        public static void ReplacePropertiesIfDefaultWith(this OpenApiParameter source, OpenApiParameter other)
        {
            source.Content ??= other.Content;
            source.Deprecated = source.Deprecated == false && other.Deprecated;
            source.Description ??= other.Description;
            source.Example ??= other.Example;
            source.Examples ??= other.Examples;
            source.Explode = source.Explode == false && other.Explode;
            source.Extensions ??= other.Extensions;
            source.In ??= other.In;
            source.Name ??= other.Name;
            source.Reference ??= other.Reference;
            source.Required = source.Required == false && other.Required;
            source.Schema ??= other.Schema;
            source.Style ??= other.Style;
            source.AllowReserved = source.AllowReserved == false && other.AllowReserved;
            source.UnresolvedReference = source.UnresolvedReference == false && other.UnresolvedReference;
            source.AllowEmptyValue = source.AllowEmptyValue == false && other.AllowEmptyValue;
        }
    }
}