using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Hierarchy;

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

        if (Signature.Contains("partial ")) {
            sb.AppendLine($"{Signature};");
        } else {
            sb.AppendLine($"{Signature} {{");

            sb.IncrementIndent();
            MainReservation.AppendTo(sb);

            if (reservationsByID != null) sb.AppendLine();

            var lastRes = ReservationsByID.Values.Last();
            foreach (var reservation in reservationsByID.Values) {
                reservation.AppendTo(sb);

                if (reservation != lastRes) sb.AppendLine();
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