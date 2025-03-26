using System;
using UnityExtended.Generators;
using Xunit;

namespace UnityExtended.Generators.Tests;

public class EquatableListTest {

    [Fact]
    public void EqualityCheckIsValid() {
        string[] a = ["a", "b", "c"];

        EquatableList<string> ae = new(a);
        EquatableList<string> ae2 = new(a);
        
        Assert.True(ae == ae2);

        ae2[0] = "s";
        
        Assert.True(ae != ae2);
    }
}