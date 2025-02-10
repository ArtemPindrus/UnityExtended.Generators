using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using UnityExtended.Generator;

internal class Program {
    public static void Main() {
        string mocking = File.ReadAllText(@"C:\Users\Artem\Documents\C#\UnityExtended.Generator\UseGenerator\Mocking\MockingUnity.cs");
        string source = File.ReadAllText(@"C:\Users\Artem\Documents\C#\UnityExtended.Generator\UseGenerator\Source.cs");

        SyntaxTree sourceSyntaxTree = CSharpSyntaxTree.ParseText(source);
        SyntaxTree mockingSyntaxTree = CSharpSyntaxTree.ParseText(mocking);
        
        CSharpCompilation compilation = CSharpCompilation
            .Create("UseGenerator", [sourceSyntaxTree, mockingSyntaxTree])
            .AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location));

        var generator = new UnityGenerator();
        
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGenerators(compilation);
    }
}



