using UnityExtended.Generators.FillerData;
using UnityExtended.Generators.Helpers;
using UnityExtended.Generators.Hierarchy;

namespace UnityExtended.Generators.ClassFillers;

public class HandleInputFiller : IClassFiller<HandleInputFillerData, Class> {
    public const string AwakeMethodSignature = $"private void HandleInputAwake{GeneratorHelper.GenerationPostfix}()";
    public const string OnEnableMethodSignature = $"private void HandleInputOnEnable{GeneratorHelper.GenerationPostfix}()";
    public const string OnDisableMethodSignature = $"private void HandleInputOnDisable{GeneratorHelper.GenerationPostfix}()";

    
    public Class Fill(Class c, HandleInputFillerData data) {
        c.AddUsings("""
                    UnityExtended.Core.Types
                    UnityEngine.InputSystem
                    """);

        var awake = c.GetOrCreateMethod(AwakeMethodSignature);
        var onEnable = c.GetOrCreateMethod(OnEnableMethodSignature);
        var onDisable = c.GetOrCreateMethod(OnDisableMethodSignature);

        var inputAsset = data.InputAsset;
        
        awake.AddStatement($"{inputAsset.ConcreteInputAssetName} = InputSingletonsManager.GetInstance<{inputAsset.FullyQualifiedInputAssetName}>();");

        c.AddField($"private {inputAsset.FullyQualifiedInputAssetName} {inputAsset.ConcreteInputAssetName};");
        
        foreach (var actionMap in inputAsset.ActionMaps) {
            c.AddField($"private {actionMap.FullyQualifiedActionMapName} {actionMap.ConcreteActionMapName};");

            awake.AddStatement($"{actionMap.ConcreteActionMapName} = {inputAsset.ConcreteInputAssetName}.{actionMap.ConcreteActionMapName.Replace("Actions", "")};");

            foreach (var action in actionMap.Actions) {
                foreach (var actionEvent in action.Events) {
                    c.GetOrCreateMethod($"partial void {actionEvent.MethodName}(InputAction.CallbackContext callbackContext)");

                    if (!actionEvent.Implemented) continue;
                    
                    string additionStatement =
                        $"{actionMap.ConcreteActionMapName}.{action.Name}.{actionEvent.EventName} += {actionEvent.MethodName};";
                    onEnable.AddStatement(additionStatement);
                    onDisable.AddStatement(additionStatement.Replace("+", "-"));
                }
            }
        }

        return c;
    }
}