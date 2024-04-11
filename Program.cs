using System.Text.Json;

string json = File.ReadAllText("obj/project.assets.json");
var assets = JsonSerializer.Deserialize<Assets>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

var projectLibraries = assets!.Targets.Single().Value.ToProjectLibraries();

foreach (var lib in projectLibraries) 
{
    Console.WriteLine(lib);
    Console.WriteLine("[ " + string.Join(",", lib.Dependencies.Select(x => x)) + " ]");
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
        if (entry.Key.Split("/") is not [string packageName, string version]) {
            throw new ArgumentException($"expected one forward slash (/) in the project identifier. got {entry.Key}");
        }

        return new ProjectLibrary(
                PackageName: packageName,
                Version: version,
                Dependencies: entry.Value.Dependencies.Select(kvp => new PackageReference(kvp.Key, kvp.Value)).ToArray());
    }
}

public class AssetLibrary
{
    public string Type { get; set; } = string.Empty;
    
    public Dictionary<string, string> Dependencies { get; set; } = new();
}

public record PackageReference(string PackageName, string Version);

public record ProjectLibrary(string PackageName, string Version, PackageReference[] Dependencies);
