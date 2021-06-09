﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
#if DEBUG
using System.Windows.Markup;
#endif

[assembly: AssemblyTitle("OpenSky.AgentMSFS")]
[assembly: AssemblyDescription("OpenSky Agent for Flight Simulator 2020")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("OpenSky")]
[assembly: AssemblyProduct("OpenSky")]
[assembly: AssemblyCopyright("sushi.at for OpenSky")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]
[assembly: AssemblyVersion("1.6.0.0")]
[assembly: AssemblyFileVersion("1.6.0.0")]

// This allows us to detect debug mode in XAML
#if DEBUG
[assembly: XmlnsDefinition("debug-mode", "Namespace")]
#endif