using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using VerifyCS = LineLength.Test
    .CSharpAnalyzerVerifier<LineLength.LineLengthAnalyzer>;

namespace LineLength.Test
{
    [TestClass]
    public class LineLengthUnitTests
    {
        public class DynamicDataItem
        {
            private string code;

            public DynamicDataItem(
                string testCaseName,
                string code,
                params DiagnosticResult[] expectedResults)
            {
                if (string.IsNullOrWhiteSpace(testCaseName))
                {
                    throw new ArgumentException(
                        $"'{nameof(testCaseName)}' cannot be null " +
                        $"or whitespace.",
                        nameof(testCaseName));
                }

                TestCaseName = testCaseName;
                Code = code;
                ExpectedResults.AddRange(expectedResults);
            }

            public string TestCaseName { get; }

            public string Code
            {
                get => code;
                set => code = value ?? string.Empty;
            }


            public List<DiagnosticResult> ExpectedResults { get; }
                = new List<DiagnosticResult>();


            public static implicit operator object[](DynamicDataItem item)
            {
                var rv = new object[]
                {
                    item.Code,
                    item.ExpectedResults
                };

                return rv;
            }
        }


        public static string GetTestCaseName(MethodInfo methodInfo, object[] objects)
        {
            var item = objects[0] as DynamicDataItem;

            return item.TestCaseName;
        }


        //No diagnostics expected to show up
        [DataTestMethod()]
        [DynamicData(nameof(DynamicData),
            DynamicDataSourceType.Method,
            DynamicDataDisplayName = nameof(GetTestCaseName))]
        public async Task LineLengthCheckTest(
            DynamicDataItem item)
        ////string code,
        ////params DiagnosticResult[] expectedResults)
        {
            await VerifyCS.VerifyAnalyzerAsync(
                item.Code,
                item.ExpectedResults.ToArray());
        }


        public static IEnumerable<object[]> DynamicData()
        {
            const string Comment80 =
                "//23456789qwertyuiop" +
                "0123456789qwertyuiop" +
                "0123456789qwertyuiop" +
                "0123456789qwertyuiop";



            yield return new object[] {
                new DynamicDataItem("No code", "")
            };
            yield return new object[] {
                new DynamicDataItem("81 chars",
                    Comment80 + "=",
                    new DiagnosticResult(LineLengthAnalyzer.LinesLengthRule)
                        .WithLocation(1,1))
            };
            yield return new object[]
            {
                new DynamicDataItem("81+80+81 chars",
                @$"{Comment80}1
{Comment80}
{Comment80}2",
                    new DiagnosticResult(LineLengthAnalyzer.LinesLengthRule)
                        .WithLocation(1,1),
                    new DiagnosticResult(LineLengthAnalyzer.LinesLengthRule)
                        .WithLocation(3,1))
            };
        }

    }
}
