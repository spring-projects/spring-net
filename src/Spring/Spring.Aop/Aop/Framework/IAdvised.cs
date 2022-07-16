#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

using AopAlliance.Aop;

using Spring.Proxy;

namespace Spring.Aop.Framework
{
    /// <summary>
    /// Configuration data for an AOP proxy factory.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This configuration includes the
    /// <see cref="AopAlliance.Intercept.IInterceptor"/>s,
    /// <see cref="Spring.Aop.IAdvisor"/>s, and (any) proxied interfaces.
    /// </p>
    /// <p>
    /// Any AOP proxy obtained from Spring.NET can be cast to this interface to
    /// allow the manipulation of said proxy's AOP advice.
    /// </p>
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Aleksandar Seovic (.NET)</author>
    /// <seealso cref="Spring.Aop.Framework.AdvisedSupport"/>
    [ProxyIgnore]
    public interface IAdvised
    {
        /// <summary>
        /// Should proxies obtained from this configuration expose
        /// the AOP proxy to the
        /// <see cref="Spring.Aop.Framework.AopContext"/> class?
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is useful if an advised object needs to call another advised
        /// method on itself. (If it uses the <c>this</c> reference (<c>Me</c>
        /// in Visual Basic.NET), the invocation will <b>not</b> be advised).
        /// </p>
        /// </remarks>
        bool ExposeProxy { get; }

        /// <summary>
        /// Gets the
        /// <see cref="Spring.Aop.Framework.IAdvisorChainFactory"/>
        /// implementation that will be used to get the interceptor
        /// chains for the advised
        /// <see cref="Spring.Aop.Framework.AdvisedSupport.Target"/>.
        /// </summary>
        /// <value>
        /// The <see cref="Spring.Aop.Framework.IAdvisorChainFactory"/>
        /// implementation that will be used to get the interceptor
        /// chains for the advised
        /// <see cref="Spring.Aop.Framework.AdvisedSupport.Target"/>.
        /// </value>
        IAdvisorChainFactory AdvisorChainFactory { get; }

        /// <summary>
        /// Is the target <see cref="System.Type"/> to be proxied in addition
        /// to any interfaces declared on the proxied <see cref="System.Type"/>?
        /// </summary>
        bool ProxyTargetType { get; }

        /// <summary>
        /// Is target type attributes, method attributes, method's return type attributes
        /// and method's parameter attributes to be proxied in addition
        /// to any interfaces declared on the proxied <see cref="System.Type"/>?
        /// </summary>
        bool ProxyTargetAttributes { get; }

        /// <summary>
        /// Returns the collection of <see cref="Spring.Aop.IAdvisor"/>
        /// instances that have been applied to this proxy.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Will never return <cref lang="null"/>, but may return an
        /// empty array (in the case where no
        /// <see cref="Spring.Aop.IAdvisor"/> instances have been applied to
        /// this proxy).
        /// </p>
        /// </remarks>
        /// <value>
        /// The collection of <see cref="Spring.Aop.IAdvisor"/>
        /// instances that have been applied to this proxy.
        /// </value>
        IList<IAdvisor> Advisors { get; }

        /// <summary>
        /// Returns the collection of <see cref="Spring.Aop.IIntroductionAdvisor"/>
        /// instances that have been applied to this proxy.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Will never return <cref lang="null"/>, but may return an
        /// empty array (in the case where no
        /// <see cref="Spring.Aop.IIntroductionAdvisor"/> instances have been
        /// applied to this proxy).
        /// </p>
        /// </remarks>
        /// <value>
        /// The collection of <see cref="Spring.Aop.IIntroductionAdvisor"/>
        /// instances that have been applied to this proxy.
        /// </value>
        IList<IIntroductionAdvisor> Introductions { get; }

        /// <summary>
        /// Returns the collection of interface <see cref="System.Type"/>s
        /// to be (or that are being) proxied by this proxy.
        /// </summary>
        /// <value>
        /// The collection of interface <see cref="System.Type"/>s
        /// to be (or that are being) proxied by this proxy.
        /// </value>
        IList<Type> Interfaces { get; }

        /// <summary>
        /// Returns the mapping of the proxied interface
        /// <see cref="System.Type"/>s to their delegates.
        /// </summary>
        /// <value>
        /// The mapping of the proxied interface
        /// <see cref="System.Type"/>s to their delegates.
        /// </value>
        IDictionary<Type, object> InterfaceMap { get; }

        /// <summary>
        /// Is this configuration frozen?
        /// </summary>
        /// <remarks>
        /// <p>
        /// When a config is frozen, no advice changes can be made. This is
        /// useful for optimization, and useful when we don't want callers
        /// to be able to manipulate configuration after casting to
        /// <see cref="Spring.Aop.Framework.IAdvised"/>.
        /// </p>
        /// </remarks>
        bool IsFrozen { get; }

