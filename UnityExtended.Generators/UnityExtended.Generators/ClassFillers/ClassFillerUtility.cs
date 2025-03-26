using UnityExtended.Generators.FillerData;
using UnityExtended.Generators.Hierarchy;

namespace UnityExtended.Generators.ClassFillers;

public static class ClassFillerUtility {
    public static Class GenericFill(Class c, IFillerData d) {
        if (d is GetComponentFillerData getComponentFillerData) {
            GetComponentFiller f = new();
            f.Fill(c, getComponentFillerData);
        } else if (d is GetComponentAheadFillerData getComponentAheadFillerData) {
            GetComponentAheadFiller f = new();
            f.Fill(c, getComponentAheadFillerData);
        } else if (d is HandleInputFillerData handleInputFillerData) {
            HandleInputFiller f = new();
            f.Fill(c, handleInputFillerData);
        }

        return c;
    }
}