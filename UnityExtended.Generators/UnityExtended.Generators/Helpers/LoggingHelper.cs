using System.IO;

#if LOG
using SourceGenerator.SharedLibrary;
#endif

namespace UnityExtended.Generators.Helpers;

public static class LoggingHelper {
    public const string LoggingBasePath =
        @"C:\Users\Artem\Documents\C#\UnityExtended.Generator\UnityExtended.Generators\UnityExtended.Generators\Logging";

    private static string? lastTxtFileName;

    public static void Log(string message, string txtFileName) {
#if LOG
        if (txtFileName != lastTxtFileName) {
            GeneratorLogging.SetLogFilePath(Path.Combine(LoggingBasePath, $"{txtFileName}.txt"));
            lastTxtFileName = txtFileName;
        }
        
        GeneratorLogging.LogMessage(message);
#endif
    }
}