        /// <summary>
        /// Returns the <see cref="Spring.Aop.ITargetSource"/> used by this
        /// <see cref="Spring.Aop.Framework.IAdvised"/> object.
        /// </summary>
        /// <value>
        /// The <see cref="Spring.Aop.ITargetSource"/> used by this
        /// <see cref="Spring.Aop.Framework.IAdvised"/> object.
        /// </value>
        ITargetSource TargetSource { get; }

        /// <summary>
        /// Returns a boolean specifying if this <see cref="IAdvised"/>
        /// instance can be serialized.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can be serialized, <c>false</c> otherwise.
        /// </value>
        bool IsSerializable { get; }

        /// <summary>
        /// Adds the supplied <paramref name="advice"/> to the end (or tail)
        /// of the advice (interceptor) chain.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Please be aware that Spring.NET's AOP implementation only supports
        /// method advice (as encapsulated by the
        /// <see cref="AopAlliance.Intercept.IMethodInterceptor"/> interface).
        /// </p>
        /// </remarks>
        /// <param name="advice">
        /// The <see cref="AopAlliance.Aop.IAdvice"/> to be added.
        /// </param>
        /// <seealso cref="Spring.Aop.Support.DefaultPointcutAdvisor"/>
        /// <seealso cref="Spring.Aop.Framework.IAdvised.AddAdvice(int,IAdvice)"/>
        void AddAdvice(IAdvice advice);

        /// <summary>
        /// Adds the supplied <paramref name="advice"/> to the supplied
        /// <paramref name="position"/> in the advice (interceptor) chain.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Please be aware that Spring.NET's AOP implementation only supports
        /// method advice (as encapsulated by the
        /// <see cref="AopAlliance.Intercept.IMethodInterceptor"/> interface).
        /// </p>
        /// </remarks>
        /// <param name="position">
        /// The zero (0) indexed position (from the head) at which the
        /// supplied <paramref name="advice"/> is to be inserted into the
        /// advice (interceptor) chain.
        /// </param>
        /// <param name="advice">
        /// The <see cref="AopAlliance.Aop.IAdvice"/> to be added.
        /// </param>
        /// <seealso cref="Spring.Aop.Support.DefaultPointcutAdvisor"/>
        /// <seealso cref="Spring.Aop.Framework.IAdvised.AddAdvice(IAdvice)"/>
        void AddAdvice(int position, IAdvice advice);

        /// <summary>
        /// Is the supplied <paramref name="intf"/> (interface)
        /// <see cref="System.Type"/> proxied?
        /// </summary>
        /// <param name="intf">
        /// The interface <see cref="System.Type"/> to test.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the supplied <paramref name="intf"/>
        /// (interface) <see cref="System.Type"/> is proxied;
        /// <see langword="false"/> if not or the supplied
        /// <paramref name="intf"/> is <cref lang="null"/>.
        /// </returns>
        bool IsInterfaceProxied(Type intf);

        /// <summary>
        /// Adds the advisors from the supplied <paramref name="advisors"/>
        /// to the list of <see cref="Spring.Aop.Framework.IAdvised.Advisors"/>.
        /// </summary>
        /// <param name="advisors">
        /// The <see cref="IAdvisors"/> to add advisors from.
        /// </param>
        /// <exception cref="AopConfigException">
        /// If this proxy configuration is frozen and the
        /// <paramref name="advisors"/> cannot be added.
        /// </exception>
        void AddAdvisors(IAdvisors advisors);

        /// <summary>
        /// Adds the supplied <paramref name="advisor"/> to the list
        /// of <see cref="Spring.Aop.Framework.IAdvised.Advisors"/>.
        /// </summary>
        /// <param name="advisor">
        /// The <see cref="Spring.Aop.IAdvisor"/> to add.
        /// </param>
        /// <exception cref="AopConfigException">
        /// If this proxy configuration is frozen and the
        /// <paramref name="advisor"/> cannot be added.
        /// </exception>
        void AddAdvisor(IAdvisor advisor);

        /// <summary>
        /// Adds the supplied <paramref name="advisor"/> to the list
        /// of <see cref="Spring.Aop.Framework.IAdvised.Advisors"/>.
        /// </summary>
        /// <param name="index">
        /// The index in the <see cref="Spring.Aop.Framework.IAdvised.Advisors"/>
        /// list at which the supplied <paramref name="advisor"/>
        /// is to be inserted.
        /// </param>
        /// <param name="advisor">
        /// The <see cref="Spring.Aop.IIntroductionAdvisor"/> to add.
        /// </param>
        /// <exception cref="AopConfigException">
        /// If this proxy configuration is frozen and the
        /// <paramref name="advisor"/> cannot be added.
        /// </exception>
        void AddAdvisor(int index, IAdvisor advisor);

