using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace UnityExtended.Generators.Hierarchy;

public class Method : HierarchyMember {
    public string Signature { get; }

    public List<string> Attributes {
        get {
            if (attributes == null) attributes = new();

            return attributes;
        }
    }
    
    public Dictionary<string, Reservation> ReservationsByID {
        get {
            if (reservationsByID == null) reservationsByID = new();

            return reservationsByID;
        }
    }

    public Reservation MainReservation { get; } = new("Main");

    private List<string>? attributes;
    private Dictionary<string,Reservation>? reservationsByID;
    
    public Method(string signature) {
        Signature = signature;
    }

    public Reservation GetOrCreateReservation(string id) {
        GetOrCreateReservation(id, out var res);

        return res;
    }

    public bool GetOrCreateReservation(string id, out Reservation reservation) {
        if (!ReservationsByID.TryGetValue(id, out reservation)) {
            reservation = new(id);
            ReservationsByID[id] = reservation;
            
            return false;
        }
        
        return true;
    }

    public Method InsertStatement(string statement, int index) {
        MainReservation.InsertStatement(statement, index);
        
        return this;
    }

    public Method AddStatement(string statement) {
        MainReservation.AddStatement(statement);

        return this;
    }

    public Method AddStatements(string newStatements) {
        MainReservation.AddStatements(newStatements);

        return this;
    }

    public Method AddAttribute(string attribute) {
        Attributes.Add(attribute);

        return this;
    }

    public void Merge(Method other) {
        foreach (var attribute in other.Attributes) {
            Attributes.Add(attribute);
        }

        foreach (var statement in MainReservation.Statements) {
            MainReservation.AddStatement(statement);
        }
    }

    public override void AppendTo(IndentedStringBuilder sb) {
        foreach (var attribute in Attributes) {
            sb.AppendLine(attribute);
        }

        if (Signature.Contains("partial ") && MainReservation.Statements.Count == 0 && reservationsByID == null) {
            sb.AppendLine($"{Signature};");
        } else {
            sb.AppendLine($"{Signature} {{");

            sb.IncrementIndent();
            MainReservation.AppendTo(sb);

            if (reservationsByID != null) {
                sb.AppendLine();
                
                var stats = ReservationsByID.Values.ToArray();
                int length = stats.Length;
                
                for (int i = 0; i < length; i++) {
                    stats[i].AppendTo(sb);

                    if (i != length - 1) sb.AppendLine();
                }
            }

            sb.DecrementIndent().AppendLine("}");
        }
    }

    public override string ToString() {
        IndentedStringBuilder sb = new();
        AppendTo(sb);

        return sb.ToString();
    }
}