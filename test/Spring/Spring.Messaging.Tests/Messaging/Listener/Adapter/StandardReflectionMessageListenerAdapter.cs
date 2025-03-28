#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

#region

using System.Reflection;
using Spring.Expressions;
using Spring.Util;

#endregion

namespace Spring.Messaging.Listener.Adapter;

/// <summary>
/// POC for a standard reflection based listener method invoker. 
/// </summary>
/// <author>Mark Pollack</author>
public class StandardReflectionMessageListenerAdapter : MessageListenerAdapter
{
    private const BindingFlags BINDING_FLAGS
        = BindingFlags.Public | BindingFlags.NonPublic
                              | BindingFlags.Instance | BindingFlags.Static
                              | BindingFlags.IgnoreCase;

    protected override object InvokeListenerMethod(string methodName, object[] arguments)
    {
        try
        {
            MethodInfo mi = MethodNode.GetBestMethod(HandlerObject.GetType(), methodName, BINDING_FLAGS, arguments);
            if (mi == null)
            {
                throw new ListenerExecutionFailedException("Failed to invoke the target method '" + methodName +
                                                           "' with arguments " +
                                                           StringUtils.CollectionToCommaDelimitedString(arguments));
            }

            try
            {
                return mi.Invoke(HandlerObject, arguments);
            }
            catch (Exception e)
            {
                throw new ListenerExecutionFailedException("Listener method '" + methodName + "' threw exception.", e);
            }
        }
        catch (Exception e)
        {
            throw new ListenerExecutionFailedException("Failed to invoke the target method '" + methodName +
                                                       "' with arguments " +
                                                       StringUtils.CollectionToCommaDelimitedString(arguments), e);
        }
    }
}