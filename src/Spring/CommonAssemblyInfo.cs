using System;
using System.Reflection;
[assembly: CLSCompliant(false)]

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//

[assembly: AssemblyConfiguration("net-2.0.win32; Release")]
[assembly: AssemblyCompany("http://www.springframework.net")]
[assembly: AssemblyProduct("Spring.NET Framework 1.3.1")]
[assembly: AssemblyCopyright("Copyright 2002-2011 Spring.NET Framework Team.")]
[assembly: AssemblyTrademark("Apache License, Version 2.0")]
[assembly: AssemblyCulture("")]

//
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      .NET Framework Version (RR)
//      Revision = "1" for builds with VS.NET, nant build is # of days since 'project.year' 
//      property
//
//
// This is to support side-by-side deployment of .NET 1.1 and .NET 2.0 versions of the assembly.

//note that this #if #elif #endif contstruct requires NET_#_# values be checked in DESCENDING order to work properly
// (e.g., add any new ones at the TOP of this construct!)

#if NET_4_0
[assembly: AssemblyVersion("2.0.0.45001")]
#elif NET_4_0
[assembly: AssemblyVersion("2.0.0.40001")]
#elif NET_3_5
[assembly: AssemblyVersion("2.0.0.35001")]
#endif














//
// In order to sign your assembly you must specify a key to use. Refer to the 
// Microsoft .NET Framework documentation for more information on assembly signing.
//
// Use the attributes below to control which key is used for signing. 
//
// Notes: 
//   (*) If no key is specified, the assembly is not signed.
//   (*) KeyName refers to a key that has been installed in the Crypto Service
//       Provider (CSP) on your machine. KeyFile refers to a file which contains
//       a key.
//   (*) If the KeyFile and the KeyName values are both specified, the 
//       following processing occurs:
//       (1) If the KeyName can be found in the CSP, that key is used.
//       (2) If the KeyName does not exist and the KeyFile does exist, the key 
//           in the KeyFile is installed into the CSP and used.
//   (*) In order to create a KeyFile, you can use the sn.exe (Strong Name) utility.
//       When specifying the KeyFile, the location of the KeyFile should be
//       relative to the project output directory which is
//       %Project Directory%\obj\<configuration>. For example, if your KeyFile is
//       located in the project directory, you would specify the AssemblyKeyFile 
//       attribute as [assembly: AssemblyKeyFile("..\\..\\mykey.snk")]
//   (*) Delay Signing is an advanced option - see the Microsoft .NET Framework
//       documentation for more information on this.
//
#if STRONG
[assembly: AssemblyDelaySign(false)]
#endif
