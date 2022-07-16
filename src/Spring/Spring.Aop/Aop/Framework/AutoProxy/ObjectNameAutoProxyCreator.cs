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

using System.Collections;
using Spring.Objects.Factory;
using Spring.Util;

namespace Spring.Aop.Framework.AutoProxy
{
    /// <summary>
    /// AutoProxyCreator that identifies objects to proxy via a list of names.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Auto proxy creator that identifies objects to proxy via a list of names.
    ///	Checks for direct, "xxx*", "*xxx" and "*xxx*" matches.
    /// </para>
    /// <para>In case of a IFactoryObject, only the objects created by the
    /// FactoryBean will get proxied.  If you intend to proxy a IFactoryObject instance itself
    /// specify the object name of the IFactoryObject including
    /// the factory-object prefix "&amp;"  e.g. "&amp;MyFactoryObject".
    /// </para>
    /// </remarks>
    /// <seealso cref="Spring.Aop.Framework.AutoProxy.ObjectNameAutoProxyCreator.IsMatch"/>
    /// <author>Juergen Hoeller</author>
    /// <author>Adhari C Mahendra (.NET)</author>
    /// <author>Erich Eichinger</author>
    public class ObjectNameAutoProxyCreator : AbstractFilteringAutoProxyCreator
    {
        private IList objectNames;

        /// <summary>
        /// Initializes a new instance of <see cref="ObjectNameAutoProxyCreator"/>.
        /// </summary>
        public ObjectNameAutoProxyCreator()
        {}

        /// <summary>
        /// Set the names of the objects in IList fashioned way that should automatically
        /// get wrapped with proxies.
        /// A name can specify a prefix to match by ending with "*", e.g. "myObject,tx*"
        /// will match the object named "myObject" and all objects whose name start with "tx".
        /// </summary>
        public IList ObjectNames
        {
            set
            {
                AssertUtils.ArgumentHasElements(value, "ObjectNames");
                objectNames = value;
            }
            get
            {
                return objectNames;
            }
        }

        /// <summary>
        /// Identify as object to proxy if the object name is in the configured list of names.
        /// </summary>
        protected override bool IsEligibleForProxying( Type targetType, string targetName )
        {
            bool shallProxy = IsObjectNameMatch(targetType, targetName, this.ObjectNames);
            return shallProxy;
        }

        /// <summary>
        /// Return if the given object name matches the mapped name.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The default implementation checks for "xxx*", "*xxx" and "*xxx*" matches,
        /// as well as direct equality. Can be overridden in subclasses.
        /// </p>
        /// </remarks>
        /// <param name="objectName">the object name to check</param>
        /// <param name="mappedName">the name in the configured list of names</param>
        /// <returns>if the names match</returns>
        protected virtual bool IsMatch( string objectName, string mappedName )
        {
            return PatternMatchUtils.SimpleMatch( mappedName, objectName );
        }

        /// <summary>
        /// Convenience method that may be used by derived classes. Iterates over the list of <paramref name="objectNamePatterns"/> to match <paramref name="objectName"/> against.
        /// </summary>
        /// <param name="objType">the object's type. Must not be <c>null</c>.</param>
        /// <param name="objectName">the name of the object Must not be <c>null</c>.</param>
        /// <param name="objectNamePatterns">the list of patterns, that <paramref name="objectName"/> shall be matched against. Must not be <c>null</c>.</param>
        /// <returns>
        /// If <paramref name="objectNamePatterns"/> is <c>null</c>, will always return <c>true</c>, otherwise
        /// if <paramref name="objectName"/> matches any of the patterns specified in <paramref name="objectNamePatterns"/>.
        /// </returns>
        protected bool IsObjectNameMatch(Type objType, string objectName, IList objectNamePatterns)
        {
            AssertUtils.ArgumentNotNull(objType, "objType");
            AssertUtils.ArgumentNotNull(objectName, "objectName" );
            AssertUtils.ArgumentNotNull(objectNamePatterns, "objectNamePatterns");

            for (int i = 0; i < objectNamePatterns.Count; i++)
            {
                string mappedName = (string)objectNamePatterns[i];
                if (typeof(IFactoryObject).IsAssignableFrom(objType))
                {
                    if (!objectName.StartsWith(ObjectFactoryUtils.FactoryObjectPrefix))
                    {
                        continue;
                    }
                    mappedName = mappedName.Substring(ObjectFactoryUtils.FactoryObjectPrefix.Length);
                }
                if (IsMatch(objectName, mappedName))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
