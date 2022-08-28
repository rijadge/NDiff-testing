using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.OpenApi.Models;

namespace NDiff.Services.Generators
{
    public static class ResponseGenerator
    {
        /// <summary>
        /// Generates the <see cref="OpenApiResponses"/> from the input data.
        /// </summary>
        /// <param name="mediaTypesForStatusCodes"></param>
        /// <param name="contentTypes"></param>
        /// <param name="producesOpenApiMedia"></param>
        /// <param name="producesErrorOpenApiMediaType"></param>
        /// <param name="containsDefaultResponseType"></param>
        /// <param name="producesDefaultOpenApiMediaType"></param>
        /// <returns></returns>
        public static OpenApiResponses GenerateOpenApiResponses(
            Dictionary<int, OpenApiMediaType> mediaTypesForStatusCodes, List<string> contentTypes,
            OpenApiMediaType producesOpenApiMedia, OpenApiMediaType producesErrorOpenApiMediaType,
            bool containsDefaultResponseType,
            OpenApiMediaType producesDefaultOpenApiMediaType)
        {
            var openApiResponses = new OpenApiResponses();

            foreach (var (statusCode, mediaType) in mediaTypesForStatusCodes)
            {
                Dictionary<string, OpenApiMediaType> content;

                if (IsServerErrorRange(producesErrorOpenApiMediaType, mediaType, statusCode))
                    content = ContentGenerator.GenerateContent(producesErrorOpenApiMediaType, contentTypes);
                else
                    content = ContentGenerator.GenerateContent(mediaType, contentTypes);

                var openApiResponse = new OpenApiResponse
                {
                    Content = mediaType == null ? null : content,
                    Description = GetStatusDescription(statusCode)
                };

                openApiResponses[statusCode.ToString()] = openApiResponse;
            }

            // if Produces attribute exists then override the 200OK response.
            if (Default200OkOpenApiMediaExists(producesOpenApiMedia))
            {
                openApiResponses[StatusCodes.Status200OK.ToString()] = new OpenApiResponse()
                {
                    Content = ContentGenerator.GenerateContent(producesOpenApiMedia, contentTypes),
                    Description = GetStatusDescription(StatusCodes.Status200OK)
                };
            }

            if (!containsDefaultResponseType) return openApiResponses;

            {
                // TODO: in case both are null they will have a "ProblemDetails" model. I think this will be general one
                // TODO: even for the producesErrorOpenApiMediaType.
                var content =
                    ContentGenerator.GenerateContent(producesDefaultOpenApiMediaType ?? producesErrorOpenApiMediaType,
                        contentTypes);
                var openApiResponse = new OpenApiResponse
                {
                    Content = content,
                    Description = "Error"
                };
                openApiResponses["default"] = openApiResponse;
            }

            return openApiResponses;
        }

        /// <summary>
        /// Checks if the default 200 OK produces <see cref="OpenApiMediaType"/> is set.
        /// If yes, it will override the 200 OK that may come from ProducesResponseType attribute.
        /// </summary>
        /// <param name="producesOpenApiMedia">The <see cref="OpenApiMediaType"/> for Produces attribute.</param>
        /// <returns>True if Schema and OpenApiMediaType exist; otherwise false.</returns>
        private static bool Default200OkOpenApiMediaExists(OpenApiMediaType producesOpenApiMedia)
        {
            return producesOpenApiMedia is {Schema: { }};
        }

        /// <summary>
        /// Gets the description of status code.
        /// </summary>
        /// <param name="statusCode">Outputs the status code as integer.</param>
        /// <returns>The description of statusCode.</returns>
        private static string GetStatusDescription(int statusCode)
        {
            return ReasonPhrases.GetReasonPhrase(statusCode);
        }

        /// <summary>
        /// Checks if this attribute is in server error range between 400 and 499, and also makes sure that
        /// there is no Type variable in the ProducesResponseType as well as the <see cref="analyzedErrorResponseTypeAttribute"/> is set.
        /// </summary>
        /// <param name="analyzedErrorResponseTypeAttribute"></param>
        /// <param name="producesResponseMediaType"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        private static bool IsServerErrorRange(OpenApiMediaType analyzedErrorResponseTypeAttribute,
            OpenApiMediaType producesResponseMediaType, int statusCode)
        {
            return statusCode is > 399 and < 500 && analyzedErrorResponseTypeAttribute is not null &&
                   producesResponseMediaType?.Schema == null;
        }
    }
}