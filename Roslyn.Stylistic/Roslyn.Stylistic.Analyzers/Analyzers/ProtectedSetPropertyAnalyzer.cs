using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslyn.Stylistic.Analyzers.Analyzers
{
    internal class ProtectedSetPropertyAnalyzer
    {
        public const string DiagnosticId = "ProtectedSetPropertyDiagnosticId";

        private const string Title = "\"Public\" modificator on setter.";

        private const string MessageFormat = "Setter should have \"protected\" modificator.";

        private const string Description = "Setter should have \"protected\" modificator.";

        public const string CodeFixTitle = "Make \"protected\" modificator";

        private const string Category = "Incapsulation";

        private static IEnumerable<string> AvailablePrefixes => new List<string> { "is" };

        public static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public static void Analyze(SyntaxNodeAnalysisContext context)
        {
            var propertyNode = (PropertyDeclarationSyntax)context.Node;

            if (!propertyNode.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword))) return;

            var setAccessor =
                propertyNode.AccessorList.Accessors.FirstOrDefault(acc =>
                    acc.IsKind(SyntaxKind.SetAccessorDeclaration));

            if (setAccessor != null && (!setAccessor.Modifiers.Any()
                || setAccessor.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword))))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule,
                    propertyNode.Identifier.GetLocation(),
                    propertyNode.Identifier.Text));
            }
        }

        public static async Task<Solution> CodeFix(Document document, CodeFixContext context, CancellationToken cancellationToken)
        {
            var root = await context.Document
                .GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var property = (PropertyDeclarationSyntax)root.FindNode(diagnosticSpan);
            var setAccessor = property.AccessorList.Accessors
                .FirstOrDefault(acc => acc.IsKind(SyntaxKind.SetAccessorDeclaration));

            var protectedModifier = SyntaxFactory.Token(SyntaxKind.ProtectedKeyword);

            var modified = setAccessor.WithModifiers(SyntaxFactory.TokenList(protectedModifier));
            var newRoot = root.ReplaceNode(setAccessor, modified);

            return document.WithSyntaxRoot(newRoot).Project.Solution;
        }
    }
}
