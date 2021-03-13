<h1 align="center">
    <img src="https://raw.githubusercontent.com/JimBobSquarePants/ImageProcessor/develop/build/icons/imageprocessor-logo-256.png" alt="ImageProcessor" width="175"/>
    <br>
    ImageProcessor
    <br>
    <br>
    <a href="https://ci.appveyor.com/project/JamesSouth/imageprocessor/branch/develop" rel="nofollow"><img src="https://ci.appveyor.com/api/projects/status/8ypr7527dnao04yr/branch/develop?svg=true" alt="Build status" data-canonical-src="https://ci.appveyor.com/api/projects/status/8ypr7527dnao04yr/branch/Framework?svg=true" style="max-width:100%;"></a>
<a href="https://huboard.com/JimBobSquarePants/ImageProcessor/" rel="nofollow"><img src="https://img.shields.io/github/issues-raw/JimBobSquarePants/imageprocessor.svg" alt="Issues open" style="max-width:100%;"></a>
<a href="http://sourcebrowser.io/Browse/JimBobSquarePants/ImageProcessor/" rel="nofollow"><img src="https://img.shields.io/badge/Browse-Source-green.svg" alt="Source Browser" style="max-width:100%;"></a>
<a href="https://gitter.im/JimBobSquarePants/ImageProcessor?utm_source=badge&amp;utm_medium=badge&amp;utm_campaign=pr-badge&amp;utm_content=badge" rel="nofollow"><img src="https://badges.gitter.im/Join%20Chat.svg" style="max-width:100%;"></a>
</h1>

⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️

**ImageProcessor is, and will only ever be supported on the .NET Framework running on a Windows OS. Please do not attempt to use with .NET Core or NET 5+**

⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️

**Imageprocessor** is a lightweight, fluent wrapper around System.Drawing.

It's fast, extensible, easy to use, comes bundled with some great features and is fully open source.

For full documentation please see [http://imageprocessor.org/](http://imageprocessor.org/)

## Roadmap
Focus for the ImageProcessor libraries has switched to desktop only due to the [lack of support for System.Drawing on Windows Services and ASP.NET](https://docs.microsoft.com/en-us/dotnet/api/system.drawing?view=netframework-4.8#remarks). As such, the `ImageProcessor.Web`and accompanying libraries will not be further developed. For an alternative please use [`ImageSharp.Web`](https://github.com/SixLabors/ImageSharp.Web).

### Latest Releases
| Library                                       | Version                                                                                                                                                       |
| :-------------------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **ImageProcessor**                            | [![NuGet](https://buildstats.info/nuget/ImageProcessor)](https://www.nuget.org/packages/ImageProcessor)                                                       |
| **ImageProcessor.Plugins.WebP**               | [![NuGet](https://buildstats.info/nuget/ImageProcessor.Plugins.WebP)](https://www.nuget.org/packages/ImageProcessor.Plugins.WebP)                             |


## Documentation

ImageProcessor's documentation, included in this repo in the gh-pages branch, is built with [Jekyll](http://jekyllrb.com) and publicly hosted on GitHub Pages at <http://imageprocessor.org>. The docs may also be run locally.

### Running documentation locally
1. If necessary, [install Jekyll](http://jekyllrb.com/docs/installation) (requires v2.5.3x).
  - **Windows users:** Read [this unofficial guide](https://github.com/juthilo/run-jekyll-on-windows/) to get Jekyll up and running without problems. 
2. From the root `/ImageProcessor` directory, run `jekyll serve` in the command line.
3. Open <http://localhost:4000> in your browser to navigate to your site.
Learn more about using Jekyll by reading its [documentation](http://jekyllrb.com/docs/home/).

### The ImageProcessor Team

Grand High Eternal Dictator
- [James Jackson-South](https://github.com/jimbobsquarepants)
