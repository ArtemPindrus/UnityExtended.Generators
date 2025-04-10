using Microsoft.CodeAnalysis;

namespace UnityExtended.Generators.Generators;

public abstract class FunctionalGenerator<TProvider> {
    public IncrementalValuesProvider<TProvider> Initialize(IncrementalGeneratorInitializationContext context) {
        var provider = CreatePipeline(context);
        
        context.RegisterSourceOutput(provider, Action);

        return provider;
    }

    protected abstract IncrementalValuesProvider<TProvider> CreatePipeline(IncrementalGeneratorInitializationContext context);
    
    protected abstract void Action(SourceProductionContext context, TProvider entry);
}