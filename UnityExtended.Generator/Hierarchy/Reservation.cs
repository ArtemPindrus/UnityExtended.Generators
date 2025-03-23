using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Hierarchy;

public class Reservation : HierarchyMember {
    public IEnumerable<string> Statements => statements;
    public string ID { get; }
    
    private List<string> statements = new();

    public Reservation(string id) {
        ID = id;
    }
    
    public Reservation InsertStatement(string statement, int index) {
        statements.Insert(index, statement);

        return this;
    }

    public Reservation AddStatement(string statement) {
        statements.Add(statement);

        return this;
    }

    public Reservation AddStatements(string newStatements) {
        string[] split = newStatements.Split('\n');

        foreach (var st in split) {
            statements.Add(st.Trim());
        }

        return this;
    }
    
    public Reservation AddStatements(IEnumerable<string> newStatements) {
        statements.AddRange(newStatements);

        return this;
    }

    public override void AppendTo(IndentedStringBuilder sb) {
        sb.AppendLine($"// Reservation {ID}");
        
        foreach (var s in statements) {
            sb.AppendLine(s);
        }
    }
}