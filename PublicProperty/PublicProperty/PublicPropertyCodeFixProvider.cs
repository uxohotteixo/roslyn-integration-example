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

namespace PublicProperty
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PublicPropertyCodeFixProvider)), Shared]
    public class PublicPropertyCodeFixProvider : CodeFixProvider
    {
        private const string title = "Make uppercase";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(PublicPropertyAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedSolution: c => MakeUppercaseAsync(context.Document, context, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Solution> MakeUppercaseAsync(Document document, CodeFixContext context,
            CancellationToken cancellationToken)
        {
            var root = await context.Document
                .GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var property = (PropertyDeclarationSyntax) root.FindNode(diagnosticSpan);
            var setAccessor = property.AccessorList.Accessors
                .FirstOrDefault(acc => acc.IsKind(SyntaxKind.SetAccessorDeclaration));

            var protectedModifier = SyntaxFactory.Token(SyntaxKind.ProtectedKeyword);

            var modified = setAccessor.WithModifiers(SyntaxFactory.TokenList(protectedModifier));
            var newRoot = root.ReplaceNode(setAccessor, modified);

            return document.WithSyntaxRoot(newRoot).Project.Solution;
        }
    }
}
