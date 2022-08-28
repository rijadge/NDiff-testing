using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;
using NDiff.Enums;

namespace NDiff.Helpers
{
    /// <summary>
    /// Used to map the type and format of <see cref="OpenApiSchema"/>.
    /// </summary>
    public static class OpenApiSchemaPropertyMapper
    {
        public static readonly Dictionary<SpecialType, OpenApiSchemaType> TypeMapper =
            new()
            {
                [SpecialType.System_Boolean] = OpenApiSchemaType.Bool,
                [SpecialType.System_DateTime] = OpenApiSchemaType.String,
                [SpecialType.System_String] = OpenApiSchemaType.String,
                [SpecialType.System_Int16] = OpenApiSchemaType.Int,
                [SpecialType.System_Int32] = OpenApiSchemaType.Int,
                [SpecialType.System_Int64] = OpenApiSchemaType.Int,
                [SpecialType.System_UInt16] = OpenApiSchemaType.Int,
                [SpecialType.System_UInt32] = OpenApiSchemaType.Int,
                [SpecialType.System_UInt64] = OpenApiSchemaType.Int,
                [SpecialType.System_Double] = OpenApiSchemaType.Number,
                [SpecialType.System_Decimal] = OpenApiSchemaType.Number,
                [SpecialType.System_Char] = OpenApiSchemaType.String,
                [SpecialType.System_Byte] = OpenApiSchemaType.Int,
                [SpecialType.System_SByte] = OpenApiSchemaType.Int,
                [SpecialType.System_Object] = OpenApiSchemaType.Object
            };

        public static readonly Dictionary<SpecialType, OpenApiSchemaFormat?> FormatMapper =
            new()
            {
                [SpecialType.System_Boolean] = default,
                [SpecialType.System_DateTime] = OpenApiSchemaFormat.Datetime,
                [SpecialType.System_String] = default,
                [SpecialType.System_Int16] = OpenApiSchemaFormat.Int16,
                [SpecialType.System_Int32] = OpenApiSchemaFormat.Int32,
                [SpecialType.System_Int64] = OpenApiSchemaFormat.Int64,
                [SpecialType.System_UInt16] = OpenApiSchemaFormat.UInt16,
                [SpecialType.System_UInt32] = OpenApiSchemaFormat.UInt32,
                [SpecialType.System_UInt64] = OpenApiSchemaFormat.UInt64,
                [SpecialType.System_Double] = OpenApiSchemaFormat.Double,
                [SpecialType.System_Decimal] = default,
                [SpecialType.System_Char] = default,
                [SpecialType.System_Byte] = OpenApiSchemaFormat.UInt8,
                [SpecialType.System_SByte] = OpenApiSchemaFormat.SInt8,
                [SpecialType.System_Object] = default
            };
    }
}