Getting Started
================================
Wave is a C# library that extends the ESRI ArcObjects and Schneider Electric ArcFM APIs that are used for developing Geographical Information System (GIS) extensions to the ArcMap and ArcFM software or stand-alone applications. The library uses extension methods to extend the functionality of existing objects and provide workable wrappers around commonly used COM objects.

The concept is to eliminate the need for the developer to learn new namespaces, but allow them to take advantage of the Visual Studio IDE to identify the new features for objects.

.. note::
    It's always best to consult the :doc:`apidocs` documentation to understand the purpose of the methods and classes provided by Wave.

There are two ways for getting started using Wave.

GitHub
---------------------
You can visit `GitHub <https://github.com/Jumpercables/Wave>`_ to download and compiled the source. Once compiled you can reference the assemblies in your projects.



Microsoft Package Manager
--------------------------------------
Wave is publicly distributed using the `Nuget <http://www.nuget.org>`_ service that is available to the .NET community within Visual Studios.

- To install Wave [9.3], run the following command in the Package Manager Console.

.. code-block::

	PM> Install-Package Wave.9.3 -Version 1.0.1


Requirements
--------------------
The prerequisites for developing and using Wave requires both open source and commercial software.

- `ArcGIS Desktop 9.3.1 <http://www.esri.com/software/arcgis>`_
- `ArcFM Solution 9.3.1 <http://www.schneider-electric.com/products/ww/en/6100-network-management-software/6120-geographic-information-system-arcfm-solution/62051-arcfm/>`_
- `Apache log4net 2.0.3 <https://github.com/apache/log4net>`_
- `Microsoft .NET Framework 3.5 SP1 <http://www.microsoft.com/en-us/download/details.aspx?id=22>`_
