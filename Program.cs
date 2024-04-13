
using System.CommandLine;
using System.Text.Json;
using NugetDepednencies;

var rootCommand = new RootCommand("NugetDependencies Command Line Tool");

var projectPathOption = new Option<string?>(aliases: ["--projectPath", "-p"], description: "The project path to check");

var assetsCommand = new Command("assets", "read the package-specific bits of the project.assets.json file of the current project");
assetsCommand.AddOption(projectPathOption);
assetsCommand.SetHandler(Assets, projectPathOption);
rootCommand.AddCommand(assetsCommand);

var packagesCommand = new Command("packages", "list all the project packages and versions in json format");
packagesCommand.AddOption(projectPathOption);
packagesCommand.SetHandler(Packages, projectPathOption);
rootCommand.AddCommand(packagesCommand);

var checkCommand = new Command("check", "validate that there are no package version conflicts");
checkCommand.AddOption(projectPathOption);
checkCommand.SetHandler(Check, projectPathOption);
rootCommand.AddCommand(checkCommand);

rootCommand.Invoke(args);

void Assets(string? projectPath) {
    var assets = AssetsJsonReader.DeserializeAssets(projectPath);
    JsonSerializer.Serialize(Console.OpenStandardOutput(), assets, new JsonSerializerOptions { WriteIndented = true });
    Console.WriteLine();
}

void Packages(string? projectPath) {
    var packages = AssetsJsonReader.DeserializeProjectLibraries(projectPath)?.Select(lib => lib.Package).ToList() ?? new();
    JsonSerializer.Serialize(Console.OpenStandardOutput(), packages, new JsonSerializerOptions { WriteIndented = true });
    Console.WriteLine();
}

void Check(string? projectPath) {
    var packageVersions = AssetsJsonReader.DeserializeProjectLibraries(projectPath)?
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
    }

    Console.ForegroundColor = previousConsoleColor;
    Console.WriteLine("Project is ok.");
}
