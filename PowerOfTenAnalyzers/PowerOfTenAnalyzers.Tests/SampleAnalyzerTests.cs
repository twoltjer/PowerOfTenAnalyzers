using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace PowerOfTenAnalyzers.Tests;

public class SampleAnalyzerTests<TAnalyzer> where TAnalyzer : DiagnosticAnalyzer, new()
{
    private readonly string _sampleClassCode;
    private readonly string? _secondFileCode;

    protected SampleAnalyzerTests(string filename, string? secondFilename = null)
    {
        var diagnosticId = GetDiagnosticId(typeof(TAnalyzer));
        var relativePath = $"../../../../../PowerOfTenAnalyzers.Sample/{diagnosticId}/{filename}";
        var runningAssemblyLocation = Assembly.GetExecutingAssembly().Location;
        var path = Path.Combine(runningAssemblyLocation, relativePath);
        _sampleClassCode = File.ReadAllText(path);
        if (secondFilename != null)
        {
            relativePath = $"../../../../../PowerOfTenAnalyzers.Sample/{diagnosticId}/{secondFilename}";
            path = Path.Combine(runningAssemblyLocation, relativePath);
            _secondFileCode = File.ReadAllText(path);
        }
    }

    private static string GetDiagnosticId(Type? type)
    {
        while (type != null)
        {
            var fieldInfo = type.GetField("DiagnosticId", BindingFlags.Public | BindingFlags.Static);
            if (fieldInfo != null)
            {
                return (string)fieldInfo.GetValue(null)!;
            }

            type = type.BaseType;
        }

        throw new ArgumentException("Expected analyzer type or a base class to have a public static DiagnosticId field");
    }

    protected async Task VerifyDiagnostics(IList<DiagnosticResult> expected) 
    {
        var test = new CSharpAnalyzerTest<TAnalyzer, XUnitVerifier>();
        test.TestState.Sources.Add(_sampleClassCode);
        if (_secondFileCode != null)
        {
            test.TestState.Sources.Add(_secondFileCode);
        }
        test.ExpectedDiagnostics.AddRange(expected);
        await test.RunAsync();
    }

    protected Task VerifyDiagnostics(DiagnosticResult expected)
    {
        return VerifyDiagnostics([ expected ]);
    }
}