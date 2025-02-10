using System.Collections.Generic;
using Hierarchy;

namespace UnityExtended.Generator;

public interface IGenerate{}

public interface IGenerateClass : IGenerate {
    public Class GeneratedClass { get; }
}