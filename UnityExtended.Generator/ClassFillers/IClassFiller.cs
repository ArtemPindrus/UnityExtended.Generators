using Hierarchy;
using UnityExtended.Generator.FillerData;

namespace UnityExtended.Generator.ClassFillers;

public interface IClassFiller<T> where T : IFillerData {
    public Class Fill(Class c, T data);
}