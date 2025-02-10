using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Hierarchy;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace UnityExtended.Generator;

[Generator]
public class UnityGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var getComponentProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            "UnityExtended.Generator.Attributes.GetComponentAttribute",
            static (_,_) => true, GetComponentAttributeData.TransformIntoIGenerate)
            .Where(x => x is not null)
            .Collect();
        
        var serializePropertyProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            "UnityExtended.Generator.Attributes.SerializePropertyWithBackingAttribute",
            static (_,_) => true, SerializePropertyWithBackingAttributeData.TransformIntoIGenerate)
            .Where(x => x is not null)
            .Collect();
        
        var handleInputProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            "UnityExtended.Generator.Attributes.HandleInputAttribute",
            static (_,_) => true, HandleInputAttributeData.TransformToIGenerate)
            .Collect();

        var provider = getComponentProvider.Combine(handleInputProvider).Combine(serializePropertyProvider).Select(Selector);
        
        context.RegisterSourceOutput(provider, Execute);

    }

    private IEnumerable<IGenerate> Selector(((ImmutableArray<IGenerate?> arrayL, ImmutableArray<IEnumerable<IGenerate>> arrayR) tuple, ImmutableArray<IGenerate?> array) tuple, CancellationToken _) {
        var first = tuple.array;
        (var second, var third) = tuple.tuple;

        foreach (var ig in first) {
            if (ig is not null) yield return ig;
        }

        foreach (var ig in second) {
            if (ig is not null) yield return ig;
        }

        foreach (var enumerable in third) {
            foreach (var ig in enumerable) {
                if (ig is not null) yield return ig;
            }
        }
    }

    private static IEnumerable<IGenerate> Selector((ImmutableArray<IGenerate?>, ImmutableArray<IEnumerable<IGenerate>>) tuple, CancellationToken _) {
        (ImmutableArray<IGenerate?> left, ImmutableArray<IEnumerable<IGenerate>> right) = tuple;

        foreach (var iGenerate in left) {
            if (iGenerate is null) continue;
            yield return iGenerate;
        }

        foreach (var enumerable in right) {
            foreach (var iGenerate in enumerable) {
                yield return iGenerate;
            }
        }
    }

    private static void Execute(SourceProductionContext context, IEnumerable<IGenerate> requiredGeneratedData) {
        ExtractGeneratedClassesFromData(requiredGeneratedData, out Dictionary<string, Class> classesToGenerate); // hashed by FQName

        AddSecondMethodsIfNecessary(classesToGenerate.Values);
        
        foreach (var classToGenerate in classesToGenerate.Values) {
            IndentedStringBuilder stringBuilder = new();

            // open Namespace
            if (classToGenerate.NamespaceName is { } namespaceName)
                stringBuilder.AppendLine($"namespace {namespaceName} {{").IncrementIndent();

            // open Class
            stringBuilder.AppendLine($"partial class {classToGenerate.ClassName} {{").IncrementIndent();

            //// fields
            foreach (var fieldDeclaration in classToGenerate.Fields) {
                stringBuilder.AppendLine(fieldDeclaration);
            }

            stringBuilder.AppendLine();

            //// methods
            foreach (var method in classToGenerate.Methods) {
                if (method.MethodSignature.Contains("partial")) {
                    stringBuilder.AppendLine($"{method.MethodSignature};").AppendLine();
                }
                else {
                    stringBuilder.AppendLine($"{method.MethodSignature} {{").IncrementIndent();
                
                    foreach (var statement in method.Statements) {
                        stringBuilder.AppendLine(statement);
                    }

                    stringBuilder.DecrementIndent().AppendLine("}").AppendLine();
                }
            }

            stringBuilder.DecrementIndent().AppendLine("}"); 
            // close Class

            if (classToGenerate.NamespaceName is not null)
                stringBuilder.DecrementIndent().AppendLine("}");
            // close Namespace
            
            context.AddSource($"{classToGenerate.FullyQualifiedClassName}.g.cs", stringBuilder.ToString());
        }
    }

    private static void ExtractGeneratedClassesFromData(IEnumerable<IGenerate> requiredGeneratedData,
        out Dictionary<string, Class> classesToGenerate) {
        classesToGenerate = new();
        
        foreach (var generateData in requiredGeneratedData) {
            if (generateData is IGenerateClass classGenerator) {
                if (!classesToGenerate.ContainsKey(classGenerator.GeneratedClass.FullyQualifiedClassName)) {
                    classesToGenerate.Add(classGenerator.GeneratedClass.FullyQualifiedClassName,
                        classGenerator.GeneratedClass);
                }
            }
            if (generateData is IGenerateMethods methodsGenerator) {
                if (classesToGenerate.TryGetValue(methodsGenerator.GeneratedClass.FullyQualifiedClassName, out Class existingClass)) {
                    existingClass.TryAddMethods(methodsGenerator.Methods);
                }
                else {
                    classesToGenerate.Add(methodsGenerator.GeneratedClass.FullyQualifiedClassName, methodsGenerator.GeneratedClass);
                    methodsGenerator.GeneratedClass.TryAddMethods(methodsGenerator.Methods);
                }
            }
            if (generateData is IGenerateMethod methodGenerator) {
                if (classesToGenerate.TryGetValue(methodGenerator.GeneratedClass.FullyQualifiedClassName, out Class existingClass)) {
                    existingClass.TryAddMethod(methodGenerator.Method);
                }
                else {
                    classesToGenerate.Add(methodGenerator.GeneratedClass.FullyQualifiedClassName, methodGenerator.GeneratedClass);
                    methodGenerator.GeneratedClass.TryAddMethod(methodGenerator.Method);
                }
            }
            if (generateData is IGenerateStatements generateStatements) {
                if (!classesToGenerate.TryGetValue(generateStatements.GeneratedClass.FullyQualifiedClassName, out Class existingClass)) {
                    classesToGenerate.Add(generateStatements.GeneratedClass.FullyQualifiedClassName, generateStatements.GeneratedClass);
                    existingClass = generateStatements.GeneratedClass;
                }
                
                foreach (var statement in generateStatements.Statements) {
                    existingClass.GetOrAddMethod(statement.TargetMethod).TryAddStatement(statement);
                }
            }
            if (generateData is IGenerateStatement generateStatement) {
                if (!classesToGenerate.TryGetValue(generateStatement.GeneratedClass.FullyQualifiedClassName, out Class existingClass)) {
                    classesToGenerate.Add(generateStatement.GeneratedClass.FullyQualifiedClassName, generateStatement.GeneratedClass);
                    existingClass = generateStatement.GeneratedClass;
                }
                
                existingClass.GetOrAddMethod(generateStatement.Method).TryAddStatement(generateStatement.Statement);
            }

            if (generateData is IGenerateField generateField) {
                if (!classesToGenerate.TryGetValue(generateField.GeneratedClass.FullyQualifiedClassName, out Class existingClass)) {
                    classesToGenerate.Add(generateField.GeneratedClass.FullyQualifiedClassName, generateField.GeneratedClass);
                    existingClass = generateField.GeneratedClass;
                }

                existingClass.TryAddField(generateField.FieldDeclaration);
            }
        }
    }

    private static void AddSecondMethodsIfNecessary(IEnumerable<Class> classesToGenerate) {
        foreach (var classToGenerate in classesToGenerate) {
            Add(classToGenerate, GeneratorHelper.AwakeMethodSignature, GeneratorHelper.Awake2MethodSignature);
            
            Add(classToGenerate, GeneratorHelper.OnEnableMethodSignature, GeneratorHelper.OnEnable2MethodSignature);
            
            Add(classToGenerate, GeneratorHelper.OnDisableMethodSignature, GeneratorHelper.OnDisable2MethodSignature);
            
            Add(classToGenerate, GeneratorHelper.OnValidateMethodSignature, GeneratorHelper.OnValidate2MethodSignature);
        }

        static void Add(Class classToGenerate, string existingSignature, string generatedSignature) {
            Method existingMethod =
                classToGenerate.Methods.FirstOrDefault(x => x.MethodSignature == existingSignature);
            
            if (existingMethod != default) {
                var generateMethod = new Method(generatedSignature,
                    classToGenerate.FullyQualifiedClassName);
                classToGenerate.TryAddMethod(generateMethod);

                var calling = generatedSignature.Substring(generatedSignature.LastIndexOf(' ') + 1) + ";";

                existingMethod.TryAddStatement(new StatementDeclaration(calling, existingMethod));
            }
        }
    }
}