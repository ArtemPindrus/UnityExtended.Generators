using System;
using UnityExtended.Generators.Extensions;

namespace UnityExtended.Generators.FillerData;

public record struct HandleInputFillerData : IFillerData {
    public string FullyQualifiedGeneratedClassName { get; }
    public InputAsset InputAsset { get; }
    
    public HandleInputFillerData(string fullyQualifiedGeneratedClassName, InputAsset inputAsset) {
        FullyQualifiedGeneratedClassName = fullyQualifiedGeneratedClassName;
        InputAsset = inputAsset;
    }
}

public record struct InputAsset {
    /// <summary>
    /// Fully qualified name of the generated type for the Input Asset.
    /// </summary>
    public string FullyQualifiedInputAssetName { get; }

    public string ConcreteInputAssetName { get; }

    public EquatableList<ActionMap> ActionMaps { get; } = new();
    
    public InputAsset(string fullyQualifiedInputAssetName, string concreteInputAssetName) {
        FullyQualifiedInputAssetName = fullyQualifiedInputAssetName;
        ConcreteInputAssetName = concreteInputAssetName;
    }
}

public record struct ActionMap {
    public string FullyQualifiedActionMapName { get; }
    public string ConcreteActionMapName { get; }

    public EquatableList<Action> Actions { get; } = new();
    
    public ActionMap(string fullyQualifiedActionMapName, string concreteActionMapName) {
        FullyQualifiedActionMapName = fullyQualifiedActionMapName;
        ConcreteActionMapName = concreteActionMapName;
    }
}

public record struct Action {
    private const string EventPrefix = "On";
    
    public string Name { get; }
    public EquatableList<Event> Events { get; }
    
    public Action(string name) {
        Name = name;
        string methodName = $"{EventPrefix}{Name}";
        
        Events = [
            new Event(methodName, "started"),
            new Event(methodName, "performed"),
            new Event(methodName, "canceled"),
        ];
    }
}

public record class Event {
    public bool Implemented { get; private set; }
    
    public string MethodName { get; }
    public string EventName { get; }
    
    public Event(string methodPrefix, string eventName, bool Implemented = false) {
        MethodName = $"{methodPrefix}{eventName.ToUpperFirst()}";
        EventName = eventName;
        this.Implemented = Implemented;
    }
    
    public void SetIsImplemented() => Implemented = true;
}