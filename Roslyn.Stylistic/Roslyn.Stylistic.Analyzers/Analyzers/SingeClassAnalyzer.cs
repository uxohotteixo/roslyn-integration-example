using System;
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
    internal class SingeClassAnalyzer
    {
        public const string DiagnosticId = "SingeClassDiagnosticId";

        private const string Title = "More than one class in source file.";

        private const string MessageFormat = "Only one class is available in source file.";

        private const string Description = "Only one class is available in source file.";

        public const string CodeFixTitle = "Move class in another file.";

        private const string Category = "Incapsulation";

        private static IEnumerable<string> AvailablePrefixes => new List<string> { "is" };

        public static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public static void Analyze(SyntaxTreeAnalysisContext context)
        {
            var root = context.Tree.GetRoot();

            var extraClasses = root.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Where(cls => cls.Parent.IsKind(SyntaxKind.NamespaceDeclaration))
                .Skip(1)
                .ToList();

            extraClasses.ForEach(cls => context
                .ReportDiagnostic(Diagnostic.Create(Rule, cls.Identifier.GetLocation(), cls.Identifier.Text)));
        }


        public static async Task<Solution> CodeFix(Document document, CodeFixContext context, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
