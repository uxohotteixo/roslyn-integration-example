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
    internal class SingleLineAttributesAnalyzer
    {
        public const string DiagnosticId = "SingleLineAttrsDiagnosticId";

        private const string Title = "Attributes positioned on one line.";

        private const string MessageFormat = "Attributes should be positioned on own line.";

        private const string Description = "Bool variable should have is prefix.";

        private const string Category = "Positioning";

        public const string CodeFixTitle = "Break attributes on lines.";

        public static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public static void Analyze(SyntaxTreeAnalysisContext context)
        {
            var tree = context.Tree;

            if (!tree.HasCompilationUnitRoot) return;

            var root = tree.GetRoot();

            var aa = root.DescendantNodes()
                .OfType<AccessorDeclarationSyntax>()
                .FirstOrDefault();

            var singleLineAttrsGrouped = root.DescendantNodes()
                .OfType<AttributeListSyntax>()
                .Select(attr => new { Attribute = attr, StartLine = tree.GetLineSpan(attr.Span).StartLinePosition.Line })
                .GroupBy(x => x.StartLine)
                .Where(gr => gr.Count() > 1);

            foreach (var grouped in singleLineAttrsGrouped)
            {
                foreach (var item in grouped)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, item.Attribute.GetLocation()));
                }
            }
        }

        public static async Task<Solution> CodeFix(Document document, CodeFixContext context,
            CancellationToken cancellationToken)
        {
            var root = await context.Document
                .GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var node = (ClassDeclarationSyntax)root.FindNode(diagnosticSpan).Parent;

            var attrs = node.AttributeLists.Skip(1).ToArray();

            foreach (var attr in attrs)
            {
                root = root.RemoveNode(attr, SyntaxRemoveOptions.KeepTrailingTrivia);
            }

            var first = node.AttributeLists.First();
            var reversed = attrs.Reverse().ToArray();
            foreach (var attrNode in reversed)
            {
                var newAttr = SyntaxFactory.AttributeList(attrNode.Attributes);
                root = root.InsertNodesAfter(root.FindNode(first.Span), new [] { newAttr });
            }

            return document.WithSyntaxRoot(root).Project.Solution;
        }
    }
}
