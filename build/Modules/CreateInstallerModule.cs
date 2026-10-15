using Build.Options;
using Microsoft.Extensions.Options;
using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.FileSystem;
using ModularPipelines.Git.Extensions;
using ModularPipelines.Modules;
using ModularPipelines.Options;
using Shouldly;
using Sourcy.DotNet;
using File = ModularPipelines.FileSystem.File;

namespace Build.Modules;

/// <summary>
///     Create the .msi installer.
/// </summary>
[DependsOn<ResolveVersioningModule>]
[DependsOn<ResolveConfigurationsModule>]
[DependsOn<CompileProjectModule>]
public sealed class CreateInstallerModule(IOptions<BuildOptions> buildOptions) : Module
{
    protected override async Task ExecuteModuleAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        var versioningResult = await context.GetModule<ResolveVersioningModule>();
        var versioning = versioningResult.ValueOrDefault!;
        var configurationsResult = await context.GetModule<ResolveConfigurationsModule>();
        var configurations = configurationsResult.ValueOrDefault!;

        var wixTarget = new File(Projects.Kapibara_RevitToolKit.FullName);
        var wixInstaller = new File(Projects.Installer.FullName);
        var wixToolFolder = await InstallWixAsync(context, cancellationToken);

        await context.DotNet().Build(new DotNetBuildOptions
        {
            ProjectSolution = wixInstaller.Path,
            Configuration = "Release"
        }, cancellationToken: cancellationToken);

        var builderFile = wixInstaller.Folder!
            .GetFolder(Path.Combine("bin", "Release"))
            .FindFile(file =>
                file.NameWithoutExtension == wixInstaller.NameWithoutExtension && file.Extension == ".exe");

        builderFile.ShouldNotBeNull(
            $"No installer builder was found for the project: {wixInstaller.NameWithoutExtension}");

        var targetDirectories = configurations
            .Select(configuration => Path.Combine(wixTarget.Folder!.Path, "bin", configuration, "publish"))
            .ToArray();

        targetDirectories.ShouldNotBeEmpty("No content were found to create an installer");
        foreach (var directory in targetDirectories)
        {
            Directory.Exists(directory).ShouldBeTrue($"Publish directory not found: {directory}");
            Directory.GetFiles(directory, "*.addin").ShouldNotBeEmpty($"No add-in manifest found in {directory}");
        }

        await context.Shell.Command.ExecuteCommandLineTool(
            new GenericCommandLineToolOptions(builderFile.Path)
            {
                Arguments = [versioning.Version, ..targetDirectories]
            },
            new CommandExecutionOptions
            {
                WorkingDirectory = context.Git().RootDirectory,
                EnvironmentVariables = new Dictionary<string, string?>
                {
                    { "PATH", $"{Environment.GetEnvironmentVariable("PATH")};{wixToolFolder}" }
                }
            }, cancellationToken: cancellationToken);

        var outputFolder = context.Git().RootDirectory.GetFolder(buildOptions.Value.OutputDirectory);
        foreach (var outputFile in outputFolder.GetFiles(file => file.Extension == ".msi"))
        {
            context.Summary.KeyValue("Artifacts", "Installer", outputFile.Path);
        }
    }

    /// <summary>
    ///     Installs the WiX toolset required for building installers.
    /// </summary>
    private static async Task<Folder> InstallWixAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        var wixToolFolder = Folder.CreateTemporaryFolder();
        await context.DotNet().Tool.Execute(new DotNetToolOptions
        {
            Arguments = ["install", "wix", "--version", "6.0.2", "--tool-path", wixToolFolder.Path]
        }, cancellationToken: cancellationToken);

        return wixToolFolder;
    }
}
