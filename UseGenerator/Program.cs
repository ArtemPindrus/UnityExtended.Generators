using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using UnityExtended.Generator;

internal class Program {
    public static void Main() {
        string mocking = File.ReadAllText(@"C:\Users\Artem\Documents\C#\UnityExtended.Generator\UseGenerator\Mocking\MockingUnity.cs");
        string attributes = File.ReadAllText(@"C:\Users\Artem\Documents\C#\UnityExtended.Generator\UseGenerator\Mocking\ExtendedAttributes.cs");
        string source = File.ReadAllText(@"C:\Users\Artem\Documents\C#\UnityExtended.Generator\UseGenerator\Source.cs");

        SyntaxTree sourceSyntaxTree = CSharpSyntaxTree.ParseText(source);
        SyntaxTree mockingSyntaxTree = CSharpSyntaxTree.ParseText(mocking);
        SyntaxTree attributesSyntaxTree = CSharpSyntaxTree.ParseText(attributes);

        var references = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

        CSharpCompilation compilation = CSharpCompilation
            .Create("UseGenerator", [mockingSyntaxTree, attributesSyntaxTree, sourceSyntaxTree])
            .AddReferences(
                references
            );

        var generator = new UnityGenerator();
        
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        var d = driver.RunGenerators(compilation);
    }
}



