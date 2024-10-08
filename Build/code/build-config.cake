//////////////////////////////////////////////////////////////////////
// Build Config
//////////////////////////////////////////////////////////////////////

#addin nuget:?package=Cake.Json&version=7.0.1

#load const.cake

using System.Text.RegularExpressions;

class BuildConfig {

    private const string BaseTag = "$base";

    public static string GetBaseFileName() {
        if (string.IsNullOrEmpty(Global.Platform)) return $"shared-{Global.Target}.json"; 
        else return $"{Global.Platform}-{Global.Target}.json"; 
    } 

    public static string GetBaseFilePath() => Path.GetFullPath(Path.Join(Global.ConfigDir, GetBaseFileName()));

    private static string FeaturesConfig => Path.GetFullPath(Path.Join(Global.ConfigDir, "_features.json"));

    private class FeatureConfigEntry {
        public string Name { get; set; }
        public string Define { get; set; }
        public string Comment { get; set; }
        public bool DefaultOn { get; set; }
    }

    public static string FeatureDefines { get; private set; }

    private static void RecursiveLoadConfig(string basePath, string fileName) {
        
        var fullName = Path.GetFullPath(Path.Join(basePath, fileName));

        var loadedConfig = Global.Context.DeserializeJsonFromFile<Dictionary<string, string>>(fullName);
        loadedConfig.TryGetValue("$base", out var baseFileName);

        if (!string.IsNullOrEmpty(baseFileName)) {
            foreach (var baseFn in baseFileName.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) {
                RecursiveLoadConfig(basePath, baseFn);
            }
        }

        Global.Context.Information($"    Loading build config from '{fullName}'");
        foreach (var cfg in loadedConfig) {
            if (cfg.Key == BaseTag) continue;
            _config[cfg.Key] = cfg.Value;
        }
    }

    public static void Load() {
        Global.Context.Information("Loading build config");
        var featureConfig = Global.Context.DeserializeJsonFromFile<FeatureConfigEntry[]>(FeaturesConfig);

        foreach (var feature in featureConfig) {
            _config[$"Feature.{feature.Name}"] = feature.DefaultOn ? "true" : "false";
        }

        RecursiveLoadConfig(Global.ConfigDir, GetBaseFileName());

        var envVariables = Global.Context.EnvironmentVariables();
        foreach (var env in envVariables) {
            var key = env.Key;
            if (key.ToLowerInvariant().StartsWith("feature_")) key = $"Feature.{key["feature_".Length..]}";
            _config[key] = env.Value;
        }

        var defines = new List<string>(8);

        foreach (var feature in featureConfig) {
            var key = $"Feature.{feature.Name}";
            var val = Var(key, "").ToLowerInvariant();
            if (val != "true" && val != "1") continue;
            defines.Add(feature.Define);
        }

        FeatureDefines = string.Join(';', defines);
    }

    private static Dictionary<string, string> _config = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

    public static string Var(string key) => _config.TryGetValue(key, out var result) ? result : throw new Exception($"Required option not found: {key}");
    public static string Var(string key, string def) => _config.TryGetValue(key, out var result) ? result : def;

    public static string ReplaceVariables(string str) {
        var regex = new Regex(@"\$\{([A-Za-z0-9]*)\}");
        var allVariables = regex.Matches(str).Select(x => x.Groups[1].ToString()).Where(x => x.Length > 0).Distinct();
        foreach (var variable in allVariables) {
            str = str.Replace($"${{{variable}}}", Var(variable));
        }

        return str;
    }
}

BuildConfig.Load();

#if !SL_BUILD
if (Global.Platform != BuildConfig.Var("Target")) throw new Exception($"Building platform set to '{Global.Platform}' but config loaded for '{BuildConfig.Var("Target")}'");
#endif
