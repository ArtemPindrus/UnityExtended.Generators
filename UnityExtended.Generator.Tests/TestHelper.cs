namespace UnityExtended.Generator.Tests;

public static class TestHelper {
    public static void SanitizeString(ref string s) {
        var split = s.Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Where(x => !x.Contains("//"));

        s = string.Join('\n', split);
    }
}