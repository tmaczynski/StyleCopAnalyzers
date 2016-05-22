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
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using TestHelper;
    using Xunit;

    /// <summary>
    /// This class contains unit tests for SA1139.
    /// </summary>
    /// <seealso cref="SA1139UseLiteralSuffixNotationInsteadOfCasting" />
    /// <seealso cref="SA1139CodeFixProvider" />
    public class SA1139UnitTests : CodeFixVerifier
    {
        /// <summary>
        /// Verifies that using literals does not produce diagnostic.
        /// </summary>
        /// <param name="literalType">The type which is checked.</param>
        /// <param name="literalSuffix">The correpsonding literal's suffix.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData("int", "")]
        [InlineData("long", "L")]
        [InlineData("long", "l")]
        [InlineData("ulong", "UL")]
        [InlineData("ulong", "Ul")]
        [InlineData("ulong", "uL")]
        [InlineData("ulong", "ul")]
        [InlineData("uint", "U")]
        [InlineData("uint", "u")]
        [InlineData("float", "F")]
        [InlineData("float", "f")]
        [InlineData("double", "D")]
        [InlineData("double", "d")]
        [InlineData("decimal", "M")]
        [InlineData("decimal", "m")]
        public async Task TestUsingLiteralsAsClassFieldsDoesNotProduceDiagnosticAsync(string literalType, string literalSuffix)
        {
            var testCode = $@"
class ClassName
{{
    {literalType} x = 1{literalSuffix};

    public void Method()
    {{
        var x = 1{literalSuffix};
    }}
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
        [InlineData("float", "F")]
        [InlineData("double", "D")]
        [InlineData("decimal", "M")]
        public async Task TestUsingCastsInFieldDeclarationProducesDiagnosticAndCorrectCodefixAsync(string literalType, string literalSuffix)
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
        /// Verifies that using casts in a method body produces diagnostic.
        /// </summary>
        /// <param name="literalType">The type which is checked.</param>
        /// <param name="literalSuffix">The suffix corresponding to the type</param>
        /// <param name="sign">The sign of a number ("+", "-" or string.Empty)</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData("long", "L", "")]
        [InlineData("long", "L", "+")]
        [InlineData("long", "L", "-")]
        [InlineData("ulong", "UL", "")]
        [InlineData("ulong", "UL", "+")]
        [InlineData("uint", "U", "")]
        [InlineData("float", "F", "")]
        [InlineData("float", "F", "+")]
        [InlineData("float", "F", "-")]
        [InlineData("double", "D", "")]
        [InlineData("double", "D", "+")]
        [InlineData("double", "D", "-")]
        [InlineData("decimal", "M", "")]
        [InlineData("decimal", "M", "+")]
        [InlineData("decimal", "M", "-")]
        public async Task TestUsingCastsInMethodProducesDiagnosticAndCorrectCodefixAsync(string literalType, string literalSuffix, string sign)
        {
            var testCode = $@"
class ClassName
{{
    public void Method()
    {{
        var x = ({literalType}){sign}1;
    }}
}}
";

            var fixedCode = $@"
class ClassName
{{
    public void Method()
    {{
        var x = {sign}1{literalSuffix};
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
        [InlineData("1", "int")]
        [InlineData("1L", "long")]
        [InlineData("1l", "long")]
        [InlineData("1UL", "ulong")]
        [InlineData("1Ul", "ulong")]
        [InlineData("1uL", "ulong")]
        [InlineData("1ul", "ulong")]
        [InlineData("1U", "uint")]
        [InlineData("1u", "uint")]
        [InlineData("1F", "float")]
        [InlineData("1f", "float")]
        [InlineData("1D", "double")]
        [InlineData("1d", "double")]
        [InlineData("1M", "decimal")]
        [InlineData("1m", "decimal")]
        public async Task TestDoNotRaportDiagnositcOnRedundantCastAsync(string literal, string type)
        {
            var testCode = $@"
class ClassName
{{
    public void Method()
    {{
        var x = ({type}){literal};
    }}
}}
";
            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Verifies that using casts for a literal with a suffix produces diagnostics with a correct codefix.
        /// </summary>
        /// <param name="wrongLiteralWithCast">The literal with a suffix and a cast</param>
        /// <param name="correctLiteral">The corresponding literal with suffix</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData("(ulong)1L", "1UL")]
        [InlineData("(ulong)1l", "1UL")]
        [InlineData("(ulong)1U", "1UL")]
        [InlineData("(ulong)1u", "1UL")]
        public async Task TestUsingCastsOnLiteralsWithSuffixInMethodProducesDiagnosticAndCorrectCodefixAsync(string wrongLiteralWithCast, string correctLiteral)
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

        /// <summary>
        /// Verifies that other types of casts should not produces diagnostics.
        /// </summary>
        /// <param name="correctCastExpression">A legal cast that should not trigger diagnostic</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData("(long)~1")]
        [InlineData("(bool)true")]
        [InlineData("(bool)(false)")]
        [InlineData("(long)~+1")]
        public async Task TestOtherTypesOfCastsShouldNotTriggerDiagnosticAsync(string correctCastExpression)
        {
            var testCode = $@"
class ClassName
{{
    public void Method()
    {{
        var x = {correctCastExpression};
    }}
}}
";
            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Verifies that diagnostics is not produced when error CS0221 raported.
        /// </summary>
        /// <param name="type">A type that a literal is casted on</param>
        /// <param name="castedLiteral">A literal that is casted</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData("ulong", "-1")]
        public async Task TestCodeTriggeringCS0221ShouldNotTriggerDiagnosticAsync(string type, string castedLiteral)
        {
            var testCode = $@"
class ClassName
{{
    public void Method()
    {{
        var x = ({type}){castedLiteral};
    }}
}}
";

            DiagnosticResult[] expectedDiagnosticResult =
            {
                new DiagnosticResult()
                {
                    Id = "CS0221",
                    Severity = DiagnosticSeverity.Error,
                    Message = $"Constant value '{castedLiteral}' cannot be converted to a '{type}' (use 'unchecked' syntax to override)",
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 6, 17) }
                }
            };
            await this.VerifyCSharpDiagnosticAsync(testCode, expectedDiagnosticResult, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Verifies that using casts in unchecked enviroment produces diagnostics with a correct codefix.
        /// </summary>
        /// <param name="castExpression">A cast which can be performend in unchecked enviroment</param>
        /// <param name="correctLiteral">The corresponding literal with suffix</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData("(ulong)-1L", "18446744073709551615UL")]
        public async Task TestCastsInUncheckedEnviromentShouldPreserveValueAsync(string castExpression, string correctLiteral)
        {
            var testCode = $@"
class ClassName
{{
    public void Method()
    {{
        unchecked
        {{
            var x = {castExpression};
        }}
    }}
}}
";
            var fixedCode = $@"
class ClassName
{{
    public void Method()
    {{
        unchecked
        {{
            var x = {correctLiteral};
        }}
    }}
}}
";
            DiagnosticResult[] expectedDiagnosticResult =
            {
                this.CSharpDiagnostic().WithLocation(8, 21)
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
