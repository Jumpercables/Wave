# Overview
[Wave](https://github.com/Jumpercables/Wave) is a C# library that extends the ArcGIS for Desktop and ArcFM Solution APIs in an effort to simplify customizing these products. The library has been designed to use extension methods to expose the features of the library.

**Wave Extensions for ArcGIS** - The package for the ArcGIS for Desktop extensions.

[![Wave Extensions for ArcGIS](https://buildstats.info/nuget/Wave.Extensions.Esri)](https://www.nuget.org/packages/Wave.Extensions.Esri/)

**Wave Extensions for ArcFM** - The package for the ArcFM Solution extensions.

[![Wave Extensions for ArcFM](https://buildstats.info/nuget/Wave.Extensions.Miner)](https://www.nuget.org/packages/Wave.Extensions.Miner/)

## Documentation
You can [download](Wave.v4.5.chm) the latest API documentation in CHM format.

* **Note:** On some systems, the contents of the CHM file is blocked. Right click on the CHM file, select **Properties**, and click on the **Unblock** button if it is present in the lower right corner of the **General** tab in the properties dialog.

## Installation ##
You can download a release of the library using the Microsoft NuGet Package Manager within Visual Studio.

To install **Wave Extensions for ArcGIS**, run the following command in the Package Manager Console.

```
	PM> Install-Package Wave.Extensions.Esri
```

To install **Wave Extensions for ArcFM**, run the following command in the Package Manager Console.

```
	PM> Install-Package Wave.Extensions.Miner
```

* The **Wave.Extensions.Esri** package features are included in the **Wave.Extensions.Miner** package, as the *ArcFM Solution* product is tightly coupled with the *ArcGIS for Desktop* product.

### Requirements ###
- ArcGIS for Desktop 10 (or higher)
- ArcFM Solution 10 (or higher)
- 3.5 SP 1 or 4.5 .NET Framework
- Visual Studio 2010 (or higher)

### Third Party Libraries ###
- log4net 2.0.3
