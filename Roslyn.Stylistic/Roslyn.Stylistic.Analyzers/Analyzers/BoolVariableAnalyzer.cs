using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Rename;

namespace Roslyn.Stylistic.Analyzers.Analyzers
{
    internal static class BoolVariableAnalyzer
    {
        public const string DiagnosticId = "CorrectBoolNameDiagnosticId";

        private const string Title = "Bool local variable has incorrect name.";

        private const string MessageFormat = "Bool local variable should have is prefix.";

        private const string Description = "Bool local variable should have \"is\" prefix.";

        public const string CodeFixTitle = "Add \"is\" prefix";

        private const string Category = "Naming";

        private static IEnumerable<string> AvailablePrefixes => new List<string> { "is" };

        public static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public static void Analyze(SemanticModelAnalysisContext context)
        {
            var semanticModel = context.SemanticModel;

            var variableDeclarations = semanticModel.SyntaxTree
                .GetRoot()
                .DescendantNodes()
                .OfType<VariableDeclarationSyntax>()
                .Where(node =>
                    node.Ancestors().OfType<LocalDeclarationStatementSyntax>().Any()
                );

            foreach (var declaration in variableDeclarations)
            {
                var symbol = semanticModel.GetSymbolInfo(declaration.Type).Symbol;
                var symbolType = semanticModel.GetTypeInfo(declaration.Type).Type;
                if (symbolType.SpecialType != SpecialType.System_Boolean) continue;
                foreach (var variable in declaration.Variables)
                {
                    if (!AvailablePrefixes.Any(ap => variable.Identifier.Text.StartsWith(ap)))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Rule, variable.Identifier.GetLocation(), symbol.Name));
                    }
                }
            }
        }

        public static async Task<Solution> CodeFix(Document document, CodeFixContext context, CancellationToken cancellationToken)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
  
            var localDecl = (VariableDeclaratorSyntax) root.FindNode(diagnosticSpan);

            var identifierTokenName = localDecl.Identifier.Text;

            var newName = "is" + char.ToUpperInvariant(identifierTokenName[0]) 
                               + identifierTokenName.Substring(1, identifierTokenName.Length - 1);

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var typeSymbol = semanticModel.GetDeclaredSymbol(localDecl, cancellationToken);

            var originalSolution = document.Project.Solution;
            var optionSet = originalSolution.Workspace.Options;
            var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, newName, optionSet, cancellationToken).ConfigureAwait(false);

            return newSolution;
        }
    }
}
