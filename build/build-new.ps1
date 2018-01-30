$appveyorBuildNumber = "$env:APPVEYOR_BUILD_NUMBER";

$buildPath = Resolve-Path ".";
$srcPath = Join-Path $buildPath ".\src";
$nuspecPath = Join-Path $buildPath ".\build\nuspecs"
$binPath = Join-Path $buildPath "_buildoutput"

Write-Debug $buildPath;
Write-Debug $srcPath;

# Our project objects
$imageprocessor = @{
    name    = "ImageProcessor"
    version = "2.5.6.${appveyorBuildNumber}"
    folder  = Join-Path $buildPath "\src\ImageProcessor"
    output  = Join-Path $binPath "\ImageProcessor\lib\net452"
    csproj  = "ImageProcessor.csproj"
    nuspec  = "ImageProcessor.nuspec"
};

$projects = @($imageprocessor);

# Updates the AssemblyInfo file with the specified version
# http://www.luisrocha.net/2009/11/setting-assembly-version-with-windows.html
function Update-AssemblyInfo ([string]$file, [string]$version) {
    
Write-Host "Patching assembly to $($version)"

    $assemblyVersionPattern = 'AssemblyVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'
    $fileVersionPattern = 'AssemblyFileVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'
    $assemblyVersion = 'AssemblyVersion("' + $version + '")';
    $fileVersion = 'AssemblyFileVersion("' + $version + '")';

    (Get-Content $file) | ForEach-Object {
        ForEach-Object {$_ -replace $assemblyVersionPattern, $assemblyVersion } |
            ForEach-Object {$_ -replace $fileVersionPattern, $fileVersion }
    } | Set-Content $file
}

foreach ($project in $projects) {

    Write-Host "Building project $($project.name) at version $($project.version)";
    Update-AssemblyInfo -file (Join-Path $project.folder "Properties\AssemblyInfo.cs") -version $project.version;

    $buildCommand = "msbuild $(Join-Path $project.folder $project.csproj) 
    /t:Build 
    /p:Warnings=true 
    /p:Configuration=Release 
    /p:Platform=AnyCPU
    /p:PipelineDependsOnBuild=False 
    /p:OutDir=$($project.output) 
    /clp:WarningsOnly 
    /clp:ErrorsOnly 
    /clp:Summary 
    /clp:PerformanceSummary 
    /v:Normal /nologo";
    Write-Host $buildCommand;
    Invoke-Expression $buildCommand;

    #TODO: Nuget
}