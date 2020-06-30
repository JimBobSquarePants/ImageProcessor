$buildNumber = "$env:APPVEYOR_BUILD_NUMBER".Trim().PadLeft(5, "0");
$buildPath = Resolve-Path ".";
$binPath = Join-Path $buildPath "build\_BuildOutput";
$testsPath = Join-Path $buildPath "tests";
$nuspecsPath = Join-Path $buildPath "build\nuspecs";
$nugetOutput = Join-Path $binPath "NuGets";

# Projects (NuGet dependencies are handled in the nuspec files themselves)
$imageprocessor = @{
    name    = "ImageProcessor"
    version = "2.9.0"
    folder  = Join-Path $buildPath "src\ImageProcessor"
    output  = Join-Path $binPath "ImageProcessor\lib\net452"
    csproj  = "ImageProcessor.csproj"
    nuspec  = Join-Path $nuspecsPath "ImageProcessor.nuspec"
};

$imageProcessorPluginsCair = @{
    name    = "ImageProcessor.Plugins.Cair"
    version = "1.3.1"
    folder  = Join-Path $buildPath "src\ImageProcessor.Plugins.Cair"
    output  = Join-Path $binPath "ImageProcessor.Plugins.Cair\lib\net452"
    csproj  = "ImageProcessor.Plugins.Cair.csproj"
    nuspec  = Join-Path $nuspecsPath "ImageProcessor.Plugins.Cair.nuspec"
};

$imageProcessorPluginsWebP = @{
    name    = "ImageProcessor.Plugins.WebP"
    version = "1.3.0"
    folder  = Join-Path $buildPath "src\ImageProcessor.Plugins.WebP"
    output  = Join-Path $binPath "ImageProcessor.Plugins.WebP\lib\net452"
    csproj  = "ImageProcessor.Plugins.WebP.csproj"
    nuspec  = Join-Path $nuspecsPath "ImageProcessor.Plugins.WebP.nuspec"
};

$imageprocessorWeb = @{
    name    = "ImageProcessor.Web"
    version = "4.12.1"
    folder  = Join-Path $buildPath "src\ImageProcessor.Web"
    output  = Join-Path $binPath "ImageProcessor.Web\lib\net452"
    csproj  = "ImageProcessor.Web.csproj"
    nuspec  = Join-Path $nuspecsPath "ImageProcessor.Web.nuspec"
};

$imageprocessorWebConfig = @{
    version = "2.6.0"
    nuspec  = Join-Path $nuspecsPath "ImageProcessor.Web.Config.nuspec"
};

$imageProcessorWebPluginsAzureBlobCache = @{
    name    = "ImageProcessor.Web.Plugins.AzureBlobCache"
    version = "1.7.0"
    folder  = Join-Path $buildPath "src\ImageProcessor.Web.Plugins.AzureBlobCache"
    output  = Join-Path $binPath "ImageProcessor.Web.Plugins.AzureBlobCache\lib\net452"
    csproj  = "ImageProcessor.Web.Plugins.AzureBlobCache.csproj"
    nuspec  = Join-Path $nuspecsPath "ImageProcessor.Web.Plugins.AzureBlobCache.nuspec"
};

$imageProcessorWebPluginsAmazonS3Cache = @{
    name    = "ImageProcessor.Web.Plugins.AmazonS3Cache"
    version = "1.2.0"
    folder  = Join-Path $buildPath "src\ImageProcessor.Web.Plugins.AmazonS3Cache"
    output  = Join-Path $binPath "ImageProcessor.Web.Plugins.AmazonS3Cache\lib\net452"
    csproj  = "ImageProcessor.Web.Plugins.AmazonS3Cache.csproj"
    nuspec  = Join-Path $nuspecsPath "ImageProcessor.Web.Plugins.AmazonS3Cache.nuspec"
};

$imageProcessorWebPluginsPostProcessor = @{
    name    = "ImageProcessor.Web.Plugins.PostProcessor"
    version = "2.0.0"
    folder  = Join-Path $buildPath "src\ImageProcessor.Web.Plugins.PostProcessor"
    output  = Join-Path $binPath "ImageProcessor.Web.Plugins.PostProcessor\lib\net452"
    csproj  = "ImageProcessor.Web.Plugins.PostProcessor.csproj"
    nuspec  = Join-Path $nuspecsPath "ImageProcessor.Web.PostProcessor.nuspec"
};

$projects = @(
    $imageprocessor, 
    $imageProcessorPluginsCair,
    $imageProcessorPluginsWebP,
    $imageprocessorWeb,
    $imageprocessorWebConfig,
    $imageProcessorWebPluginsAzureBlobCache,
    $imageProcessorWebPluginsAmazonS3Cache,
    $imageProcessorWebPluginsPostProcessor
);

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

# Restore all packages, loop through our projects, patch, build, and pack.
Invoke-Expression "nuget restore $(Join-Path $buildPath "ImageProcessor.sln")"

# Patch and Build
Write-Host "Building Projects" -ForegroundColor Magenta;

foreach ($project in $projects) {

    if ($project.csproj -eq $null -or $project.csproj -eq "") {

        continue;
    }

    $assemblyVersion = "$($project.version).$buildNumber";
    Write-Host "Building Project $($project.name) at version $($assemblyVersion)" -ForegroundColor Yellow;
    Update-AssemblyInfo -file (Join-Path $project.folder "Properties\AssemblyInfo.cs") -version $assemblyVersion;

    $buildCommand = "msbuild $(Join-Path $project.folder $project.csproj) /t:Build /p:Warnings=true /p:Configuration=Release /p:Platform=AnyCPU /p:PipelineDependsOnBuild=False /p:OutDir=$($project.output) /clp:WarningsOnly /clp:ErrorsOnly /clp:Summary /clp:PerformanceSummary /v:Normal /nologo";
    Write-Host $buildCommand -ForegroundColor Yellow;
    Invoke-Expression $buildCommand;
}

#Test 
Write-Host "Building Tests" -ForegroundColor Magenta;
foreach ($testProject in $testProjects) {

    $testBuildCommand = "msbuild $($testProject) /t:Build /p:Configuration=Release /p:Platform=""AnyCPU"" /p:Warnings=true /clp:WarningsOnly /clp:ErrorsOnly /v:Normal /nologo"
    Write-Host "Building project $($testProject)" -ForegroundColor Yellow;
    Invoke-Expression $testBuildCommand;
}

# Pack
Write-Host "Packing Artifacts" -ForegroundColor Magenta;
foreach ($project in $projects) {

    $packCommand = "nuget pack $($project.nuspec) -OutputDirectory $($nugetOutput) -Version $($project.version)";
    Write-Host $packCommand -ForegroundColor Yellow;
    Invoke-Expression $packCommand;
}