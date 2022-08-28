using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NDiff.ExtensionMethods;
using NDiff.Models;
using NDiff.Services.Generators;

namespace NDiff.Services.Analyzers.ClassAnalyzers
{
    public class ClassAnalyzer : IClassAnalyzer
    {
        private readonly IAnalyzedClassesState _analyzedClassesState;

        public ClassAnalyzer(IAnalyzedClassesState analyzedClassesState)
        {
            _analyzedClassesState = analyzedClassesState;
        }

        public void AnalyzeClass(ITypeSymbol classSymbol, ClassDeclarationSyntax classDeclarationSyntax)
        {
            if (_analyzedClassesState.IsClassAlreadyAnalyzed(classSymbol) || classDeclarationSyntax is null)
            {
                return;
            }

            var classInformation = new ClassInformation
            {
                IsControllerExtendable = classDeclarationSyntax.ContainsApiControllerAttribute() ||
                                         classSymbol.IsBaseTypeController(),
                ContainsNonApiAttribute = classDeclarationSyntax.ContainsNonControllerAttribute(),
                ClassDeclarationSyntax = classDeclarationSyntax,
                ClassSymbol = classSymbol,
                ControllerName = classDeclarationSyntax.Identifier.ToString()
            };

            classInformation.IsClassController =
                classInformation.IsControllerExtendable || classDeclarationSyntax.EndsWithController();

            // if it contains non api attribute no need to further analyze because it is not a controller, nor are the derived classes.
            if (classInformation.ContainsNonApiAttribute)
            {
                _analyzedClassesState.AddAnalyzedClass(classSymbol, classInformation);
                return;
            }

            // what if the base type is object and does not end with controller??
            // Analyze it because it may have endpoints that can be inherited from other Controller classes.
            if (classSymbol.IsBaseTypeController() || classSymbol.IsBaseTypeObject() ||
                classSymbol.GetBaseTypeClassDeclarationSyntax() is null)
            {
                var analyzedClassInformation = OpenApiGenerator.AnalyzeBaseClass(classInformation);
                // This is a normal class to be analyzed and put it in dictionary - it is the parent.
                _analyzedClassesState.AddAnalyzedClass(classSymbol, analyzedClassInformation);
            }
            else
            {
                // this has to be compared with the parent class.
                AnalyzeClass(classSymbol.BaseType, classSymbol.GetBaseTypeClassDeclarationSyntax());

                if (classSymbol.BaseType == null) return;
                _analyzedClassesState.TryGetAnalyzedClassValue(classSymbol.BaseType, out var parent);

                // if parent has nonApi then this class is also not a controller
                if (parent.ContainsNonApiAttribute)
                {
                    classInformation.ContainsNonApiAttribute = true;
                    _analyzedClassesState.AddAnalyzedClass(classSymbol, classInformation);
                    return;
                }

                // if parent is controllerExtendable, then this class is a controller, as well as, extendable
                if (parent.IsControllerExtendable)
                {
                    classInformation.IsControllerExtendable = true;
                    // IsClassController is used at the end when we generate the document, we include only those that have IsClassController bit set.
                    classInformation.IsClassController = true;
                }

                var updatedClassInformation = OpenApiGenerator.AnalyzeChildParentClasses(classInformation, parent);
                _analyzedClassesState.AddAnalyzedClass(classSymbol, updatedClassInformation);
            }
        }
    }
}