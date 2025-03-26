using UnityExtended.Generators.FillerData;
using Xunit;

namespace UnityExtended.Generators.Tests.FillerData;

public class GetComponentFillerDataTest {
    [Fact]
    public void EqualityIsValid() {
        string commonFQCN = "class";
        string commonFN = "field";
        string commonFQTN = "type";
        In commonIn = In.Children;
        bool commonPl = false;
        
        GetComponentFillerData d = new(commonFQCN, commonFN, commonFQTN, commonIn, commonPl);
        GetComponentFillerData d1 = new(commonFQCN, commonFN, commonFQTN, commonIn, commonPl);
        GetComponentFillerData d2 = new("otherclass", commonFN, commonFQTN, commonIn, commonPl);
        
        Assert.True(d == d1);
        Assert.True(d != d2 && d1 != d2);
    }
}