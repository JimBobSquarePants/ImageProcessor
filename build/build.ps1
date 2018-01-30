$buildNumber = "$env:APPVEYOR_BUILD_NUMBER";
$buildPath = Resolve-Path ".";
$binPath = Join-Path $buildPath "build\_BuildOutput";
$testsPath = Join-Path $buildPath "tests";
$nuspecsPath = Join-Path $buildPath "build\nuspecs";
$nugetOutput = Join-Path $binPath "NuGets";

# Projects. Nuget Dependencies are handled in the nuspec files themselves and depend on the Major.Minor.Build number only.
$imageprocessor = @{
    name    = "ImageProcessor"
    version = "2.5.6.${buildNumber}"
    folder  = Join-Path $buildPath "src\ImageProcessor"
    output  = Join-Path $binPath "ImageProcessor\lib\net452"
    csproj  = "ImageProcessor.csproj"
    nuspec  = Join-Path $nuspecsPath "ImageProcessor.nuspec"
};

$projects = @($imageprocessor);

$testProjects = @(
    (Join-Path $testsPath "ImageProcessor.UnitTests\ImageProcessor.UnitTests.csproj"),
    (Join-Path $testsPath "ImageProcessor.Web.UnitTests\ImageProcessor.Web.UnitTests.csproj")
);

# Updates the AssemblyInfo file with the specified version.
# http://www.luisrocha.net/2009/11/setting-assembly-version-with-windows.html
function Update-AssemblyInfo ([string]$file, [string]$version) {

    $assemblyVersionPattern = 'AssemblyVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'
    $fileVersionPattern = 'AssemblyFileVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'
    $assemblyVersion = 'AssemblyVersion("' + $version + '")';
    $fileVersion = 'AssemblyFileVersion("' + $version + '")';

    (Get-Content $file) | ForEach-Object {
        ForEach-Object {$_ -replace $assemblyVersionPattern, $assemblyVersion } |
            ForEach-Object {$_ -replace $fileVersionPattern, $fileVersion }
    } | Set-Content $file
}

# Loop through our projects, patch, build, and pack.

# Patch and Build
foreach ($project in $projects) {

    Write-Host "Building project $($project.name) at version $($project.version)";
    Update-AssemblyInfo -file (Join-Path $project.folder "Properties\AssemblyInfo.cs") -version $project.version;

    $buildCommand = "msbuild $(Join-Path $project.folder $project.csproj) /t:Build /p:Warnings=true /p:Configuration=Release /p:Platform=AnyCPU /p:PipelineDependsOnBuild=False /p:OutDir=$($project.output) /clp:WarningsOnly /clp:ErrorsOnly /clp:Summary /clp:PerformanceSummary /v:Normal /nologo";
    Write-Host $buildCommand;
    Invoke-Expression $buildCommand;
}

#Test 
foreach ($testProject in $testProjects) {

    $testBuildCommand = "msbuild $($testProject) /t:Build /p:Configuration=Release /p:Platform=""AnyCPU"" /p:Warnings=true /clp:WarningsOnly /clp:ErrorsOnly /v:Normal /nologo"
    Write-Host "Building project $($testProject)";
    Invoke-Expression $testBuildCommand;
}

# Pack
foreach ($project in $projects) {

    $packCommand = "nuget pack $($project.nuspec) -OutputDirectory $($nugetOutput) -Version $($project.version)";
    Write-Host $packCommand;
    Invoke-Expression $packCommand;
}