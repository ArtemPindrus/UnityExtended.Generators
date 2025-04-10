using UnityExtended.Generator;
using UnityExtended.Generators.FillerData;
using UnityExtended.Generators.Helpers;
using UnityExtended.Generators.Hierarchy;

namespace UnityExtended.Generators.ClassFillers;

public class CreateCustomInspectorFiller : IClassFiller<CreateCustomInspectorFillerData, Class> {
    private const string EditorTargetFieldName = "targetCasted";
    private const string UnityEditorConstraint = "#if UNITY_EDITOR";
    private const string ModifyRootSignature = "partial void ModifyRoot(ref VisualElement root)";
    private const string CustomEditorUsings = """
                                              UnityEngine
                                              UnityEngine.UIElements
                                              UnityEditor.UIElements
                                              System
                                              System.Reflection
                                              System.Collections
                                              System.Collections.Generic
                                              UnityExtended.Core.Extensions
                                              UnityExtended.Core.Editor.Drawers
                                              """;

    
    public Class Fill(Class c, CreateCustomInspectorFillerData data) {
        c.AddImplementation("UnityEditor.Editor")
            .AddAttribute($"[UnityEditor.CustomEditor(typeof({data.BaseClassName}))]") // UnityEditor.CanEditMultipleObjects
            .AddFields($"private {data.BaseClassName} {EditorTargetFieldName};")
            .AddConstraint(UnityEditorConstraint)
            .AddUsings(CustomEditorUsings);
        
        c.GetOrCreateMethod(ModifyRootSignature);
        c.GetOrCreateMethod("partial void PreRearrange(VisualElement root)");
        
        CreateCreateGUI(c, data);

        return c;
    }

    private void CreateCreateGUI(Class c, CreateCustomInspectorFillerData data) {
        var m = c.GetOrCreateMethod(GeneratorHelper.CreateInspectorGUISignature);
        m.AddStatements($"""
                         {EditorTargetFieldName} = ({data.BaseClassName})target;
                         var root = new VisualElement();

                         InspectorElement.FillDefaultInspector(root, serializedObject, this);
                         
                         DisplayDrawer.FillInFor(root, target);
                         ButtonDrawer.FillIn(root, target);
                         
                         PreRearrange(root);
                         
                         SetVisualElementAtDrawer.Rearrange(root, target);
                         
                         ModifyRoot(ref root);
                         
                         return root;
                         """);
    }
}