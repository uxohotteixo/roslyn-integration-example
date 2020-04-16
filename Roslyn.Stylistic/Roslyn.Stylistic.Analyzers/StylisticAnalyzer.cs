using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslyn.Stylistic.Analyzers.Analyzers;

namespace Roslyn.Stylistic.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StylisticAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(BoolVariableAnalyzer.Rule, SingleLineAttributesAnalyzer.Rule, ProtectedSetPropertyAnalyzer.Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSemanticModelAction(BoolVariableAnalyzer.Analyze);
            context.RegisterSyntaxTreeAction(SingleLineAttributesAnalyzer.Analyze);
            context.RegisterSyntaxNodeAction(ProtectedSetPropertyAnalyzer.Analyze, SyntaxKind.PropertyDeclaration);
        }
    }
}
