using System.Text.Json;

namespace NugetDepednencies;

public static class AssetsJsonReader
{
    private const string _defaultPath = "obj/project.assets.json";

    public static IEnumerable<ProjectLibrary>? DeserializeProjectLibraries(string? path = null)
    {
        var assets = DeserializeAssets(path);
        return assets?.Targets.Single().Value.ToProjectLibraries();
    }

    public static Assets? DeserializeAssets(string? path = null)
    {
        string json = File.ReadAllText(ResolvePath(path));
        return JsonSerializer.Deserialize<Assets>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    private static string ResolvePath(string? path)
    {
        if (path is null) return _defaultPath;
        if (path.EndsWith("json")) return path;
        if (Directory.GetFiles(path).Any(f => f.EndsWith("csproj"))) return Path.Join(path, _defaultPath);
        throw new ArgumentException($"path doesn't lead to a project or project.assets.json file. path: {path}");
    }
}


public class Assets
{
    public int Version { get; set; }

    public Targets Targets { get; set; } = new();
}

public class Targets : Dictionary<string, Target>
{
}

public class Target : Dictionary<string, AssetLibrary>
{
    public IEnumerable<ProjectLibrary> ToProjectLibraries()
    {
        return this.Select(ToProjectLibrary);
    }

    private ProjectLibrary ToProjectLibrary(KeyValuePair<string, AssetLibrary> entry)
    {
        return new ProjectLibrary(
                Package: PackageReference.Parse(entry.Key),
                Dependencies: entry.Value.Dependencies.Select(kvp => new PackageReference(kvp.Key, kvp.Value)).ToArray());
    }
}

public class AssetLibrary
{
    public string Type { get; set; } = string.Empty;
    
    public Dictionary<string, string> Dependencies { get; set; } = new();
}

