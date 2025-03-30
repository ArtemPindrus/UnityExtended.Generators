using UnityExtended.Generator;
using UnityExtended.Generators.FillerData;
using UnityExtended.Generators.Hierarchy;

namespace UnityExtended.Generators.ClassFillers;

public static class ClassFillerUtility {
    public static Class Fill<TClassFiller, TClass, TData>(TClass c, TClassFiller filler, TData data)
        where TClass : Class
        where TData : IFillerData
        where TClassFiller : IClassFiller<TData, TClass>, new() 
    {
        filler.Fill(c, data);

        return c;
    }
}