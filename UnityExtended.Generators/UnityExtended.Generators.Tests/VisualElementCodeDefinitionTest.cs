using UnityExtended.Generator;
using Xunit;

namespace UnityExtended.Generators.Tests;

public class VisualElementCodeDefinitionTest {
    [Fact]
    public void GenericTest() {
        VisualElementCodeDefinition root = new("root") {
            IsField = true
        };
        var x = new VisualElementCodeDefinition("x", "float");
        var y = new VisualElementCodeDefinition("y", "float");
        var composite = new VisualElementCodeDefinition("comp", "CompositeType") {
                IsField = true,
                ObjectInitializer = """
                                    isEnabled = false,
                                    someProperty = true
                                    """
            }
            .AddChildren(
                new VisualElementCodeDefinition("2", "float") { Order = 2 },
                new VisualElementCodeDefinition("1", "int") { Order = 1 });
        
        root.AddChildren(x, y, composite);

        string res = root.ToString();
        var fields = root.GetHierarchyFields();
    }
}