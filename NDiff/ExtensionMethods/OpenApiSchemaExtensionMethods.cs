using Microsoft.OpenApi.Models;
using NDiff.Enums;
using NDiff.Services.Generators;

namespace NDiff.ExtensionMethods
{
    public static class OpenApiSchemaExtensionMethods
    {
        /// <summary>
        /// Checks if the <paramref name="source"/> and <paramref name="other"/> <see cref="OpenApiSchema"/> are the same.
        /// Two parameters, one in Route and the other in Method should have the same TYPES in order to have a valid endpoint!
        /// If they do not have the same types then what will happen is that the route will never match because there will be
        /// two different filters. E.g. one for being string in Route("qwe:alpha"), and one in method Get(int qwe), that do not go together.
        /// This is currently not used, but it was initially used in <see cref="RouteGenerator.AnalyzePathSegment"/>.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="other"></param>
        /// <returns>True, if they are compatible; otherwise, false.</returns>
        public static bool IsTypeCompatibleWith(this OpenApiSchema source, OpenApiSchema other)
        {
            return (source.Type.Equals(other.Type) && source.Format.Equals(other.Format)) ||
                   (string.IsNullOrEmpty(other.Format) && string.IsNullOrEmpty(other.Format));
        }

        /// <summary>
        /// Checks if the <see cref="OpenApiSchema"/> is of <see cref="OpenApiSchema.Type"/> <see cref="OpenApiSchemaType.Array"/>
        /// or <see cref="OpenApiSchemaType.Object"/> or has a reference.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsComplexType(this OpenApiSchema source)
        {
            return source.Type == OpenApiSchemaType.Array.GetStringValue() ||
                   source.Type == OpenApiSchemaType.Object.GetStringValue() || source.Reference != null;
        }

        /// <summary>
        /// It replaces the properties of <see cref="source"/> with the <see cref="other"/> when <see cref="source"/> properties are default.
        /// The changes are saved on <see cref="source"/>. 
        /// <remarks>The definition of <i>default</i>: null or false.</remarks>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="other"></param>
        public static void ReplacePropertiesIfDefaultWith(this OpenApiSchema source, OpenApiSchema other)
        {
            source.Default ??= other.Default;
            source.Deprecated = source.Deprecated == false && other.Deprecated;
            source.Description ??= other.Description;
            source.Discriminator ??= other.Discriminator;
            source.Enum ??= other.Enum;
            source.Example ??= other.Example;
            source.Extensions ??= other.Extensions;
            source.Format ??= other.Format;
            source.Items ??= other.Items;
            source.Maximum ??= other.Maximum;
            source.Minimum ??= other.Minimum;
            source.Not ??= other.Not;
            source.Nullable = source.Nullable == false && other.Nullable;
            source.Pattern ??= other.Pattern;
            source.Properties ??= other.Properties;
            source.Reference ??= other.Reference;
            source.Required ??= other.Required;
            source.Title ??= other.Title;
            source.Type ??= other.Type;
            source.Xml ??= other.Xml;
            source.AdditionalProperties ??= other.AdditionalProperties;
            source.AllOf ??= other.AllOf;
            source.AnyOf ??= other.AnyOf;
            source.ExclusiveMaximum ??= other.ExclusiveMaximum;
            source.ExclusiveMinimum ??= other.ExclusiveMinimum;
            source.ExternalDocs ??= other.ExternalDocs;
            source.MaxItems ??= other.MaxItems;
            source.MaxLength ??= other.MaxLength;
            source.MaxProperties ??= other.MaxProperties;
            source.MinItems ??= other.MinItems;
            source.MinLength ??= other.MinLength;
            source.MinProperties ??= other.MinProperties;
            source.MultipleOf ??= other.MultipleOf;
            source.OneOf ??= other.OneOf;
            source.ReadOnly = source.ReadOnly == false && other.ReadOnly;
            source.UniqueItems ??= other.UniqueItems;
            source.UnresolvedReference = source.UnresolvedReference == false && other.UnresolvedReference;
            source.WriteOnly = source.WriteOnly == false && other.WriteOnly;
            source.AdditionalPropertiesAllowed =
                source.AdditionalPropertiesAllowed == false && other.AdditionalPropertiesAllowed;
        }
    }
}