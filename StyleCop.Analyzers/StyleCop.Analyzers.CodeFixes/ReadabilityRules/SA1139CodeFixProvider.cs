// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

/* Contributor: Tomasz Maczyński */

namespace StyleCop.Analyzers.ReadabilityRules
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Implements a code fix for <see cref="SA1139UseLiteralSuffixNotationInsteadOfCasting"/>.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SA1139CodeFixProvider))]
    [Shared]
    internal class SA1139CodeFixProvider : CodeFixProvider
    {
        private static readonly Dictionary<SyntaxKind, string> LiteralSyntaxKindToSuffix = new Dictionary<SyntaxKind, string>()
            {
                { SyntaxKind.LongKeyword, "L" },
                { SyntaxKind.ULongKeyword, "UL" },
                { SyntaxKind.UIntKeyword, "U" }
            };

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(SA1139UseLiteralSuffixNotationInsteadOfCasting.DiagnosticId);

        /// <inheritdoc/>
        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        ReadabilityResources.SA1139CodeFix,
                        cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, cancellationToken),
                        nameof(SA1139CodeFixProvider)),
                    diagnostic);
            }

            return SpecializedTasks.CompletedTask;
        }

        private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var node = syntaxRoot.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true) as CastExpressionSyntax;
            if (node == null)
            {
                return document;
            }

            var replacementNode = GenerateReplacementNode(node);
            var newSyntaxRoot = syntaxRoot.ReplaceNode(node, replacementNode);
            return document.WithSyntaxRoot(newSyntaxRoot);
        }

        private static SyntaxNode GenerateReplacementNode(CastExpressionSyntax node)
        {
            var literalExpressionSyntax = (LiteralExpressionSyntax)node.Expression;
            var typeToken = node.Type.GetFirstToken();
            var correspondingSuffix = LiteralSyntaxKindToSuffix[typeToken.Kind()];
            var fixedCode = SyntaxFactory.ParseExpression(literalExpressionSyntax.Token.Text + correspondingSuffix);
            return fixedCode.WithTriviaFrom(node);
        }
    }
}
