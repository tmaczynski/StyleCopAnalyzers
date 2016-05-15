// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

/* Contributor: Tomasz Maczyński */

using StyleCop.Analyzers.Helpers;

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

        // TODO: remove code duplication
        private static readonly Dictionary<string, SyntaxKind> UppercaseLiteralSuffixToLiteralSyntax = new Dictionary<string, SyntaxKind>()
            {
                { string.Empty, SyntaxKind.IntKeyword },
                { "L", SyntaxKind.LongKeyword },
                { "UL", SyntaxKind.ULongKeyword },
                { "U", SyntaxKind.UIntKeyword },
                { "F", SyntaxKind.FloatKeyword },
                { "D", SyntaxKind.DoubleKeyword },
                { "M", SyntaxKind.DecimalKeyword }
            };

        private static readonly char[] LettersAllowedInLiteralSuffix = UppercaseLiteralSuffixToLiteralSyntax.Keys
            .SelectMany(s => s.ToCharArray()).Distinct()
            .SelectMany(c => new[] { char.ToLowerInvariant(c), c })
            .ToArray();

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
            var castExpressionSyntax = (CastExpressionSyntax)context.Node;

            var castingToTypeSyntax = castExpressionSyntax.Type as PredefinedTypeSyntax;
            if (castingToTypeSyntax == null)
            {
                return;
            }

            var plusMinusSyntax = castExpressionSyntax.Expression as PrefixUnaryExpressionSyntax;
            var castedElementTypeSyntax =
                plusMinusSyntax == null ?
                castExpressionSyntax.Expression as LiteralExpressionSyntax :
                plusMinusSyntax.Operand as LiteralExpressionSyntax;

            if (castedElementTypeSyntax == null)
            {
                return;
            }

            var syntaxKindKeyword = castingToTypeSyntax.Keyword.Kind();
            if (!SyntaxKinds.IntegerLiteralKeyword.Contains(syntaxKindKeyword)
                && !SyntaxKinds.RealLiteralKeyword.Contains(syntaxKindKeyword))
            {
                return;
            }

            var castedToken = castedElementTypeSyntax.Token;
            if (!castedToken.IsKind(SyntaxKind.NumericLiteralToken))
            {
                return;
            }

            if (GetCorrespondingSyntaxKind(castedElementTypeSyntax) == syntaxKindKeyword)
            {
                // cast is redundant which is reported by another diagnostic.
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, castExpressionSyntax.GetLocation()));
        }

        private static SyntaxKind GetCorrespondingSyntaxKind(LiteralExpressionSyntax literalExprssionSyntax)
        {
            var tokenText = literalExprssionSyntax.Token.Text;
            int suffixStartIndex = tokenText.IndexOfAny(LettersAllowedInLiteralSuffix);
            var suffix = suffixStartIndex == -1 ?
                string.Empty :
                tokenText.Substring(suffixStartIndex, tokenText.Length - suffixStartIndex);
            return GetLiteralSyntaxKindBySuffix(suffix);
        }

        private static SyntaxKind GetLiteralSyntaxKindBySuffix(string suffix)
        {
            return UppercaseLiteralSuffixToLiteralSyntax[suffix.ToUpperInvariant()];
        }
    }
}
