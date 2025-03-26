using UnityExtended.Generators.FillerData;
using UnityExtended.Generators.Helpers;
using UnityExtended.Generators.Hierarchy;

namespace UnityExtended.Generators.ClassFillers;

public class HandleInputFiller : IClassFiller<HandleInputFillerData> {
    private const string ReservationsID = "HandleInputRes";
    
    public Class Fill(Class c, HandleInputFillerData data) {
        var inputAsset = data.InputAsset;
        
        var awakeMethod = c.GetOrCreateMethod(GeneratorHelper.AwakeMethodSignature);
        var onEnableMethod = c.GetOrCreateMethod(GeneratorHelper.OnEnableMethodSignature);
        var onDisableMethod = c.GetOrCreateMethod(GeneratorHelper.OnDisableMethodSignature);
        
        var awakeReservation = awakeMethod.GetOrCreateReservation(ReservationsID);
        var onEnableReservation = onEnableMethod.GetOrCreateReservation(ReservationsID);
        var onDisableReservation = onDisableMethod.GetOrCreateReservation(ReservationsID);
        
        c.AddUsings("""
                    UnityExtended.Core.Types
                    UnityEngine.InputSystem
                    """);

        c.GetOrCreateMethod(GeneratorHelper.Awake2MethodSignature);
        c.GetOrCreateMethod(GeneratorHelper.OnEnable2MethodSignature);
        c.GetOrCreateMethod(GeneratorHelper.OnDisable2MethodSignature);
        
        awakeReservation.AddStatement($"{inputAsset.ConcreteInputAssetName} = InputSingletonsManager.GetInstance<{inputAsset.FullyQualifiedInputAssetName}>();");

        c.AddField($"private {inputAsset.FullyQualifiedInputAssetName} {inputAsset.ConcreteInputAssetName};");

        foreach (var actionMap in inputAsset.ActionMaps) {
            c.AddField($"private {actionMap.FullyQualifiedActionMapName} {actionMap.ConcreteActionMapName};");

            awakeReservation.AddStatement($"{actionMap.ConcreteActionMapName} = {inputAsset.ConcreteInputAssetName}.{actionMap.ConcreteActionMapName.Replace("Actions", "")};");

            foreach (var action in actionMap.Actions) {
                foreach (var actionEvent in action.Events) {
                    c.GetOrCreateMethod($"partial void {actionEvent.MethodName}(InputAction.CallbackContext callbackContext)");

                    if (!actionEvent.Implemented) continue;
                    
                    string additionStatement =
                        $"{actionMap.ConcreteActionMapName}.{action.Name}.{actionEvent.EventName} += {actionEvent.MethodName};";
                    onEnableReservation.AddStatement(additionStatement);
                    onDisableReservation.AddStatement(additionStatement.Replace("+", "-"));
                }
            }
        }

        return c;
    }
}