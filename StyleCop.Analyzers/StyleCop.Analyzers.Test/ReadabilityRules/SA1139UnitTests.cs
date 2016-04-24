// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

/* Contributor: Tomasz Maczyński */

namespace StyleCop.Analyzers.Test.ReadabilityRules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using TestHelper;
    using Xunit;
    using Analyzers.ReadabilityRules;
    using System.Threading;

    public class SA1139UnitTests : CodeFixVerifier
    {
        /// <summary>
        /// Verifies that using literal in a declaration of a class field does not produce diagnostic.
        /// </summary>
        /// <param name="literalType">The type which is checked.</param>
        /// <param name="literalSuffix">The correpsonding literal's suffix.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData("long", "L")]
        [InlineData("long", "l")]
        [InlineData("ulong", "UL")]
        [InlineData("ulong", "ul")]
        [InlineData("ulong", "uL")]
        [InlineData("ulong", "Ul")]
        [InlineData("uint", "U")]
        [InlineData("uint", "u")]
        public async Task TestUsingLiteralsAsClassFieldsDoesNotProduceDiagnosticAsync(string literalType, string literalSuffix)
        {
            var testCode = $@"
class ClassName
{{
    {literalType} x = 1{literalSuffix};
}}
";
            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Verifies that using casts in a declaration of a class field does produce diagnostic.
        /// </summary>
        /// <param name="literalType">The type which is checked.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData("long")]
        [InlineData("ulong")]
        [InlineData("uint")]
        public async Task TestUsingCastsInFieldDeclarationProducesDiagnosticAsync(string literalType)
        {
            var testCode = $@"
class ClassName
{{
    {literalType} x = ({literalType})1;
}}
";
            DiagnosticResult[] expectedDiagnosticResult =
            {
                this.CSharpDiagnostic().WithLocation(4, 11 + literalType.Length)
            };
            await this.VerifyCSharpDiagnosticAsync(testCode, expectedDiagnosticResult, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Verifies that using literals in a method body does not produce diagnostic.
        /// </summary>
        /// <param name="literalSuffix">Literal's suffix.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData("L")]
        [InlineData("l")]
        [InlineData("UL")]
        [InlineData("ul")]
        [InlineData("uL")]
        [InlineData("Ul")]
        [InlineData("U")]
        [InlineData("u")]
        public async Task TestUsingLiteralsInMethodDoesNotProduceDiagnosticAsync(string literalSuffix)
        {
            var testCode = $@"
class ClassName
{{
    public void Method()
    {{
        var x = 1{literalSuffix};
    }}
}}
";
            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Verifies that using casts in a method body produces diagnostic.
        /// </summary>
        /// <param name="literalType">The type which is checked.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData("long")]
        [InlineData("ulong")]
        [InlineData("uint")]
        public async Task TestUsingCastsInMethodProducesDiagnosticAsync(string literalType)
        {
            var testCode = $@"
class ClassName
{{
    public void Method()
    {{
        var x = ({literalType})1;
    }}
}}
";
            DiagnosticResult[] expectedDiagnosticResult =
            {
                this.CSharpDiagnostic().WithLocation(6, 18)
            };
            await this.VerifyCSharpDiagnosticAsync(testCode, expectedDiagnosticResult, CancellationToken.None).ConfigureAwait(false);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            throw new NotImplementedException("CodeFixProvider is not available yet");
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new SA1139UseLiteralSuffixNotationInsteadOfCasting();
        }
    }
}
