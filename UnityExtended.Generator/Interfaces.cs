using System.Collections.Generic;

namespace UnityExtended.Generator;

public interface IGenerate{}

public interface IGenerateClass : IGenerate {
    public Class GeneratedClass { get; }
}

public interface IGenerateMethod : IGenerateClass {
    /// <summary>
    /// Example: private void Awake()
    /// </summary>
    public Method Method { get; }
}

public interface IGenerateMethods : IGenerateClass {
    public List<Method> Methods { get; }
}

public interface IGenerateField : IGenerateClass {
    /// <summary>
    /// Example: public string some;
    /// </summary>
    public string FieldDeclaration { get; }
}

public interface IGenerateStatement : IGenerateMethod {
    /// <summary>
    /// Example: some = this;
    /// </summary>
    public StatementDeclaration Statement { get; }
}

public record struct StatementDeclaration {
    /// <summary>
    /// Example: private void Awake
    /// </summary>
    public readonly string Statement;
    public readonly Method TargetMethod;

    public StatementDeclaration(string statement, Method targetMethod) {
        Statement = statement;
        TargetMethod = targetMethod;
    }

    public bool ShouldGenerateForMethod(Method method) => method.Qualifier == TargetMethod.Qualifier;
}

public interface IGenerateStatements : IGenerateClass {
    /// <summary>
    /// Example: some = this;
    /// </summary>
    public List<StatementDeclaration> Statements { get; }
}