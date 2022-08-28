using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;
using NDiff.ExtensionMethods;
using ContentType = NDiff.Enums.ContentType;

namespace NDiff.Services.Generators
{
    public static class ContentGenerator
    {
        /// <summary>
        /// Generates the Content of <see cref="OpenApiResponse"/>.
        /// Content property contains the response type and the link to the schema of <see cref="Type"/> specified in ProducesResponseType.
        /// </summary>
        /// <param name="openApiMedia">The media type instance.</param>
        /// <param name="contentTypes">The content types that are supported.</param>
        /// <returns></returns>
        public static Dictionary<string, OpenApiMediaType> GenerateContent(OpenApiMediaType openApiMedia,
            List<string> contentTypes)
        {
            if (contentTypes is null || !contentTypes.Any())
                return new Dictionary<string, OpenApiMediaType>()
                {
                    [MediaTypeNames.Application.Json] = openApiMedia,
                    [MediaTypeNames.Text.Plain] = openApiMedia,
                    [ContentType.TextPlain.GetStringValue()] = openApiMedia
                };

            return contentTypes.ToDictionary(contentType => contentType, _ => openApiMedia);
        }

        /// <summary>
        /// Generates <see cref="OpenApiMediaType"/> instance.
        /// </summary>
        /// <param name="argument">The type symbol provided as <see cref="ITypeSymbol"/>.</param>
        /// <returns>An <see cref="OpenApiMediaType"/> instance.</returns>
        public static OpenApiMediaType GenerateOpenApiMedia(ITypeSymbol argument)
        {
            return new OpenApiMediaType
            {
                Schema = argument.CreateOpenApiSchema()
            };
        }

        /// <summary>
        /// <inheritdoc cref="GenerateOpenApiMedia(Microsoft.CodeAnalysis.ITypeSymbol)"/>
        /// </summary>
        /// <param name="schema">The given schema.</param>
        /// <returns>An <see cref="OpenApiMediaType"/> instance.</returns>
        public static OpenApiMediaType GenerateOpenApiMedia(OpenApiSchema schema)
        {
            return new OpenApiMediaType
            {
                Schema = schema
            };
        }
    }
}