using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.MSBuild;
using ReactiveUI.Fody.Helpers;

namespace ReactiveUI.Fody.CodeFix
{
  [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PropertyToReactiveCodeFixProvider)), Shared]
    public class PropertyToReactiveCodeFixProvider : CodeFixProvider
    {

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ReactiveUIFodyCodeFixAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            context.RegisterCodeFix(CodeAction.Create("Change property to 'reactive'", c => ChangePropertySetAsync(context.Document, diagnostic, c), nameof(PropertyToReactiveCodeFixProvider) ), diagnostic);
            return Task.FromResult(0);
        }

        public static PropertyDeclarationSyntax GenProp(string propName, string typeName)
        {
            return PropertyDeclaration (IdentifierName(typeName), Identifier(propName))
                .WithAttributeLists
                    ( SingletonList ( AttributeList ( SingletonSeparatedList ( Attribute ( IdentifierName("Reactive"))))))
                .AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithAccessorList
                    (
                    AccessorList
                        (
                        List
                            (
                            new[]
                            {
                                AccessorDeclaration ( SyntaxKind.GetAccessorDeclaration) .WithSemicolonToken ( Token(SyntaxKind.SemicolonToken)),
                                AccessorDeclaration ( SyntaxKind.SetAccessorDeclaration) .WithSemicolonToken ( Token(SyntaxKind.SemicolonToken))
                            })));
        }
        [Reactive] public int FooProp { get; set; }


        private async static Task<Document> ChangePropertySetAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var propertyStatement = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<PropertyDeclarationSyntax>().First();
            var propName = propertyStatement.Identifier.Text;

            var propAnnotation = new SyntaxAnnotation();
            var reactiveProperty = GenProp(propName, propertyStatement.Type.ToString()).WithAdditionalAnnotations(propAnnotation);


            var classDeclaration = (ClassDeclarationSyntax) propertyStatement.Parent;
            var variableDeclarator = classDeclaration.DescendantNodes()
                              .OfType<VariableDeclaratorSyntax>()
                              .FirstOrDefault(v => v.Identifier.Text == "_" + propName);

            if (variableDeclarator == null)
                return document;

            var variableAnnotation = new SyntaxAnnotation();

            // Replace the property but mark the field declarator for
            // later processesing
            root = root.ReplaceNodes
                ( new CSharpSyntaxNode[] { propertyStatement, variableDeclarator}
                , (node,_) => node is PropertyDeclarationSyntax ? (SyntaxNode) reactiveProperty
                                                                  : variableDeclarator.WithAdditionalAnnotations(variableAnnotation));

            // Find the variable declarator based on the annotation again.
            variableDeclarator =
                root.DescendantNodes()
                     .OfType<VariableDeclaratorSyntax>()
                     .Where(n => n.HasAnnotation(variableAnnotation))
                     .Single();

            // Check to see if the variable is the only one in the declaration and
            // if it is then remove the entire declaration otherwise just remove
            // the declarator. ie;
            //
            //    int _foo, _bar;
            //
            // transforms to
            //
            //    int _foo;
            //
            // but
            //
            //    int _bar;
            //
            // is completely removed




            var variableDeclaration = (VariableDeclarationSyntax) variableDeclarator.Parent;
            var isOnlyFieldInDeclaration = variableDeclaration.DescendantNodes().OfType<VariableDeclaratorSyntax>().Count()==1;
            root = root.RemoveNode(isOnlyFieldInDeclaration ? variableDeclaration.Parent : variableDeclarator, SyntaxRemoveOptions.KeepExteriorTrivia);

            root = Formatter.Format(root, propAnnotation, MSBuildWorkspace.Create());

            return document.WithSyntaxRoot(root);
        }

    }
}