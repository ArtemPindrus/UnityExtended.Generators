using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using UnityExtended.Generators.Helpers;
using UnityExtended.Generators.Hierarchy;

namespace UnityExtended.Generator;

public class CustomEditorClass : Class {
    public const string EditorTargetFieldName = "targetCasted";
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
                                             UnityExtended.Core.Editor.Drawers
                                             """;

    private const string MainReservationID = "CustomInspectorMainRes";

    public const string NamePostFix = "Inspector";

    private readonly VisualElementCodeDefinition rootVisualElement = new("root");
    public readonly string BaseClassName;
    
    private readonly Reservation createGuiReservation;
    private readonly Reservation updateReservation;
    
    /// <summary>
    /// Creates class with scaffolding for Custom Inspector.
    /// </summary>
    /// <param name="baseClassName">Name of a class for which to create Custom Inspector. e.g. for LookAround generates LookAroundInspector.</param>
    private CustomEditorClass(string baseClassName) : base($"{baseClassName}{NamePostFix}") {
        BaseClassName = baseClassName;
        
        AddImplementation("UnityEditor.Editor");
        AddAttribute($"[UnityEditor.CustomEditor(typeof({baseClassName}))]"); // UnityEditor.CanEditMultipleObjects
        AddFields($"private {baseClassName} {EditorTargetFieldName};");
        AddConstraint(UnityEditorConstraint);
        AddUsings(CustomEditorUsings);

        // create methods
        GetOrCreateMethod(ModifyRootSignature);
        GetOrCreateMethod(GeneratorHelper.Update2Signature);
        
        createGuiReservation = CreateBareCreateGUI();
        updateReservation = CreateBareUpdateForEditor();
    }
    
    /// <summary>
    /// Gets cached <see cref="CustomEditorClass"/> or creates one and caches.
    /// </summary>
    /// <returns>true - got cached class. false - created new.</returns>
    public static bool GetOrCreate(string baseClassName, out CustomEditorClass c) {
        if (!GetCachedClass(baseClassName, out c)) {
            c = new CustomEditorClass(baseClassName);
            CacheClass(c);
            return false;
        }

        return true;
    }
    
    // TODO: delete caching
    /// <summary>
    /// Gets cached Class or creates one and caches.
    /// </summary>
    public new static CustomEditorClass GetOrCreate(string baseClassName) {
        GetOrCreate(baseClassName, out var c);

        return c;
    }

    protected static bool GetCachedClass(string fullyQualifiedName, out CustomEditorClass c) {
        if (FQNameToClass.TryGetValue(fullyQualifiedName, out Class existingClass)) {
            if (existingClass is CustomEditorClass found) {
                c = found;
                return true;
            }
        }

        c = null;
        return false;
    }

    public override Class Finish() {
        base.Finish();
        
        foreach (var field in rootVisualElement.GetHierarchyFields()) {
            AddField($"private {field.TypeName} {field.VariableName};");
        }

        foreach (var child in rootVisualElement.Children) {
            AddCreateGUIStatements(child.ToString());
        }
        
        return this;
    }

    public CustomEditorClass AddVisualElementToRoot(VisualElementCodeDefinition child) {
        rootVisualElement.AddChild(child);
        
        return this;
    }
    
    public CustomEditorClass AddVisualElementsToRoot(params VisualElementCodeDefinition[] childen) {
        rootVisualElement.AddChildren(childen);
        
        return this;
    }

    public CustomEditorClass AddUpdateStatements(string statements) {
        updateReservation.AddStatements(statements);

        return this;
    }

    public CustomEditorClass AddCreateGUIStatements(string statements) {
        createGuiReservation.AddStatements(statements);

        return this;
    }
    
    private CustomEditorClass InsertCreateGUIStatements(string statements, int index) {
        foreach (var statement in statements.Split('\n')) {
            createGuiReservation.InsertStatement(statement, index);
            index++;
        }

        return this;
    }
    
    private CustomEditorClass InsertCreateGUIStatement(string statement, int index) {
        createGuiReservation.InsertStatement(statement, index);

        return this;
    }
    
    private Reservation CreateBareCreateGUI() {
        var m = GetOrCreateMethod(GeneratorHelper.CreateInspectorGUISignature);
        m.AddStatements($"""
                         {EditorTargetFieldName} = ({BaseClassName})target;
                         var root = new VisualElement();

                         InspectorElement.FillDefaultInspector(root, serializedObject, this);
                         """);

        m.GetOrCreateReservation(MainReservationID, out var mainRes);
        
        m.GetOrCreateReservation(FinishReservationID)
            .AddStatements("""
                            root.schedule.Execute(Update).Every(50);
                            
                            ModifyRoot(ref root); // modify before returning;
                            
                            return root;
                            """);
            
        return mainRes;
    }
    
    public Reservation CreateBareUpdateForEditor() {
        var updateMethod = GetOrCreateMethod(GeneratorHelper.UpdateSignature);

        updateMethod.AddStatements("if (!Application.isPlaying) return;\n");

        updateMethod.GetOrCreateReservation(MainReservationID, out var mainRes);

        return mainRes;
    }
}