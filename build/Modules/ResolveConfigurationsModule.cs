using Microsoft.VisualStudio.SolutionPersistence.Model;
using Microsoft.VisualStudio.SolutionPersistence.Serializer;
using ModularPipelines.Context;
using ModularPipelines.Modules;
using Shouldly;
using Sourcy.DotNet;

namespace Build.Modules;

/// <summary>
///     Resolve solution configurations required to compile the add-in for all supported Revit versions.
/// </summary>
public sealed class ResolveConfigurationsModule : Module<string[]>
{
    protected override async Task<string[]?> ExecuteAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        var solutionModel = await LoadSolutionModelAsync(cancellationToken);
        var configurations = solutionModel.BuildTypes
            .Where(configuration => configuration.Contains("Release.R", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        configurations.ShouldNotBeEmpty($"No Release.R configurations found in {Solutions.Kapibara_RevitToolkit.FullName}");
        context.Summary.KeyValue("Build", "Configurations", string.Join(", ", configurations));

        return configurations;
    }

    private static async Task<SolutionModel> LoadSolutionModelAsync(CancellationToken cancellationToken)
    {
        var solutionPath = Solutions.Kapibara_RevitToolkit.FullName;
        await using var stream = File.OpenRead(solutionPath);
        return Path.GetExtension(solutionPath).Equals(".slnx", StringComparison.OrdinalIgnoreCase)
            ? await SolutionSerializers.SlnXml.OpenAsync(stream, cancellationToken)
            : await SolutionSerializers.SlnFileV12.OpenAsync(stream, cancellationToken);
    }
}
