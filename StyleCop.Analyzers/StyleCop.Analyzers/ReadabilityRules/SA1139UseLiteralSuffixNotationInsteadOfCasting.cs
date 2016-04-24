// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

/* Contributor: Tomasz Maczyński */

namespace StyleCop.Analyzers.ReadabilityRules
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    /// <summary>
    /// A cast is performed instead of using literal of a number. Use "U" suffix to create 32-bit unsigned integer literal, "L" for 64-bit integer literal and "UL" for 64-bit unsigned integer literal.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class SA1139UseLiteralSuffixNotationInsteadOfCasting : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="SA1139UseLiteralSuffixNotationInsteadOfCasting"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "SA1139";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(ReadabilityResources.SA1139Title), ReadabilityResources.ResourceManager, typeof(ReadabilityResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(ReadabilityResources.SA1139MessageFormat), ReadabilityResources.ResourceManager, typeof(ReadabilityResources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(ReadabilityResources.SA1139Description), ReadabilityResources.ResourceManager, typeof(ReadabilityResources));
        private static readonly string HelpLink = "https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1139.md";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, AnalyzerCategory.ReadabilityRules, DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, HelpLink);

        private static readonly Action<CompilationStartAnalysisContext> CompilationStartAction = HandleCompilationStart;
        private static readonly Action<SyntaxNodeAnalysisContext> GenericNameAction = HandleGenericName;

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(CompilationStartAction);
        }

        private static void HandleCompilationStart(CompilationStartAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionHonorExclusions(GenericNameAction, SyntaxKind.CastExpression);
        }

        private static void HandleGenericName(SyntaxNodeAnalysisContext context)
        {
            CastExpressionSyntax castExpressionSyntax = (CastExpressionSyntax)context.Node;

            var exprNodes = castExpressionSyntax.ChildNodes().ToList();
            if (exprNodes.Count != 2)
            {
                return;
            }

            // TODO: handle cases when .NET type names are used e.g. Int64
            // TODO: handle real literals https://msdn.microsoft.com/en-us/library/aa691085(v=vs.71).aspx
            var castingToTypeSyntax = exprNodes[0] as PredefinedTypeSyntax;
            var castedElementTypeSyntax = exprNodes[1] as LiteralExpressionSyntax;

            if (castingToTypeSyntax == null || castedElementTypeSyntax == null)
            {
                return;
            }

            var syntaxKindKeyword = castingToTypeSyntax.Keyword.Kind();

            if (syntaxKindKeyword != SyntaxKind.LongKeyword
                && syntaxKindKeyword != SyntaxKind.ULongKeyword
                && syntaxKindKeyword != SyntaxKind.UIntKeyword)
            {
                return;
            }

            var castedToken = castedElementTypeSyntax.Token;
            if (!castedToken.IsKind(SyntaxKind.NumericLiteralToken))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, castingToTypeSyntax.GetLocation()));
        }
    }
}
