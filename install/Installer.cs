using Installer;
using WixSharp;
using WixSharp.CommonTasks;
using WixSharp.Controls;

const string outputName = "Kapibara.RevitToolkit";
const string projectName = "Kapibara.RevitToolkit";

var versioning = Versioning.CreateFromVersionStringAsync(args[0]);
var project = new Project
{
    OutDir = "output",
    Name = projectName,
    Platform = Platform.x64,
    UI = WUI.WixUI_FeatureTree,
    MajorUpgrade = MajorUpgrade.Default,
    GUID = new Guid("0F380971-498B-4F2F-9A1A-4C3C2138955D"),
    BannerImage = @"install\Resources\Icons\BannerImage.png",
    BackgroundImage = @"install\Resources\Icons\BackgroundImage.png",
    Version = versioning.VersionPrefix,
    ControlPanelInfo =
    {
        Manufacturer = Environment.UserName,
        ProductIcon = @"install\Resources\Icons\ShellIcon.ico"
    }
};

project.RemoveDialogsBetween(NativeDialogs.WelcomeDlg, NativeDialogs.CustomizeDlg);

BuildSingleUserMsi();
BuildMultiUserUserMsi();

void BuildSingleUserMsi()
{
    project.Scope = InstallScope.perUser;
    project.OutFileName = $"{outputName}-{versioning.Version}-SingleUser";
    project.Dirs =
    [
        new InstallDir(@"%AppDataFolder%\Autodesk\Revit\Addins\", Generator.GenerateWixEntities(args[1..]))
    ];
    project.BuildMsi();
}

void BuildMultiUserUserMsi()
{
    project.Scope = InstallScope.perMachine;
    project.OutFileName = $"{outputName}-{versioning.Version}-MultiUser";
    // Revit 2027 changed the all-user add-in location; the product version is unrelated.
    project.Dirs = Generator.GenerateWixEntities(args[1..])
        .Cast<Dir>()
        .GroupBy(directory => int.Parse(directory.Name) >= 2027
            ? @"%ProgramFiles%\Autodesk\Revit\Addins"
            : @"%CommonAppDataFolder%\Autodesk\Revit\Addins")
        .Select((group, index) => index == 0
            ? new InstallDir(group.Key, group.Cast<WixEntity>().ToArray())
            : new Dir(group.Key, group.Cast<WixEntity>().ToArray()))
        .ToArray();
    project.BuildMsi();
}
