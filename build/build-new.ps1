$buildNumber = "$env:APPVEYOR_BUILD_NUMBER";
$buildPath = Resolve-Path ".";
$binPath = Join-Path $buildPath "build\_BuildOutput";
$nuspecsPath = Join-Path $buildPath "build\nuspecs";
$nugetOutput = Join-Path $binPath "NuGets";

Write-Debug $buildPath;
Write-Debug $binPath;
Write-Debug $nuspecsPath;
Write-Debug $nugetOutput;

# Our project objects
$imageprocessor = @{
    name    = "ImageProcessor"
    version = "2.5.6.${buildNumber}"
    folder  = Join-Path $buildPath "src\ImageProcessor"
    output  = Join-Path $binPath "ImageProcessor\lib\net452"
    csproj  = "ImageProcessor.csproj"
    nuspec  = Join-Path $nuspecsPath "ImageProcessor.nuspec"
};

$projects = @($imageprocessor);

# Updates the AssemblyInfo file with the specified version
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

# Loop through our projects, build and pack
foreach ($project in $projects) {

    Write-Host "Building project $($project.name) at version $($project.version)";
    Update-AssemblyInfo -file (Join-Path $project.folder "Properties\AssemblyInfo.cs") -version $project.version;

    $buildCommand = "msbuild $(Join-Path $project.folder $project.csproj) /t:Build /p:Warnings=true /p:Configuration=Release /p:Platform=AnyCPU /p:PipelineDependsOnBuild=False /p:OutDir=$($project.output) /clp:WarningsOnly /clp:ErrorsOnly /clp:Summary /clp:PerformanceSummary /v:Normal /nologo";
    Write-Host $buildCommand;
    Invoke-Expression $buildCommand;

    $packCommand = "nuget pack $($project.nuspec) -OutputDirectory $($nugetOutput) -Version $($project.version)";
    Write-Host $packCommand;
    Invoke-Expression $packCommand;
    # $NUGET_EXE Pack $nuspec_local_path -OutputDirectory $NUGET_OUTPUT -Version "$($_.version)$($_.prerelease)"
}