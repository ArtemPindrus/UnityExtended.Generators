using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Hierarchy;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using UnityExtended.Generator.ClassFillers;
using UnityExtended.Generator.Helpers;

namespace UnityExtended.Generator;

[Generator]
public class UnityGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var getComponentProvider = context.SyntaxProvider.ForAttributeWithMetadataName(AttributesHelper.GetComponentAttribute,
            predicate: static (_, _) => true,
            transform: ClassContextFactory.GetComponentFiller)
            .Collect();
        
        context.RegisterSourceOutput(getComponentProvider, Execute);
    }
    
    private static void Execute(SourceProductionContext context, ImmutableArray<Class> classes) {
        var uniqueClasses = classes.Distinct(ClassSignatureEqualityComparer.Default).ToArray();

        foreach (var c in uniqueClasses) {
            c.Finish();
        }
        
        context.AddSource(uniqueClasses);
        
        // TODO: report diagnostics
    }
}