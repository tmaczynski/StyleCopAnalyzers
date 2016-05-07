// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

/* Contributor: Tomasz Maczyński */

namespace StyleCop.Analyzers.Test.ReadabilityRules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Analyzers.ReadabilityRules;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using TestHelper;
    using Xunit;

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
        [InlineData("ulong", "Ul")]
        [InlineData("ulong", "uL")]
        [InlineData("ulong", "ul")]
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
        /// <param name="literalSuffix">The suffix corresponding to the type</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData("long", "L")]
        [InlineData("ulong", "UL")]
        [InlineData("uint", "U")]
        public async Task TestUsingCastsInFieldDeclarationProducesDiagnosticAsync(string literalType, string literalSuffix)
        {
            var testCode = $@"
class ClassName
{{
    {literalType} x = ({literalType})1;
}}
";
            var fixedCode = $@"
class ClassName
{{
    {literalType} x = 1{literalSuffix};
}}
";
            DiagnosticResult[] expectedDiagnosticResult =
            {
                this.CSharpDiagnostic().WithLocation(4, 10 + literalType.Length)
            };
            await this.VerifyCSharpDiagnosticAsync(testCode, expectedDiagnosticResult, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAsync(testCode, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
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
        [InlineData("Ul")]
        [InlineData("uL")]
        [InlineData("ul")]
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
        /// <param name="literalSuffix">The suffix corresponding to the type</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData("long", "L")]
        [InlineData("ulong", "UL")]
        [InlineData("uint", "U")]
        public async Task TestUsingCastsInMethodProducesDiagnosticAsync(string literalType, string literalSuffix)
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

            var fixedCode = $@"
class ClassName
{{
    public void Method()
    {{
        var x = 1{literalSuffix};
    }}
}}
";
            DiagnosticResult[] expectedDiagnosticResult =
            {
                this.CSharpDiagnostic().WithLocation(6, 17)
            };
            await this.VerifyCSharpDiagnosticAsync(testCode, expectedDiagnosticResult, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAsync(testCode, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        [Theory]
        [InlineData("(ulong)1L", "1UL")]
        [InlineData("(ulong)1l", "1UL")]
        [InlineData("(ulong)1U", "1UL")]
        [InlineData("(ulong)1u", "1UL")]
        public async Task TestUsingCastsOnLiteralsWithSuffixInMethodProducesDiagnosticAsync(string wrongLiteralWithCast, string correctLiteral)
        {
            var testCode = $@"
class ClassName
{{
    public void Method()
    {{
        var x = {wrongLiteralWithCast};
    }}
}}
";

            var fixedCode = $@"
class ClassName
{{
    public void Method()
    {{
        var x = {correctLiteral};
    }}
}}
";
            DiagnosticResult[] expectedDiagnosticResult =
{
                this.CSharpDiagnostic().WithLocation(6, 17)
            };
            await this.VerifyCSharpDiagnosticAsync(testCode, expectedDiagnosticResult, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAsync(testCode, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new SA1139CodeFixProvider();
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new SA1139UseLiteralSuffixNotationInsteadOfCasting();
        }
    }
}
