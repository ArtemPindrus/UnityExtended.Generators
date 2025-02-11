using System.Collections.Generic;

namespace Hierarchy;

public class Method {
    public string Signature { get; }

    public List<string> Attributes = [];
    public List<string> Statements = [];
    
    public Method(string signature) {
        Signature = signature;
    }

    public void AddStatement(string statement) {
        string[] statements = statement.Split('\n');
        
        Statements.AddRange(statements);
    }

    public void AddAttribute(string attribute) {
        Attributes.Add(attribute);
    }

    public void Merge(Method other) {
        foreach (var attribute in other.Attributes) {
            Attributes.Add(attribute);
        }

        foreach (var statement in other.Statements) {
            Statements.Add(statement);
        }
    }
}