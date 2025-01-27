using System.Text.Json.Serialization.Metadata;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using UnityExtended.Generator;

using UnityExtended.Generator.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;


internal class Program {
    public static void Main() {
        string mocking = File.ReadAllText(@"C:\Users\Artem\Documents\C#\UnityExtended.Generator\UseGenerator\Mocking\MockingUnity.cs");
        string source = File.ReadAllText(@"C:\Users\Artem\Documents\C#\UnityExtended.Generator\UseGenerator\Source.cs");

        SyntaxTree sourceSyntaxTree = CSharpSyntaxTree.ParseText(source);
        SyntaxTree mockingSyntaxTree = CSharpSyntaxTree.ParseText(mocking);
        CSharpCompilation compilation = CSharpCompilation.Create("UseGenerator", [sourceSyntaxTree, mockingSyntaxTree]);

        var generator = new UnityGenerator();
        
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);
    }
}



