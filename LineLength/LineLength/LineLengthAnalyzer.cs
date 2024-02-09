using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace LineLength
{
    [DiagnosticAnalyzer(
        LanguageNames.CSharp,
        LanguageNames.VisualBasic,
        LanguageNames.FSharp)]
    public class LineLengthAnalyzer : DiagnosticAnalyzer
    {
        public const string LengthDiagnosticId = "LK0080";


        private static readonly LocalizableString LengthTitle
            = new LocalizableResourceString(
                nameof(Resources.LengthAnalyzerTitle),
                Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString LengthMessageFormat
            = new LocalizableResourceString(
                nameof(Resources.LengthAnalyzerMessageFormat),
                Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString LengthDescription
            = new LocalizableResourceString(
                nameof(Resources.LengthAnalyzerDescriptor),
                Resources.ResourceManager, typeof(Resources));

        private const string LengthCategory = "LeetKean.CodeCulture";

        internal static readonly DiagnosticDescriptor LinesLengthRule
            = new DiagnosticDescriptor(
                LengthDiagnosticId,
                LengthTitle,
                LengthMessageFormat,
                LengthCategory,
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                description: LengthDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    LinesLengthRule);
            }
        }


        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxTreeAction(ctx =>
            {
                AnalyzeLinesLength(ctx.Tree, ctx.ReportDiagnostic);
            });
        }


        private static void AnalyzeLinesLength(SyntaxTree tree,
            Action<Diagnostic> reportDiagnostic)
        {
            var txt = tree.GetText();
            var diags = new List<Diagnostic>();

            foreach (var line in txt.Lines)
            {
                if ((line.End - line.Start) > 80)
                {
                    var loc = Location.Create(tree, line.Span);
                    diags.Add(Diagnostic.Create(LinesLengthRule, loc));
                }
            }

            foreach (var diag in diags)
            {
                reportDiagnostic(diag);
            }
        }

    }
}
