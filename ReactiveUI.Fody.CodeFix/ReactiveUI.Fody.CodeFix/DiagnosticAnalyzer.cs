using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ReactiveUI.Fody.CodeFix
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ReactiveUIFodyCodeFixAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ReactiveUIFodyCodeFix";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly string Title = "ReactiveUI.Fody CodeFix";
        private static readonly string MessageFormat = "The property {0} can be simplified using ReactiveUI.Fody";

        private static readonly string Description =
            "This code fix changes boiler plate INPC property declaration to simple ones using a fody attribute";

        private const string Category = "Properties";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(ChangePropertyModifierRule, SymbolKind.Property);
        }


        private static void ChangePropertyModifierRule(SymbolAnalysisContext context)
        {
            var propertySymbol = context.Symbol as IPropertySymbol;


            // Check if the ReactiveAttribute is already applied and if so 
            // skip it.
            var attributes = propertySymbol.GetAttributes();
            if (attributes.Any(attr => attr.AttributeClass.Name == "Reactive"))
                return;

            if (propertySymbol != null)
            {
                var property = propertySymbol.DeclaringSyntaxReferences[0].GetSyntax() as PropertyDeclarationSyntax;
                if (property == null)
                    return;

                var getter = property.AccessorList?.Accessors.FirstOrDefault(x => x.IsKind(SyntaxKind.GetAccessorDeclaration));
                var setter = property.AccessorList?.Accessors.FirstOrDefault(x => x.IsKind(SyntaxKind.SetAccessorDeclaration));

                if (setter?.Body == null)
                    return;
                if (getter?.Body == null)
                    return;

                if (setter.Body.Statements.Count != 1)
                    return;

                var statement = setter.Body;
                if (!statement.ToString().Contains("RaiseAndSetIfChanged"))
                    return;
                var idiom = $"{{this.RaiseAndSetIfChanged(ref_{propertySymbol.Name},value);}}";
                var noWhiteSpace = System.Text.RegularExpressions.Regex.Replace(statement.ToString(), @"\s+", "");

                if (idiom != noWhiteSpace)
                    return;

                // Add "contextual" menu to change property modifiers.
                var diagnostic = Diagnostic.Create(Rule, propertySymbol.Locations[0], propertySymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
