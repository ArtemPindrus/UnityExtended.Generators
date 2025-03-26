using UnityExtended.Generators.FillerData;
using UnityExtended.Generators.Hierarchy;

namespace UnityExtended.Generators.ClassFillers;

public interface IClassFiller<T> where T : IFillerData {
    public Class Fill(Class c, T data);
}