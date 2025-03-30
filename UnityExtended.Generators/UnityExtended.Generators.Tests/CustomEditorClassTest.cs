using UnityExtended.Generator;
using Xunit;

namespace UnityExtended.Generators.Tests;

public class CustomEditorClassTest {
    [Fact]
    public void GenericTesting() {
        var customEditorClass = CustomEditorClass.GetOrCreate("SomeClass");
        
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

        customEditorClass.AddVisualElementsToRoot(x, y, composite);

        customEditorClass.Finish();

        string res = customEditorClass.ToString();
    }
}