/*
 * Copyright © 2002-2026 the original author or authors.
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

using Spring.Reflection.Dynamic;

namespace Spring.Expressions;

/// <summary>
/// Represents a single extension method or extension property registered for a type
/// via <see cref="Expression.RegisterExtensionMethod(Type, string, Delegate, System.Collections.Generic.IDictionary{string, object})"/>
/// and its sibling overloads. The implementation is either a parsed lambda expression
/// or a .NET delegate; in both cases the receiver is passed as the first argument.
/// </summary>
[Serializable]
internal sealed class ExpressionExtension
{
    private readonly Type targetType;
    private readonly string name;
    private readonly LambdaExpressionNode lambdaExpression;
    private readonly Delegate callback;
    private readonly int parameterCount;

    public ExpressionExtension(Type targetType, string name, LambdaExpressionNode lambdaExpression)
    {
        this.targetType = targetType;
        this.name = name;
        this.lambdaExpression = lambdaExpression;
        this.parameterCount = lambdaExpression.ArgumentNames.Length;
    }

    public ExpressionExtension(Type targetType, string name, Delegate callback)
    {
        this.targetType = targetType;
        this.name = name;
        this.callback = callback;
        this.parameterCount = callback.Method.GetParameters().Length;
    }

    /// <summary>
    /// Gets the type this extension was registered for.
    /// </summary>
    public Type TargetType
    {
        get { return targetType; }
    }

    /// <summary>
    /// Gets the member name this extension was registered under.
    /// </summary>
    public string Name
    {
        get { return name; }
    }

    /// <summary>
    /// Gets the lambda expression implementing this extension, or <c>null</c> for the delegate form.
    /// </summary>
    public LambdaExpressionNode LambdaExpression
    {
        get { return lambdaExpression; }
    }

    /// <summary>
    /// Ensures the number of invocation arguments (including the receiver) matches the
    /// registered implementation.
    /// </summary>
    /// <exception cref="ArgumentException">If the argument count does not match.</exception>
    public void AssertArgumentCount(int argumentCount)
    {
        if (argumentCount != parameterCount)
        {
            throw new ArgumentException(string.Format(
                "Extension '{0}' registered for type '{1}' expects {2} argument(s) including the receiver, but was called with {3}.",
                name, targetType.FullName, parameterCount, argumentCount));
        }
    }

    /// <summary>
    /// Invokes the delegate form of this extension. Only valid when <see cref="LambdaExpression"/> is <c>null</c>.
    /// </summary>
    public object InvokeCallback(object[] arguments)
    {
        return new SafeMethod(callback.Method).Invoke(callback.Target, arguments);
    }
}
