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

namespace Spring.Aop.Support
{
    /// <summary>
    /// Concrete ObjectFactory-based IPointcutAdvisor thta allows for any Advice to be
    /// configured as reference to an Advice object in the ObjectFatory, as well as 
    /// the Pointcut to be configured through an object property.
    /// </summary>
    /// <remarks>
    /// Specifying the name of an advice object instead of the advice object itself
    /// (if running within a ObjectFactory/ApplicationContext) increases loose coupling
    /// at initialization time, in order to not intialize the advice object until the pointcut
    /// actually matches.
    /// </remarks>
    /// <author>Juerge Hoeller</author>
    /// <author>Mark Pollack</author>
    public class DefaultObjectFactoryPointcutAdvisor : AbstractObjectFactoryPointcutAdvisor
    {
        private IPointcut pointcut = TruePointcut.True;


        /// <summary>
        /// The <see cref="Spring.Aop.IPointcut"/> that drives this advisor.
        /// </summary>
        public override IPointcut Pointcut
        {
            get { return pointcut; }
            set { pointcut = (pointcut != null ? value : TruePointcut.True); }
        }

        /// <summary>
        /// Describe this Advisor, showing pointcut and name of advice object.
        /// </summary>
        /// <returns>Type name , pointcut, and advice object name.</returns>
        public override string ToString()
        {
            return GetType().Name + ": pointcut [" + Pointcut + "]; advice object = '" + AdviceObjectName + "'";
        }
    }
}