        /// <summary>
        /// Adds the supplied <paramref name="introductionAdvisor"/> to the list
        /// of <see cref="Spring.Aop.Framework.IAdvised.Introductions"/>.
        /// </summary>
        /// <param name="introductionAdvisor">
        /// The <see cref="Spring.Aop.IIntroductionAdvisor"/> to add.
        /// </param>
        /// <exception cref="AopConfigException">
        /// If this proxy configuration is frozen and the
        /// <paramref name="introductionAdvisor"/> cannot be added.
        /// </exception>
        void AddIntroduction(IIntroductionAdvisor introductionAdvisor);

        /// <summary>
        /// Adds the supplied <paramref name="introductionAdvisor"/> to the list
        /// of <see cref="Spring.Aop.Framework.IAdvised.Introductions"/>.
        /// </summary>
        /// <param name="index">
        /// The index in the <see cref="Spring.Aop.Framework.IAdvised.Introductions"/>
        /// list at which the supplied <paramref name="introductionAdvisor"/>
        /// is to be inserted.
        /// </param>
        /// <param name="introductionAdvisor">
        /// The <see cref="Spring.Aop.IIntroductionAdvisor"/> to add.
        /// </param>
        /// <exception cref="AopConfigException">
        /// If this proxy configuration is frozen and the
        /// <paramref name="introductionAdvisor"/> cannot be added.
        /// </exception>
        void AddIntroduction(int index, IIntroductionAdvisor introductionAdvisor);

        /// <summary>
        /// Return the index (0 based) of the supplied
        /// <see cref="Spring.Aop.IAdvisor"/> in the interceptor
        /// (advice) chain for this proxy.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The return value of this method can be used to index into
        /// the <see cref="Spring.Aop.Framework.IAdvised.Advisors"/>
        /// list.
        /// </p>
        /// </remarks>
        /// <param name="advisor">
        /// The <see cref="Spring.Aop.IAdvisor"/> to search for.
        /// </param>
        /// <returns>
        /// The zero (0) based index of this advisor, or -1 if the
        /// supplied <paramref name="advisor"/> is not an advisor for this
        /// proxy.
        /// </returns>
        int IndexOf(IAdvisor advisor);

        /// <summary>
        /// Return the index (0 based) of the supplied
        /// <see cref="Spring.Aop.IIntroductionAdvisor"/> in the introductions
        /// for this proxy.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The return value of this method can be used to index into
        /// the <see cref="Spring.Aop.Framework.IAdvised.Introductions"/>
        /// list.
        /// </p>
        /// </remarks>
        /// <param name="advisor">
        /// The <see cref="Spring.Aop.IIntroductionAdvisor"/> to search for.
        /// </param>
        /// <returns>
        /// The zero (0) based index of this advisor, or -1 if the
        /// supplied <paramref name="advisor"/> is not an introduction advisor
        /// for this proxy.
        /// </returns>
        int IndexOf(IIntroductionAdvisor advisor);

        /// <summary>
        /// Removes the supplied <paramref name="advisor"/> the list of advisors
        /// for this proxy.
        /// </summary>
        /// <param name="advisor">The advisor to remove.</param>
        /// <returns>
        /// <see langword="true"/> if advisor was found in the list of
        /// <see cref="Spring.Aop.Framework.IAdvised.Advisors"/> for this
        /// proxy and was successfully removed; <see langword="false"/> if not
        /// or if the supplied <paramref name="advisor"/> is <cref lang="null"/>.
        /// </returns>
        /// <exception cref="AopConfigException">
        /// If this proxy configuration is frozen and the
        /// <paramref name="advisor"/> cannot be removed.
        /// </exception>
        bool RemoveAdvisor(IAdvisor advisor);

        /// <summary>
        /// Removes the <see cref="Spring.Aop.IAdvisor"/> at the supplied
        /// <paramref name="index"/> in the
        /// <see cref="Spring.Aop.Framework.IAdvised.Advisors"/> list
        /// from the list of
        /// <see cref="Spring.Aop.Framework.IAdvised.Advisors"/> for this proxy.
        /// </summary>
        /// <param name="index">
        /// The index of the <see cref="Spring.Aop.IAdvisor"/> to remove.
        /// </param>
        /// <exception cref="AopConfigException">
        /// If this proxy configuration is frozen and the
        /// <see cref="Spring.Aop.IAdvisor"/> at the supplied <paramref name="index"/>
        /// cannot be removed; or if the supplied <paramref name="index"/> is out of
        /// range.
        /// </exception>
        void RemoveAdvisor(int index);

