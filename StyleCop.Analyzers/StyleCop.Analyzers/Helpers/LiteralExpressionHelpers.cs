// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class LiteralExpressionHelpers
    {
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

        private static readonly char[] LettersAllowedInLiteralSuffix =
            LettersAllowedInIntegerLiteralSuffix.Concat(LettersAllowedInRealLiteralSuffix).ToArray();

        private static readonly RegexOptions LiteralRegexOptions = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
        private static readonly Regex IntegerBase10Regex = new Regex("^([0-9]*)(|u|l|ul)$", LiteralRegexOptions, Regex.InfiniteMatchTimeout);
        private static readonly Regex IntegerBase16Regex = new Regex("^(0x)([0123456789abcdef]*)(|u|l|ul)$", LiteralRegexOptions, Regex.InfiniteMatchTimeout);
        private static readonly Regex RealRegex = new Regex("^([0-9]*)(m|f|d)|([0-9]*)[.[0-9]*[e[0-9{1,2}]]([|m|f|d])]$", LiteralRegexOptions, Regex.InfiniteMatchTimeout);

        internal static SyntaxKind GetCorrespondingSyntaxKind(LiteralExpressionSyntax literalExprssionSyntax)
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
            else if (RealLiteralSuffixToLiteralSyntaxKind.TryGetValue(suffix, out syntaxKind))
            {
                return syntaxKind;
            }

            throw new ArgumentException($"There is no integer nor real numeric literal with suffix '{suffix}'.");
        }

        private static bool IsIntegerLiteral(string literal) =>
            IntegerBase10Regex.IsMatch(literal) || IntegerBase16Regex.IsMatch(literal);

        private static bool IsRealLiteral(string literal) =>
            RealRegex.IsMatch(literal);

        internal static string StripLiteralSuffix(string literal)
        {
            int suffixStartIndex = literal.IndexOfAny(LettersAllowedInLiteralSuffix);
            return suffixStartIndex == -1 ? literal : literal.Substring(0, suffixStartIndex);
        }

        private static char[] GetCharsFromKeysLowerAndUpperCase(IDictionary<string, SyntaxKind> dict)
        {
            return dict.Keys
                    .SelectMany(s => s.ToCharArray()).Distinct()
                    .SelectMany(c => new[] { char.ToLowerInvariant(c), char.ToUpper(c) })
                    .ToArray();
        }
    }
}
