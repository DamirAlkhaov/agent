﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
#if DEBUG
using System.Windows.Markup;
#endif

[assembly: AssemblyTitle("OpenSky.Agent")]
[assembly: AssemblyDescription("OpenSky Flight Tracking Agent")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("OpenSky")]
[assembly: AssemblyProduct("OpenSky")]
[assembly: AssemblyCopyright("OpenSky project 2021-2023")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]
[assembly: AssemblyVersion("0.5.4")]
[assembly: AssemblyFileVersion("0.5.4")]

// This allows us to detect debug mode in XAML
#if DEBUG
[assembly: XmlnsDefinition("debug-mode", "Namespace")]
#endif