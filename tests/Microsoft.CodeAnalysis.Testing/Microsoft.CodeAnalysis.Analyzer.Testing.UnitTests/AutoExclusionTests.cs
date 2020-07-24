﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace Microsoft.CodeAnalysis.Testing
{
    public class AutoExclusionTests
    {
        private const string ReplaceThisWithBaseTestCode = @"
class TestClass {
  void TestMethod() { [|this|].Equals(null); }
}
";

        private const string CSharpFirstLineDiagnosticTestCode = @"[||]
class TestClass {
}
";

        private const string ReplaceMyClassWithMyBaseTestCode = @"
Class TestClass
  Sub TestMethod()
    [|MyClass|].Equals(Nothing)
  End Sub
End Class
";

        private const string VisualBasicFirstLineDiagnosticTestCode = @"[||]
Class TestClass
End Class
";

        [Fact]
        public async Task TestCSharpAnalyzerWithUnspecifiedExclusionFails()
        {
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await new CSharpReplaceThisWithBaseTest(generatedCodeAnalysisFlags: null)
                {
                    TestCode = ReplaceThisWithBaseTestCode,
                }.RunAsync();
            });

            var expected =
                "Context: Verifying exclusions in <auto-generated> code" + Environment.NewLine +
                "Mismatch between number of diagnostics returned, expected \"0\" actual \"1\"" + Environment.NewLine +
                Environment.NewLine +
                "Diagnostics:" + Environment.NewLine +
                "// /0/Test0.cs(4,23): warning ThisToBase: message" + Environment.NewLine +
                "VerifyCS.Diagnostic().WithSpan(4, 23, 4, 27)," + Environment.NewLine +
                Environment.NewLine;
            new DefaultVerifier().EqualOrDiff(expected, exception.Message);
        }

        [Fact]
        public async Task TestCSharpAnalyzerWithoutExclusionPasses()
        {
            await new CSharpReplaceThisWithBaseTest(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics)
            {
                TestCode = ReplaceThisWithBaseTestCode,
            }.RunAsync();
        }

        [Fact]
        public async Task TestCSharpAnalyzerWithoutSuppressionFails()
        {
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await new CSharpAnalyzerTest<FirstLineDiagnosticAnalyzer>
                {
                    TestCode = CSharpFirstLineDiagnosticTestCode,
                }.RunAsync();
            });

            var expected =
                "Context: Verifying exclusions in '#pragma warning disable' code" + Environment.NewLine +
                "Mismatch between number of diagnostics returned, expected \"0\" actual \"1\"" + Environment.NewLine +
                Environment.NewLine +
                "Diagnostics:" + Environment.NewLine +
                "// /0/Test0.cs(1,1): warning FirstLine: message" + Environment.NewLine +
                "VerifyCS.Diagnostic().WithSpan(1, 1, 1, 1)," + Environment.NewLine +
                Environment.NewLine;
            new DefaultVerifier().EqualOrDiff(expected, exception.Message);
        }

        [Fact]
        public async Task TestCSharpAnalyzerNotConfigurableWithoutSuppressionPasses()
        {
            await new CSharpAnalyzerTest<NotConfigurableReplaceThisWithBaseAnalyzer>
            {
                TestCode = ReplaceThisWithBaseTestCode,
            }.RunAsync();
        }

        [Theory]
        [InlineData(GeneratedCodeAnalysisFlags.None)]
        [InlineData(GeneratedCodeAnalysisFlags.Analyze)]
        public async Task TestCSharpAnalyzerWithExclusionPasses(GeneratedCodeAnalysisFlags generatedCodeAnalysisFlags)
        {
            await new CSharpReplaceThisWithBaseTest(generatedCodeAnalysisFlags)
            {
                TestCode = ReplaceThisWithBaseTestCode,
            }.RunAsync();
        }

        [Fact]
        public async Task TestCSharpAnalyzerWithoutGeneratedCodeExclusionButAllowedPasses()
        {
            await new CSharpReplaceThisWithBaseTest(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics)
            {
                TestCode = ReplaceThisWithBaseTestCode,
                TestBehaviors = TestBehaviors.SkipGeneratedCodeCheck,
            }.RunAsync();
        }

        [Fact]
        public async Task TestCSharpAnalyzerWithoutSuppressionExclusionButAllowedPasses()
        {
            await new CSharpAnalyzerTest<FirstLineDiagnosticAnalyzer>
            {
                TestCode = CSharpFirstLineDiagnosticTestCode,
                TestBehaviors = TestBehaviors.SkipSuppressionCheck,
            }.RunAsync();
        }

        [Fact]
        [WorkItem(159, "https://github.com/dotnet/roslyn-sdk/pull/159")]
        public async Task TestVisualBasicAnalyzerWithUnspecifiedExclusionFails()
        {
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await new VisualBasicReplaceThisWithBaseTest(generatedCodeAnalysisFlags: null)
                {
                    TestCode = ReplaceMyClassWithMyBaseTestCode,
                }.RunAsync();
            });

            var expected =
                "Context: Verifying exclusions in <auto-generated> code" + Environment.NewLine +
                "Mismatch between number of diagnostics returned, expected \"0\" actual \"1\"" + Environment.NewLine +
                Environment.NewLine +
                "Diagnostics:" + Environment.NewLine +
                "// /0/Test0.vb(5,5): warning ThisToBase: message" + Environment.NewLine +
                "VerifyVB.Diagnostic().WithSpan(5, 5, 5, 12)," + Environment.NewLine +
                Environment.NewLine;
            new DefaultVerifier().EqualOrDiff(expected, exception.Message);
        }

        [Fact]
        [WorkItem(159, "https://github.com/dotnet/roslyn-sdk/pull/159")]
        public async Task TestVisualBasicAnalyzerWithoutExclusionPasses()
        {
            await new VisualBasicReplaceThisWithBaseTest(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics)
            {
                TestCode = ReplaceMyClassWithMyBaseTestCode,
            }.RunAsync();
        }

        [Fact]
        public async Task TestVisualBasicAnalyzerWithoutSuppressionFails()
        {
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await new VisualBasicAnalyzerTest<FirstLineDiagnosticAnalyzer>
                {
                    TestCode = VisualBasicFirstLineDiagnosticTestCode,
                }.RunAsync();
            });

            var expected =
                "Context: Verifying exclusions in '#Disable Warning' code" + Environment.NewLine +
                "Mismatch between number of diagnostics returned, expected \"0\" actual \"1\"" + Environment.NewLine +
                Environment.NewLine +
                "Diagnostics:" + Environment.NewLine +
                "// /0/Test0.vb(1,1): warning FirstLine: message" + Environment.NewLine +
                "VerifyVB.Diagnostic().WithSpan(1, 1, 1, 1)," + Environment.NewLine +
                Environment.NewLine;
            new DefaultVerifier().EqualOrDiff(expected, exception.Message);
        }

        [Theory]
        [InlineData(GeneratedCodeAnalysisFlags.None)]
        [InlineData(GeneratedCodeAnalysisFlags.Analyze)]
        [WorkItem(159, "https://github.com/dotnet/roslyn-sdk/pull/159")]
        public async Task TestVisualBasicAnalyzerWithExclusionPasses(GeneratedCodeAnalysisFlags generatedCodeAnalysisFlags)
        {
            await new VisualBasicReplaceThisWithBaseTest(generatedCodeAnalysisFlags)
            {
                TestCode = ReplaceMyClassWithMyBaseTestCode,
            }.RunAsync();
        }

        [Fact]
        [WorkItem(159, "https://github.com/dotnet/roslyn-sdk/pull/159")]
        public async Task TestVisualBasicAnalyzerWithoutGeneratedCodeExclusionButAllowedPasses()
        {
            await new VisualBasicReplaceThisWithBaseTest(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics)
            {
                TestCode = ReplaceMyClassWithMyBaseTestCode,
                TestBehaviors = TestBehaviors.SkipGeneratedCodeCheck,
            }.RunAsync();
        }

        [Fact]
        public async Task TestVisualBasicAnalyzerWithoutSuppressionExclusionButAllowedPasses()
        {
            await new VisualBasicAnalyzerTest<FirstLineDiagnosticAnalyzer>
            {
                TestCode = VisualBasicFirstLineDiagnosticTestCode,
                TestBehaviors = TestBehaviors.SkipSuppressionCheck,
            }.RunAsync();
        }

        [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
        private class ReplaceThisWithBaseAnalyzer : DiagnosticAnalyzer
        {
            internal static readonly DiagnosticDescriptor Descriptor =
                new DiagnosticDescriptor("ThisToBase", "title", "message", "category", DiagnosticSeverity.Warning, isEnabledByDefault: true);

            private readonly GeneratedCodeAnalysisFlags? _generatedCodeAnalysisFlags;

            public ReplaceThisWithBaseAnalyzer(GeneratedCodeAnalysisFlags? generatedCodeAnalysisFlags)
            {
                _generatedCodeAnalysisFlags = generatedCodeAnalysisFlags;
            }

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

            public override void Initialize(AnalysisContext context)
            {
                context.EnableConcurrentExecution();
                if (_generatedCodeAnalysisFlags.HasValue)
                {
                    context.ConfigureGeneratedCodeAnalysis(_generatedCodeAnalysisFlags.Value);
                }

                context.RegisterSyntaxNodeAction(HandleThisExpression, CSharp.SyntaxKind.ThisExpression);
                context.RegisterSyntaxNodeAction(HandleMyClassExpression, VisualBasic.SyntaxKind.MyClassExpression);
            }

            private void HandleThisExpression(SyntaxNodeAnalysisContext context)
            {
                var node = (CSharp.Syntax.ThisExpressionSyntax)context.Node;
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, node.Token.GetLocation()));
            }

            private void HandleMyClassExpression(SyntaxNodeAnalysisContext context)
            {
                var node = (VisualBasic.Syntax.MyClassExpressionSyntax)context.Node;
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, node.Keyword.GetLocation()));
            }
        }

        [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
        private class NotConfigurableReplaceThisWithBaseAnalyzer : DiagnosticAnalyzer
        {
            internal static readonly DiagnosticDescriptor Descriptor =
                new DiagnosticDescriptor("ThisToBase", "title", "message", "category", DiagnosticSeverity.Warning, isEnabledByDefault: true, customTags: new[] { WellKnownDiagnosticTags.NotConfigurable });

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

            public override void Initialize(AnalysisContext context)
            {
                context.EnableConcurrentExecution();
                context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

                context.RegisterSyntaxNodeAction(HandleThisExpression, CSharp.SyntaxKind.ThisExpression);
                context.RegisterSyntaxNodeAction(HandleMyClassExpression, VisualBasic.SyntaxKind.MyClassExpression);
            }

            private void HandleThisExpression(SyntaxNodeAnalysisContext context)
            {
                var node = (CSharp.Syntax.ThisExpressionSyntax)context.Node;
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, node.Token.GetLocation()));
            }

            private void HandleMyClassExpression(SyntaxNodeAnalysisContext context)
            {
                var node = (VisualBasic.Syntax.MyClassExpressionSyntax)context.Node;
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, node.Keyword.GetLocation()));
            }
        }

        [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
        private class FirstLineDiagnosticAnalyzer : DiagnosticAnalyzer
        {
            internal static readonly DiagnosticDescriptor Descriptor =
                new DiagnosticDescriptor("FirstLine", "title", "message", "category", DiagnosticSeverity.Warning, isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

            public override void Initialize(AnalysisContext context)
            {
                context.EnableConcurrentExecution();
                context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

                context.RegisterSyntaxTreeAction(HandleSyntaxTree);
            }

            private void HandleSyntaxTree(SyntaxTreeAnalysisContext context)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptor,
                    Location.Create(context.Tree, new TextSpan(0, 0))));
            }
        }

        private class CSharpReplaceThisWithBaseTest : AnalyzerTest<DefaultVerifier>
        {
            private readonly GeneratedCodeAnalysisFlags? _generatedCodeAnalysisFlags;

            public CSharpReplaceThisWithBaseTest(GeneratedCodeAnalysisFlags? generatedCodeAnalysisFlags)
            {
                _generatedCodeAnalysisFlags = generatedCodeAnalysisFlags;
            }

            public override string Language => LanguageNames.CSharp;

            protected override string DefaultFileExt => "cs";

            protected override CompilationOptions CreateCompilationOptions()
            {
                return new CSharp.CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            }

            protected override ParseOptions CreateParseOptions()
            {
                return new CSharp.CSharpParseOptions(CSharp.LanguageVersion.Default, DocumentationMode.Diagnose);
            }

            protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
            {
                yield return new ReplaceThisWithBaseAnalyzer(_generatedCodeAnalysisFlags);
            }
        }

        private class VisualBasicReplaceThisWithBaseTest : AnalyzerTest<DefaultVerifier>
        {
            private readonly GeneratedCodeAnalysisFlags? _generatedCodeAnalysisFlags;

            public VisualBasicReplaceThisWithBaseTest(GeneratedCodeAnalysisFlags? generatedCodeAnalysisFlags)
            {
                _generatedCodeAnalysisFlags = generatedCodeAnalysisFlags;
            }

            public override string Language => LanguageNames.VisualBasic;

            protected override string DefaultFileExt => "vb";

            protected override CompilationOptions CreateCompilationOptions()
            {
                return new VisualBasic.VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            }

            protected override ParseOptions CreateParseOptions()
            {
                return new VisualBasic.VisualBasicParseOptions(VisualBasic.LanguageVersion.Default, DocumentationMode.Diagnose);
            }

            protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
            {
                yield return new ReplaceThisWithBaseAnalyzer(_generatedCodeAnalysisFlags);
            }
        }
    }
}
