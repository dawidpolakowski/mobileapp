#tool "nuget:?package=xunit.runner.console"
#tool "nuget:?package=NUnit.Runners&version=2.6.3"

public class TemporaryFileTransformation
{
    public string Path { get; set; }
    public string Original { get; set; }
    public string Temporary { get; set; }
}

var target = Argument("target", "Default");
var buildAll = Argument("buildall", Bitrise.IsRunningOnBitrise);

private Action Test(string testFiles)
{
    var testSettings = new XUnit2Settings
    {
        // Parallelism = ParallelismOption.All,
        Parallelism = ParallelismOption.None,
        HtmlReport = true,
        NoAppDomain = true,
        OutputDirectory = "./bin"
    };

    return () => XUnit2(GetFiles(testFiles), testSettings);
}

private Action BuildSolution(string targetProject, string configuration = "Release", string platform = "")
{
    var buildSettings = new MSBuildSettings 
    {
        Verbosity = Bitrise.IsRunningOnBitrise ? Verbosity.Verbose : Verbosity.Minimal,
        Configuration = configuration
    };

    if (!string.IsNullOrEmpty(platform))
    {
        buildSettings = buildSettings.WithProperty("Platform", platform);
    }

    return () => MSBuild(targetProject, buildSettings);
}

//Temporary variable replacement
private string GetCommitHash()
{   
    IEnumerable<string> redirectedOutput;
    StartProcess("git", new ProcessSettings
    {
        Arguments = "rev-parse HEAD",
        RedirectStandardOutput = true
    }, out redirectedOutput);

    return redirectedOutput.Last();
}

private TemporaryFileTransformation GetIntegrationTestsConfigurationTransformation()
{   
    const string path = "Toggl.Ultrawave.Tests.Integration/Helper/Configuration.cs";
    var commitHash = GetCommitHash(); 
    var filePath = GetFiles(path).Single();
    var file = TransformTextFile(filePath).ToString();

    return new TemporaryFileTransformation
    { 
        Path = path, 
        Original = file,
        Temporary = file.Replace("{CAKE_COMMIT_HASH}", commitHash)
    };
}

private TemporaryFileTransformation GetUITestsFileTransformation()
{   
    const string path = "Toggl.Daneel.Tests.UI/Credentials.cs";
    var username = EnvironmentVariable("TOGGL_UI_TEST_USERNAME");
    var password = EnvironmentVariable("TOGGL_UI_TEST_PASSWORD"); 
    var filePath = GetFiles(path).Single();
    var file = TransformTextFile(filePath).ToString();

    return new TemporaryFileTransformation
    { 
        Path = path, 
        Original = file,
        Temporary = file.Replace("{TOGGL_UI_TEST_USERNAME}", username).Replace("{TOGGL_UI_TEST_PASSWORD}", password)
    };
}

private TemporaryFileTransformation GetIntegrationTestsCredentialsTransformation()
{
    const string path = "Toggl.Ultrawave.Tests.Integration/Helper/TestUserCredentials.cs";
    var starterUsername = EnvironmentVariable("TOGGL_INTEGRATION_TEST_STARTER_USERNAME");
    var starterPassword = EnvironmentVariable("TOGGL_INTEGRATION_TEST_STARTER_PASSWORD");
    var filePath = GetFiles(path).Single();
    var file = TransformTextFile(filePath).ToString();

    return new TemporaryFileTransformation
    {
        Path = path,
        Original = file,
        Temporary = file.Replace("{TOGGL_INTEGRATION_TEST_STARTER_USERNAME}", starterUsername)
                        .Replace("{TOGGL_INTEGRATION_TEST_STARTER_PASSWORD}", starterPassword)
    };
}

var transformations = new List<TemporaryFileTransformation> 
{
    GetIntegrationTestsConfigurationTransformation(),
    GetUITestsFileTransformation(),
    GetIntegrationTestsCredentialsTransformation()
};

Setup(context => transformations.ForEach(transformation => System.IO.File.WriteAllText(transformation.Path, transformation.Temporary)));
Teardown(context => transformations.ForEach(transformation => System.IO.File.WriteAllText(transformation.Path, transformation.Original)));

//Build
Task("Clean")
    .Does(() => 
        {
            CleanDirectory("./bin");
            CleanDirectories("./**/obj");
        });

Task("Nuget")
    .IsDependentOn("Clean")
    .Does(() => NuGetRestore("./Toggl.sln"));

Task("Build.Tests.All")
    .IsDependentOn("Nuget")
    .Does(BuildSolution("./Toggl.sln", "Debug"));

Task("Build.Tests.Unit")
    .IsDependentOn("Nuget")
    .Does(BuildSolution("./Toggl.sln", "UnitTests"));

Task("Build.Tests.Integration")
    .IsDependentOn("Nuget")
    .Does(BuildSolution("./Toggl.sln", "ApiTests"));

Task("Build.Tests.UI")
    .IsDependentOn("Nuget")
    .Does(BuildSolution("./Toggl.sln", "Debug", "iPhoneSimulator"));

Task("Build.Release.iOS")
    .IsDependentOn("Nuget")
    .Does(BuildSolution("./Toggl.sln", "Release", "iPhone"));

Task("Build.Release.iOS.TestFlight")
    .IsDependentOn("Nuget")
    .Does(BuildSolution("./Toggl.sln", "Release.TestFlight"));

//Unit Tests
Task("Tests.Unit")
    .IsDependentOn(buildAll ? "Build.Tests.All" : "Build.Tests.Unit")
    .Does(Test("./bin/Debug/*.Tests.dll"));

//Integration Tests
Task("Tests.Integration")
    .IsDependentOn(buildAll ? "Build.Tests.All" : "Build.Tests.Integration")
    .Does(Test("./bin/Debug/*.Tests.Integration.dll"));

//UI Tests
Task("Tests.UI")
    .IsDependentOn("Build.Tests.UI")
    .Does(() => NUnit(GetFiles("./bin/Debug/*.Tests.UI.dll")));

// All Tests
Task("Tests")
    .IsDependentOn("Tests.Unit")
    .IsDependentOn("Tests.Integration")
    .IsDependentOn("Tests.UI");

//Default Operation
Task("Default")
    .IsDependentOn("Tests.Unit");

RunTarget(target);
