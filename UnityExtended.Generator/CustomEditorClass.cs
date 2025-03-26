using System.Collections.Generic;
using System.Linq;
using Hierarchy;
using Microsoft.CodeAnalysis;

namespace UnityExtended.Generator;

public class CustomEditorClass : Class {
    public const string EditorTargetFieldName = "targetCasted";
    public const string CreateInspectorGUISignature = "public override VisualElement CreateInspectorGUI()";
    public const string UnityEditorConstraint = "#if UNITY_EDITOR";
    public const string ModifyRootSignature = "partial void ModifyRoot(ref VisualElement root)";
    public const string CustomEditorUsings = """
                                             UnityEngine
                                             UnityEngine.UIElements
                                             UnityEditor.UIElements
                                             System
                                             System.Reflection
                                             System.Collections
                                             System.Collections.Generic
                                             UnityExtended.Core.Extensions
                                             UnityExtended.Core.EditorTools.Drawers
                                             UnityExtended.Core.EditorTools.Extensions
                                             """;

    public const string NamePostFix = "Inspector";

    private const int InitializationSegmentIndex = 0;
    private const int UniqueStatementsSegmentIndex = 1;
    private const int FinishSegmentIndex = 2;

    private readonly VisualElementField rootVisualElementField = VisualElementField.CreateRoot();
    public readonly string BaseClassName;
    
    private readonly SegmentedMethod createGUI;
    private readonly SegmentedMethod update;
    private readonly Method modifyRoot;
    private readonly Method update2;
    
    /// <summary>
    /// Creates class with scaffolding for Custom Inspector.
    /// </summary>
    /// <param name="baseClassName">Name of a class for which to create Custom Inspector. e.g. for LookAround generates LookAroundInspector.</param>
    /// <param name="fullyQualifiedName">Name of the generated class.</param>
    private CustomEditorClass(string baseClassName, string fullyQualifiedName) : base(fullyQualifiedName) {
        BaseClassName = baseClassName;
        
        AddImplementation("UnityEditor.Editor");
        AddAttribute($"[UnityEditor.CustomEditor(typeof({baseClassName})), UnityEditor.CanEditMultipleObjects]");
        AddFields($"private {baseClassName} {EditorTargetFieldName};");
        AddConstraint(UnityEditorConstraint);
        AddUsings(CustomEditorUsings);
        
        createGUI = CreateBareCreateGUI();
        update = CreateBareUpdateForEditor();
        modifyRoot = new(ModifyRootSignature);
        update2 = new(GeneratorHelper.Update2Signature);
    }
    
    /// <summary>
    /// Creates class with scaffolding for Custom Inspector.
    /// </summary>
    /// <param name="baseClassName">Name of a class for which to create Custom Inspector. e.g. for LookAround generates LookAroundInspector.</param>
    public static CustomEditorClass GetFor(string baseClassName) {
        string fqName = baseClassName + NamePostFix;

        if (!GetCachedClass(fqName, out var c)) {
            c = new CustomEditorClass(baseClassName, fqName);
            CacheClass(c);
        }

        return (CustomEditorClass)c;
    }

    /// <summary>
    /// Finishes CustomEditorClass.
    /// </summary>
    /// <returns></returns>
    public Class Build() {
        AddCreateGUIStatements(rootVisualElementField.ToString());
        BuildFields(rootVisualElementField);
        
        // AddMethods(
        //     createGUI.ConnectSegments(),
        //     update.ConnectSegments(),
        //     modifyRoot,
        //     update2);
        
        return this;

        void BuildFields(VisualElementField field) {
            foreach (var child in rootVisualElementField.Children) {
                if (!child.Children.Any()) AddFields($"private {child.FieldName};");
                else BuildFields(child);
            }
        }
    }

    public CustomEditorClass Merge(CustomEditorClass other) {
        //base.Merge(other);

        createGUI.Merge(other.createGUI, [InitializationSegmentIndex, FinishSegmentIndex]);
        update.Merge(other.update, [InitializationSegmentIndex, FinishSegmentIndex]);

        return this;
    }

    public CustomEditorClass AddVisualElementToRoot(VisualElementField visualElementField) {
        rootVisualElementField.AddChild(visualElementField);
        
        return this;
    }

    public CustomEditorClass AddUpdateStatements(string statements) {
        // segment UniqueStatementsSegmentIndex is reserved for unique statements
        update.AddStatements(UniqueStatementsSegmentIndex, statements);

        return this;
    }

    public CustomEditorClass AddCreateGUIStatements(string statements) {
        // segment UniqueStatementsSegmentIndex is reserved for unique statements
        createGUI.AddStatements(UniqueStatementsSegmentIndex, statements);

        return this;
    }
    
    public SegmentedMethod CreateBareCreateGUI() {
        SegmentedMethod createGUI = new(CreateInspectorGUISignature);
            
        // segment InitializationSegmentIndex for init
        createGUI.AddStatements(InitializationSegmentIndex, $"""
                                                             // SEGMENT {InitializationSegmentIndex}
                                                             {EditorTargetFieldName} = ({BaseClassName})target;

                                                             InspectorElement.FillDefaultInspector(root, serializedObject, this);

                                                             // SEGMENT {UniqueStatementsSegmentIndex}
                                                             """);
        
        // segment UniqueStatementsSegmentIndex is reserved

        // segment FinishSegmentIndex for return;
        createGUI.AddStatements(FinishSegmentIndex, $"""

                                                    // SEGMENT {FinishSegmentIndex}
                                                    root.schedule.Execute(Update).Every(50);
                                                    
                                                    ModifyRoot(ref root); // modify before returning;

                                                    return root;
                                                    """);
            
        return createGUI;
    }
    
    public static SegmentedMethod CreateBareUpdateForEditor() {
        SegmentedMethod update = new SegmentedMethod(GeneratorHelper.UpdateSignature)
            .AddStatements(InitializationSegmentIndex, "if (!Application.isPlaying) return;\n") // InitializationSegmentIndex for check
            // UniqueStatementsSegmentIndex is reserved
            .AddStatements(FinishSegmentIndex, "Update2();"); // FinishSegmentIndex for additional call

        return update;
    }
}