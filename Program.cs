
using System.Text.Json;
using NugetDepednencies;

var verb = CommandLineParser.Parse(args);
return verb switch {
    Verb.Assets => Assets(),
    Verb.Packages => Packages(),
    Verb.Check => Check(),
    _ => throw new ArgumentOutOfRangeException()
};

int Assets() {
    var assets = AssetsJsonReader.DeserializeAssets();
    JsonSerializer.Serialize(Console.OpenStandardOutput(), assets, new JsonSerializerOptions { WriteIndented = true });
    Console.WriteLine();
    return 0;
}

int Packages() {
    var packages = AssetsJsonReader.DeserializeProjectLibraries()?.Select(lib => lib.Package).ToList() ?? new();
    JsonSerializer.Serialize(Console.OpenStandardOutput(), packages, new JsonSerializerOptions { WriteIndented = true });
    Console.WriteLine();
    return 0;
}

int Check() {
    var packageVersions = AssetsJsonReader.DeserializeProjectLibraries()?
        .SelectMany(lib => lib.Dependencies.Append(lib.Package))
        .Distinct()
        .ToLookup(p => p.PackageName, p => p.Version);
    
    bool failed = false;
    var previousConsoleColor = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Yellow;
    foreach (var package in packageVersions!) {
        if (package.Count() > 1) {
            failed = true;
            Console.WriteLine($"Package version conflict. package {package.Key} has conflicting versions: {string.Join("; ", package)}");
        }
    }

    if (failed)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Found package version conflicts.");
        return 1;
    }

    Console.ForegroundColor = previousConsoleColor;
    Console.WriteLine("Project is ok.");
    return 0;
}

enum Verb {
    Assets,
    Packages,
    Check,
}

static class CommandLineParser
{
    public static Verb? Parse(string[] args)
    {
        if (args is []) {
            var verbs = Enum.GetValues<Verb>().Select(s => s.ToString().ToLower());
            Console.WriteLine($"supported commands: {string.Join(", ", verbs)}");
            return null;
        }

        string verbStr = args[0];
        if (!Enum.TryParse<Verb>(verbStr, ignoreCase: true, out var verb)) {
            var verbs = string.Join(", ", Enum.GetValues<Verb>().Select(s => s.ToString().ToLower()));
            Console.WriteLine($"supported commands: {verbs}. got {verbStr}");
            return null;
        }

        return verb;
    }
}
