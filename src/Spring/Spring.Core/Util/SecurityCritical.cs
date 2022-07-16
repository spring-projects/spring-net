#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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

#if !NETSTANDARD
using System.Runtime.CompilerServices;
using System.Security;

namespace Spring.Util
{
    /// <summary>
    /// Utility class to be used from within this assembly for executing security critical code
    /// NEVER EVER MAKE THIS PUBLIC!
    /// </summary>
    /// <author>Erich Eichinger</author>
    internal class SecurityCritical
    {
        [SecurityCritical, SecurityTreatAsSafe]
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ExecutePrivileged(IStackWalk permission, Action callback)
        {
            permission.Assert();
            try
            {
                callback();
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
        }
    }
}
#endif
