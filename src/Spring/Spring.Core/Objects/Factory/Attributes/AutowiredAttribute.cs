#region License

/*
 * Copyright © 2010-2011 the original author or authors.
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

namespace Spring.Objects.Factory.Attributes
{
    /// <summary>
    /// Marks a constructor, field, propery or config method as to be
    /// autowired by Spring's dependency injection facilities.
    /// 
    /// Only one constructor (at max) of any given bean class may carry this
    /// annotation, indicating the constructor to autowire when used as a Spring
    /// bean. Such a constructor does not have to be public.
    /// 
    /// Fields are injected right after construction of a object, before any
    /// config methods are invoked. Such a config field does not have to be public.
    /// 
    /// Config methods may have an arbitrary name and any number of arguments;
    /// each of those arguments will be autowired with a matching bean in the
    /// Spring container. Object property setter methods are effectively just
    /// a special case of such a general config method. Such config methods
    /// do not have to be public.
    /// 
    /// In the case of multiple argument methods, the 'required' parameter is
    /// applicable for all arguments.
    /// 
    /// In case of a {@link java.util.Collection} or {@link java.util.Map}
    /// dependency type, the container will autowire all beans matching the
    /// declared value type. In case of a Map, the keys must be declared as
    /// type String and will be resolved to the corresponding bean names.
    /// 
    /// Note that actual injection is performed through a
    /// {@link org.springframework.beans.factory.config.BeanPostProcessor
    /// BeanPostProcessor} which in turn means that you <em>cannot</em>
    /// use {@code @Autowired} to inject references into
    /// {@link org.springframework.beans.factory.config.BeanPostProcessor
    /// BeanPostProcessor} or
    /// {@link org.springframework.beans.factory.config.BeanFactoryPostProcessor BeanFactoryPostProcessor}
    /// types. Please consult the javadoc for the {@link AutowiredAnnotationBeanPostProcessor}
    /// class (which, by default, checks for the presence of this annotation).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Constructor)]
    public class AutowiredAttribute : Attribute
    {
        private bool _required = true;

        /// <summary>
        /// Defines it Autowired PostProcessor should fail if object is not set
        /// </summary>
        public bool Required
        {
            get { return _required; } 
            set { _required = value; }
        }
    }
}