        /// <summary>
        /// Removes the supplied <paramref name="advice"/> from the list
        /// of <see cref="Spring.Aop.Framework.IAdvised.Advisors"/>.
        /// </summary>
        /// <param name="advice">
        /// The <see cref="AopAlliance.Aop.IAdvice"/> to remove.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the supplied <paramref name="advice"/> was
        /// found in the list of <see cref="Spring.Aop.Framework.IAdvised.Advisors"/>
        /// and successfully removed.
        /// </returns>
        /// <exception cref="AopConfigException">
        /// If this proxy configuration is frozen and the
        /// <see cref="AopAlliance.Aop.IAdvice"/> cannot be removed.
        /// </exception>
        bool RemoveAdvice(IAdvice advice);

        /// <summary>
        /// Removes the supplied <paramref name="introduction"/> from the list
        /// of <see cref="Spring.Aop.Framework.IAdvised.Introductions"/>.
        /// </summary>
        /// <param name="introduction">
        /// The <see cref="Spring.Aop.IIntroductionAdvisor"/> to remove.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the supplied <paramref name="introduction"/> was
        /// found in the list of <see cref="Spring.Aop.Framework.IAdvised.Introductions"/>
        /// and successfully removed.
        /// </returns>
        /// <exception cref="AopConfigException">
        /// If this proxy configuration is frozen and the
        /// <see cref="Spring.Aop.IIntroductionAdvisor"/> cannot be removed.
        /// </exception>
        bool RemoveIntroduction(IIntroductionAdvisor introduction);

        /// <summary>
        /// Removes the <see cref="Spring.Aop.IIntroductionAdvisor"/> at the supplied
        /// <paramref name="index"/> in the list of
        /// <see cref="Spring.Aop.Framework.IAdvised.Introductions"/> for this proxy.
        /// </summary>
        /// <param name="index">The index of the advisor to remove.
        /// </param>
        /// <exception cref="AopConfigException">
        /// If this proxy configuration is frozen and the
        /// <see cref="Spring.Aop.IIntroductionAdvisor"/> at the supplied
        /// <paramref name="index"/> cannot be removed; or if the supplied
        /// <paramref name="index"/> is out of range.
        /// </exception>
        void RemoveIntroduction(int index);

        /// <summary>
        /// Replaces the <see cref="Spring.Aop.IIntroductionAdvisor"/> that
        /// exists at the supplied <paramref name="index"/> in the list of
        /// <see cref="Spring.Aop.Framework.IAdvised.Introductions"/>
        /// with the supplied <paramref name="introduction"/>.
        /// </summary>
        /// <param name="index">
        /// The index of the <see cref="Spring.Aop.IIntroductionAdvisor"/>
        /// in the list of
        /// <see cref="Spring.Aop.Framework.IAdvised.Introductions"/>
        /// that is to be replaced.
        /// </param>
        /// <param name="introduction">
        /// The new (replacement) <see cref="Spring.Aop.IIntroductionAdvisor"/>.
        /// </param>
        /// <exception cref="AopConfigException">
        /// If the supplied <paramref name="index"/> is out of range.
        /// </exception>
        void ReplaceIntroduction(int index, IIntroductionAdvisor introduction);

        /// <summary>
        /// Replaces the <paramref name="oldAdvisor"/> with the
        /// <paramref name="newAdvisor"/>.
        /// </summary>
        /// <param name="oldAdvisor">
        /// The original (old) advisor to be replaced.
        /// </param>
        /// <param name="newAdvisor">
        /// The new advisor to replace the <paramref name="oldAdvisor"/> with.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="oldAdvisor"/> was
        /// replaced; if the <paramref name="oldAdvisor"/> was not found in the
        /// advisors collection, this method returns <see langword="false"/>
        /// and (effectively) does nothing.
        /// </returns>
        /// <exception cref="AopConfigException">
        /// If this proxy configuration is frozen and the
        /// <paramref name="oldAdvisor"/> cannot be replaced.
        /// </exception>
        /// <seealso cref="Spring.Aop.Framework.ProxyConfig.IsFrozen"/>
        bool ReplaceAdvisor(IAdvisor oldAdvisor, IAdvisor newAdvisor);

        /// <summary>
        /// As <see cref="System.Object.ToString()"/> will normally be passed
        /// straight through to the advised target, this method returns the
        /// <see cref="System.Object.ToString()"/> equivalent for the AOP
        /// proxy itself.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> description of the proxy configuration.
        /// </returns>
        string ToProxyConfigString();
    }
}
