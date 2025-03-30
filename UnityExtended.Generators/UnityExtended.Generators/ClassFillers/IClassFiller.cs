using UnityExtended.Generators.FillerData;
using UnityExtended.Generators.Hierarchy;

namespace UnityExtended.Generators.ClassFillers;

public interface IClassFiller<TData, TClass> 
    where TData : IFillerData 
    where TClass : Class {
    public TClass Fill(TClass c, TData data);
}