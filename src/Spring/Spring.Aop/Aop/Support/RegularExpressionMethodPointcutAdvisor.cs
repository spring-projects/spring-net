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

#region Imports

using AopAlliance.Aop;
using Spring.Util;

#endregion

namespace Spring.Aop.Support
{
    /// <summary>
    /// Convenient class for regular expression method pointcuts that hold an
    /// <see cref="AopAlliance.Aop.IAdvice"/>, making them an
    /// <see cref="Spring.Aop.IAdvisor"/>.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Configure this class using the <see cref="Pattern"/> and
    /// <see cref="Patterns"/> pass-through properties. These are analogous
    /// to the <see cref="AbstractRegularExpressionMethodPointcut.Pattern"/> and
    /// <see cref="AbstractRegularExpressionMethodPointcut.Pattern"/>s properties of the
    /// <see cref="AbstractRegularExpressionMethodPointcut"/> class.
    /// </p>
    /// <p>
    /// Can delegate to any type of regular expression pointcut. Currently only
    /// pointcuts based on the regular expression classes from the .NET Base
    /// Class Library are supported. The
    /// <see cref="Spring.Aop.Support.DefaultPointcutAdvisor.Pointcut"/>
    /// property must be a subclass of the
    /// <see cref="AbstractRegularExpressionMethodPointcut"/> class.
    /// </p>
    /// <p>
    /// This should not normally be set directly.
    /// </p>
    /// </remarks>
    /// <author>Dmitriy Kopylenko</author>
    /// <author>Rod Johnson</author>
    /// <author>Simon White (.NET)</author>
    /// <author>Mark Pollack (.NET)</author>
    /// <see cref=" SdkRegularExpressionMethodPointcut"/>
    [Serializable]
    public class RegularExpressionMethodPointcutAdvisor : AbstractGenericPointcutAdvisor
    {
        private AbstractRegularExpressionMethodPointcut pointcut;

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="RegularExpressionMethodPointcutAdvisor"/>
        /// class.
        /// </summary>
        public RegularExpressionMethodPointcutAdvisor()
        {
            InitPointcut();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="RegularExpressionMethodPointcutAdvisor"/>
        /// class for the supplied <paramref name="advice"/>.
        /// </summary><param name="advice">
        /// The target advice.
        /// </param>
        public RegularExpressionMethodPointcutAdvisor(IAdvice advice)
        {
            Advice = advice;
            InitPointcut();
        }

        #endregion

        #region Properties

        /// <summary>
        /// A single pattern to be used during method evaluation.
        /// </summary>
        public string Pattern
        {
            set
            {
                AbstractRegularExpressionMethodPointcut armp = (AbstractRegularExpressionMethodPointcut) Pointcut;
                armp.Pattern = value;
            }
        }

        /// <summary>
        /// Multiple patterns to be used during method evaluation.
        /// </summary>
        public string[] Patterns
        {
            set
            {
                AbstractRegularExpressionMethodPointcut armp = (AbstractRegularExpressionMethodPointcut) Pointcut;
                armp.Patterns = value;
            }
        }

        /// <summary>
        /// The <see cref="Spring.Aop.IPointcut"/> that drives this advisor.
        /// </summary>
        public override IPointcut Pointcut
        {
            get { return pointcut; }
            set {
                AssertUtils.AssertArgumentType(value, "pointcut", typeof(AbstractRegularExpressionMethodPointcut),
                                              "Pointcut most be compatible with type AbstractRegularExpressionMethodPointuct");
                pointcut = value as AbstractRegularExpressionMethodPointcut;
            }
        }

        #endregion

        /// <summary>
        /// Initialises the pointcut.
        /// </summary>
        protected void InitPointcut()
        {
            pointcut = new SdkRegularExpressionMethodPointcut();
        }
    }
}
