using UnityExtended.Generator;
using UnityExtended.Generators.FillerData;

namespace UnityExtended.Generators.ClassFillers;

public class CreateCustomInspectorFiller : IClassFiller<CreateCustomInspectorFillerData, CustomEditorClass> {
    public CustomEditorClass Fill(CustomEditorClass c, CreateCustomInspectorFillerData data) {
        c.GetOrCreateMethod("partial void PreRearrange(ref VisualElement root)");
        
        c.AddCreateGUIStatements("""
                                 DisplayDrawer.FillInFor(root, target);
                                 ButtonDrawer.FillIn(root, target);

                                 PreRearrange(ref root);
                                 SetVisualElementAtDrawer.Rearrange(root, target);
                                 """);

        return c;
    }
}