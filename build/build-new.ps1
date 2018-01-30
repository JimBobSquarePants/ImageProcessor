$appveyorBuildNumber = "$env:APPVEYOR_BUILD_NUMBER";

$buildPath = Resolve-Path ".";
$srcPath = Join-Path $buildPath ".\src";
$nuspecPath = Join-Path $buildPath ".\build\nuspecs"
$binPath = Join-Path $buildPath "_buildoutput"

Write-Debug $buildPath;
Write-Debug $srcPath;

$pad = 6;

# Build number. Appveyor build number. Set by CI and padded
$buildNumber = $appveyorBuildNumber.Trim().Trim('0').PadLeft($pad, "0");

# Our project objects
$imageprocessor = @{
    name    = "ImageProcessor"
    version = "2.5.6"
    folder  = Join-Path $buildPath "\src\ImageProcessor"
    output  = Join-Path $binPath "\ImageProcessor\lib\net45"
    csproj  = "ImageProcessor.csproj"
    nuspec  = "ImageProcessor.nuspec"
};

$projects = @($imageprocessor);

# Calculates the correct version number based on the branch and build number
function Get-VersionNumber([string]$version) {
    if ("$env:APPVEYOR_PULL_REQUEST_NUMBER" -ne "") {
        Write-Host "building a PR"

        $prNumber = "$env:APPVEYOR_PULL_REQUEST_NUMBER".Trim().Trim('0').PadLeft($pad - 1, "0");

        # This is a PR
        $version = "${version}-pr${prNumber}${buildNumber}";
    }
    else {
        Write-Host "building a branch commit";

        # This is a general branch commit
        $branch = $env:APPVEYOR_REPO_BRANCH

        if ("$branch" -eq "") {
            $branch = ((git rev-parse --abbrev-ref HEAD) | Out-String).Trim()

            if ("$branch" -eq "") {
                $branch = "unknown"
            }
        }

        Write-Host $branch;

        $branch = $branch.Replace("/", "-").ToLower()

        if ($branch.ToLower() -eq "master") {
            $version = "${version}.${buildNumber}";
        }
        elseif ($branch.ToLower() -eq "develop") {
            $branch = "dev"
            $version = "${version}-dev${buildNumber}";
        }
        else {
            $version = "${version}-${branch}${buildNumber}";
        }   
        
        return $version;
    }
}

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

    $version = Get-VersionNumber $project.version;
    Write-Host "Building project $($project.name) at version $($version)";
    Update-AssemblyInfo -file (Join-Path $project.folder "Properties\AssemblyInfo.cs") -version $version;

    $buildCommand = "msbuild $(Join-Path $project.folder $project.csproj) /t:Build /p:Warnings=true /p:Configuration=Release /p:PipelineDependsOnBuild=False /p:OutDir=$($project.output) /clp:WarningsOnly /clp:ErrorsOnly /clp:Summary /clp:PerformanceSummary /v:Normal /nologo";
    Write-Host $buildCommand;
    Invoke-Expression $buildCommand;

    #TODO: Nuget
}