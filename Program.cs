using System.Text.Json;

string json = File.ReadAllText("obj/project.assets.json");
var assets = JsonSerializer.Deserialize<Assets>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

Console.WriteLine(assets!.Version);
foreach (var target in assets!.Targets)
{
    Console.WriteLine(target.Key);
    foreach (var library in target.Value)
    {
        Console.WriteLine($"\t{library.Key}");
        Console.WriteLine($"\t\t{library.Value.Type}");
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
}

public class AssetLibrary
{
    public string Type { get; set; } = string.Empty;
    
    public Dictionary<string, string> Dependencies { get; set; } = new();
}
