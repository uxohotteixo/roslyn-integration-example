using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PublicProperty
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PublicPropertyAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ProtectedSetPropertyDiagnosticId";

        private const string Title = "\"Public\" modificator on setter.";

        private const string MessageFormat = "Setter should have \"protected\" modificator.";

        private const string Description = "Setter should have \"protected\" modificator.";

        public const string CodeFixTitle = "Make \"protected\" modificator";

        private const string Category = "Incapsulation";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.PropertyDeclaration);
        }

        private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
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
    }
}
