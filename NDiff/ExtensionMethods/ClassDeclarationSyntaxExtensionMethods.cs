using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NDiff.Enums;

namespace NDiff.ExtensionMethods
{
    public static class ClassDeclarationSyntaxExtensionMethods
    {
        /// <summary>
        /// Filters only classes that are not nested in another class.
        /// </summary>
        /// <param name="enumerable"></param>
        /// <returns>Returns an <see cref="IEnumerable{T}" /> of type <see cref="ClassDeclarationSyntax"/>.</returns>
        public static IEnumerable<ClassDeclarationSyntax> IsNotNestedClass(
            this IEnumerable<ClassDeclarationSyntax> enumerable)
        {
            return enumerable.Where(classSyntax => classSyntax.Parent is not ClassDeclarationSyntax);
        }
        
        /// <summary>
        /// Filters only classes that are valid to be considered as Controllers. When traversing these controllers, it will also analyze other classes that were filtered here.
        /// </summary>
        /// <param name="enumerable"></param>
        /// <returns>Returns an <see cref="IEnumerable{T}" /> of type <see cref="ClassDeclarationSyntax"/>.</returns>
        public static IEnumerable<ClassDeclarationSyntax> IsValidControllerClass(
            this IEnumerable<ClassDeclarationSyntax> enumerable)
        {
            return enumerable.Where(classSyntax => classSyntax.Modifiers.Count > 0 &&
                                                   !classSyntax.Modifiers.Any(el =>
                                                       el.Text == ModifiersType.Static.GetStringValue() ||
                                                       el.Text == ModifiersType.Private.GetStringValue() ||
                                                       el.Text == ModifiersType.Protected.GetStringValue() ||
                                                       el.Text == ModifiersType.Internal.GetStringValue() ||
                                                       el.Text == ModifiersType.Abstract.GetStringValue() ||
                                                       el.Text != ModifiersType.Public.GetStringValue()));
        }
        
        /// <summary>
        /// Indicates whether this type and all derived types (in case the derived types do not override by adding NonController attribute)
        /// are used to serve HTTP API responses. This method checks only for ApiControllerAttribute.
        /// For this comparison the name of Controller is irrelevant (it can be any). 
        /// </summary>
        /// <param name="classDeclarationSyntax"></param> 
        /// <returns>True if it contains ApiController attribute, otherwise, false.</returns>
        public static bool ContainsApiControllerAttribute(this ClassDeclarationSyntax classDeclarationSyntax)
        {
            return classDeclarationSyntax.AttributeLists
                .SelectMany(attributeList => attributeList.Attributes)
                .Any(attribute => attribute.Name.ToString() == AttributeType.ApiController.GetStringValue());
        }
        
        /// <summary>
        /// Checks if the class name ends with Controller.
        /// If it ends with Controller, this class is considered a Controller (but not necessarily the derived classes).
        /// </summary>
        /// <param name="classDeclarationSyntax"></param>
        /// <returns>True if class name ends with Controller, otherwise, false.</returns>
        /// <remarks>Note that even if the class is not a Controller, its http methods may be used by derived classes.</remarks>
        public static bool EndsWithController(this ClassDeclarationSyntax classDeclarationSyntax)
        {
            return classDeclarationSyntax.Identifier.ToString().EndsWith(ClassType.Controller.GetStringValue());
        }
        
        /// <summary>
        /// Checks if the class contains the NonController attribute.
        /// </summary>
        /// <param name="classDeclarationSyntax"></param>
        /// <returns>True if it contains, otherwise, false.</returns>
        /// <remarks>In case the class has NonController attribute, this class and all derived classes are NOT controllers, therefore, can be immediately skipped.</remarks>
        public static bool ContainsNonControllerAttribute(this ClassDeclarationSyntax classDeclarationSyntax)
        {
            return classDeclarationSyntax.AttributeLists
                .SelectMany(attributeList => attributeList.Attributes)
                .Any(attribute => attribute.Name.ToString() == AttributeType.NonController.GetStringValue());
        }
        
        /// <summary>
        /// Retrieves the Route attributes of the class.
        /// </summary>
        /// <param name="classDeclarationSyntax"></param>
        /// <returns></returns>
        public static IEnumerable<AttributeSyntax> GetControllerRoutes(
            this ClassDeclarationSyntax classDeclarationSyntax)
        {
            return classDeclarationSyntax.AttributeLists
                .SelectMany(attributeList => attributeList.Attributes)
                .Where(attribute => attribute.Name.ToString() == AttributeType.Route.GetStringValue());
        }
    }
}