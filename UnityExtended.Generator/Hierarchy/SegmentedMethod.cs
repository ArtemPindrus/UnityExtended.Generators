using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Hierarchy;

public class SegmentedMethod {
    private readonly string signature;
    private readonly Dictionary<int, List<string>> segments = new();

    public SegmentedMethod(string signature) {
        this.signature = signature;
    }

    public Method ConnectSegments() {
        Method m = new(signature);
        
        var ordered = segments.OrderBy(x => x.Key);
        
        // TODO: broky broky
        // foreach (var segment in ordered) {
        //     m.AddStatements(segment.Value);
        // }

        return m;
    }

    public SegmentedMethod Merge(SegmentedMethod method, HashSet<int>? ignoredSegments = null) {
        foreach (var segment in segments) {
            if (ignoredSegments != null && ignoredSegments.Contains(segment.Key)) continue;
            
            if (method.segments.TryGetValue(segment.Key, out var otherSegmentStatements)) {
                segment.Value.AddRange(otherSegmentStatements);
            }
        }

        foreach (var otherSegment in method.segments) {
            if (ignoredSegments != null && ignoredSegments.Contains(otherSegment.Key)) continue;
            
            if (!segments.ContainsKey(otherSegment.Key)) {
                List<string> statementsCopy = new(otherSegment.Value);
                segments[otherSegment.Key] = statementsCopy;
            }
        }

        return this;
    }
    
    public SegmentedMethod AddStatements(int segmentID, string statements) {
        if (!segments.TryGetValue(segmentID, out List<string> segmentStatements)) {
            segmentStatements = new();
            segments[segmentID] = segmentStatements;
        }
        
        var split = statements.Split('\n').Select(x => x.TrimEnd());
        
        segmentStatements.AddRange(split);

        return this;
    }
}