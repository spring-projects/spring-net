#region License

/*
 * Copyright © 2002-2006 the original author or authors.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using System.Reflection;

[assembly: AssemblyTitle("Spring Calculator remote application")]
[assembly: AssemblyDescription("Remote application for Spring Calculator example")]
[assembly: AssemblyVersion("1.0.0.0")]

#if !NET_2_0
[assembly: AssemblyConfiguration("net-1.1.win32; Release")]
#else
[assembly: AssemblyConfiguration("net-2.0.win32; Release")]
#endif
[assembly: AssemblyCompany("http://www.springframework.net")]
[assembly: AssemblyProduct("Spring.NET Framework")]
[assembly: AssemblyCopyright("Copyright 2002-2006 Spring.NET Framework Team.")]
[assembly: AssemblyTrademark("Apache License, Version 2.0")]