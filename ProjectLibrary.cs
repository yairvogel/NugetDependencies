namespace NugetDepednencies;

public record struct PackageReference(string PackageName, string Version)
{
    public static PackageReference Parse(string assetsPackageReference)
    {
        if (assetsPackageReference.Split("/") is not [string packageName, string version]) {
            throw new ArgumentException($"expected one forward slash (/) in the project identifier. got {assetsPackageReference}");
        }
        return new(packageName, version);
    }
}

public record struct ProjectLibrary(PackageReference Package, PackageReference[] Dependencies);

