// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

/* Contributor: Tomasz Maczyński */

namespace StyleCop.Analyzers.ReadabilityRules
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using StyleCop.Analyzers.Helpers;
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
        private static readonly IDictionary<string, SyntaxKind> IntegerLiteralSuffixToLiteralSyntaxKind = 
            new Dictionary<string, SyntaxKind>(StringComparer.OrdinalIgnoreCase)
            {
                { string.Empty, SyntaxKind.IntKeyword },
                { "L", SyntaxKind.LongKeyword },
                { "UL", SyntaxKind.ULongKeyword },
                { "U", SyntaxKind.UIntKeyword },
                { "D", SyntaxKind.DoubleKeyword },
            };

        private static readonly IDictionary<string, SyntaxKind> RealLiteralSuffixToLiteralSyntaxKind =
            new Dictionary<string, SyntaxKind>(StringComparer.OrdinalIgnoreCase)
            {
                { "F", SyntaxKind.FloatKeyword },
                { "D", SyntaxKind.DoubleKeyword },
                { "M", SyntaxKind.DecimalKeyword }
            };

        private static readonly char[] LettersAllowedInIntegerLiteralSuffix =
            GetCharsFromKeysLowerAndUpperCase(IntegerLiteralSuffixToLiteralSyntaxKind);

        private static readonly char[] LettersAllowedInRealLiteralSuffix =
            GetCharsFromKeysLowerAndUpperCase(RealLiteralSuffixToLiteralSyntaxKind);

        private static readonly RegexOptions LiteralRegexOptions = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
        private static readonly Regex IntegerBase10Regex = new Regex("^([0-9]*)(|u|l|ul)$", LiteralRegexOptions, Regex.InfiniteMatchTimeout);
        private static readonly Regex IntegerBase16Regex = new Regex("^(0x)([0123456789abcdef]*)(|u|l|ul)$", LiteralRegexOptions, Regex.InfiniteMatchTimeout);
        private static readonly Regex RealRegex = new Regex("^([0-9]*)(m|f|d)|([0-9]*)[.[0-9]*[e[0-9{1,2}]]([|m|f|d])]$", LiteralRegexOptions, Regex.InfiniteMatchTimeout);

        private static char[] GetCharsFromKeysLowerAndUpperCase(IDictionary<string, SyntaxKind> dict)
        {
            return dict.Keys
                    .SelectMany(s => s.ToCharArray()).Distinct()
                    .SelectMany(c => new[] { char.ToLowerInvariant(c), char.ToUpper(c) })
                    .ToArray();
        }

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

            var unaryExpressionSyntax = castExpressionSyntax.Expression as PrefixUnaryExpressionSyntax;
            if (unaryExpressionSyntax != null)
            {
                if (unaryExpressionSyntax.Kind() != SyntaxKind.UnaryPlusExpression
                    && unaryExpressionSyntax.Kind() != SyntaxKind.UnaryMinusExpression)
                {
                    // don't raport diagnostic if bit operations are performed and for some invalid code (eg. "(long)++1")
                    return;
                }
            }

            var castedElementTypeSyntax = unaryExpressionSyntax == null ?
                castExpressionSyntax.Expression as LiteralExpressionSyntax :
                unaryExpressionSyntax.Operand as LiteralExpressionSyntax;

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

            if (!context.SemanticModel.GetConstantValue(context.Node).HasValue)
            {
                // cast does not have a valid value (like "(ulong)-1") which is reported as error
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, castExpressionSyntax.GetLocation()));
        }

        private static SyntaxKind GetCorrespondingSyntaxKind(LiteralExpressionSyntax literalExprssionSyntax)
        {
            var literalText = literalExprssionSyntax.Token.Text;
            int suffixStartIndex = -1;
            if (IsIntegerLiteral(literalExprssionSyntax.Token.Text))
            {
                suffixStartIndex = literalText.IndexOfAny(LettersAllowedInIntegerLiteralSuffix);
            }
            else if (IsRealLiteral(literalExprssionSyntax.Token.Text))
            {
                suffixStartIndex = literalText.IndexOfAny(LettersAllowedInRealLiteralSuffix);
            }

            var suffix = suffixStartIndex == -1 ?
                string.Empty :
                literalText.Substring(suffixStartIndex, length: literalText.Length - suffixStartIndex);
            return GetLiteralSyntaxKindBySuffix(suffix);
        }

        private static SyntaxKind GetLiteralSyntaxKindBySuffix(string suffix)
        {
            SyntaxKind syntaxKind;
            if (IntegerLiteralSuffixToLiteralSyntaxKind.TryGetValue(suffix, out syntaxKind))
            {
                return syntaxKind;
            }

            return RealLiteralSuffixToLiteralSyntaxKind[suffix];
        }

        private static bool IsIntegerLiteral(string literal) =>
            IntegerBase10Regex.IsMatch(literal) || IntegerBase16Regex.IsMatch(literal);

        private static bool IsRealLiteral(string literal) =>
            RealRegex.IsMatch(literal);
    }
}
