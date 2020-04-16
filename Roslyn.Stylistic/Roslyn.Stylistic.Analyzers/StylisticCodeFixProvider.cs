using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Roslyn.Stylistic.Analyzers.Analyzers;

namespace Roslyn.Stylistic.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(StylisticCodeFixProvider)), Shared]
    public class StylisticCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(
                BoolVariableAnalyzer.DiagnosticId, 
                SingleLineAttributesAnalyzer.DiagnosticId,
                ProtectedSetPropertyAnalyzer.DiagnosticId); }
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();

            switch (diagnostic.Id)
            {
                case BoolVariableAnalyzer.DiagnosticId:
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: BoolVariableAnalyzer.CodeFixTitle,
                            createChangedSolution: c => BoolVariableAnalyzer.CodeFix(context.Document, context, c),
                            equivalenceKey: BoolVariableAnalyzer.CodeFixTitle),
                        diagnostic);
                    break;
                case SingleLineAttributesAnalyzer.DiagnosticId:
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: SingleLineAttributesAnalyzer.CodeFixTitle,
                            createChangedSolution: c => SingleLineAttributesAnalyzer.CodeFix(context.Document, context, c),
                            equivalenceKey: SingleLineAttributesAnalyzer.CodeFixTitle),
                        diagnostic);
                    break;
                case ProtectedSetPropertyAnalyzer.DiagnosticId:
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: ProtectedSetPropertyAnalyzer.CodeFixTitle,
                            createChangedSolution: c => ProtectedSetPropertyAnalyzer.CodeFix(context.Document, context, c),
                            equivalenceKey: ProtectedSetPropertyAnalyzer.CodeFixTitle),
                        diagnostic);
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
