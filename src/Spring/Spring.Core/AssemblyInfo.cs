#region License

/*
 * Copyright © 2002-2009 the original author or authors.
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
using System.Runtime.InteropServices;
using System.Security;

[assembly: ComVisible(false)]
[assembly: AssemblyTitle("Spring.Core")]
[assembly: AssemblyDescription("Core functionality for Spring.Net IoC container")]

//
// Security Permissions
//
// we need full, unrestricted access to reflection metadata...
//[assembly: ReflectionPermission(SecurityAction.RequestMinimum, Unrestricted = true)]
//[assembly: AssemblyKeyFile(@"C:\users\aseovic\projects\OpenSource\Spring.Net\Spring.Net.PrivateKey.keys")]
//[assembly: AssemblyKeyFile(@"C:\projects\Spring.Net\Spring.Net.snk")]
[assembly: AllowPartiallyTrustedCallers]

[assembly: SecurityCritical]

#if NET_1_0 || NET_1_1
namespace System.Security
{
    ///<summary>
    ///</summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Method)]
    internal class SecurityCriticalAttribute : Attribute
    { }
    [AttributeUsage(AttributeTargets.Method)]
    internal class SecurityTreatAsSafeAttribute : Attribute
    { }
}
#endif
