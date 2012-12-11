using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Logging;
using Spring.Collections;
using Spring.Objects.Factory.Config;
using Spring.Util;

namespace Spring.Objects.Factory.Support
{
    public class DisposableObjectAdapter : IDisposable
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(DisposableObjectAdapter));

        private object instance;

        private string objectName;

        private bool invokeDisposableObject;

        private string destroyMethodName;

        private MethodInfo destroyMethod;

        private List<IDestructionAwareObjectPostProcessor> objectPostProcessors;

        /// <summary>
        /// Disposables the bean adapter.
        /// </summary>
        /// <param name="instance">The bean.</param>
        /// <param name="objectName">Name of the bean.</param>
        /// <param name="objectDefinition">The bean definition.</param>
        /// <param name="postProcessors">The post processors.</param>
        /// <param name="acc">The acc.</param>
        /// Create a new DisposableBeanAdapter for the given bean.
        /// @param bean the bean instance (never 
        /// <code>null</code>
        /// )
        /// @param beanName the name of the bean
        /// @param beanDefinition the merged bean definition
        /// @param postProcessors the List of BeanPostProcessors
        /// (potentially DestructionAwareBeanPostProcessor), if any

        public DisposableObjectAdapter(object instance, string objectName, RootObjectDefinition objectDefinition, ISet postProcessors)
        {
            AssertUtils.ArgumentNotNull(instance, "Disposable object must not be null");

            this.instance = instance;
            this.objectName = objectName;
            this.invokeDisposableObject = (this.instance is IDisposable); // && !beanDefinition.IsExternallyManagedDestroyMethod("destroy"));

            if (null == objectDefinition)
            {
                return;
            }

            InferDestroyMethodIfNecessary(objectDefinition);

            string definedDestroyMethodName = objectDefinition.DestroyMethodName;

            if (definedDestroyMethodName != null && !(this.invokeDisposableObject && "Destroy".Equals(definedDestroyMethodName))) // && !beanDefinition.isExternallyManagedDestroyMethod(destroyMethodName)) 
            {
                this.destroyMethodName = definedDestroyMethodName;
                this.destroyMethod = DetermineDestroyMethod();
                if (this.destroyMethod == null)
                {
                    //TODO: add support for Enforcing Destroy Method
                    //if (beanDefinition.IsEnforceDestroyMethod()) {
                    //    throw new BeanDefinitionValidationException("Couldn't find a destroy method named '" +
                    //                                                destroyMethodName + "' on bean with name '" + beanName + "'");
                    //}
                }
                else
                {
                    Type[] paramTypes = ReflectionUtils.GetParameterTypes(this.destroyMethod);
                    if (paramTypes.Length > 1)
                    {
                        throw new ObjectDefinitionValidationException("Method '" + definedDestroyMethodName + "' of object '" +
                                                                    objectName + "' has more than one parameter - not supported as Destroy Method");
                    }
                    else if (paramTypes.Length == 1 && !(paramTypes[0] == typeof(bool)))
                    {
                        throw new ObjectDefinitionValidationException("Method '" + definedDestroyMethodName + "' of object '" +
                                                                    objectName + "' has a non-boolean parameter - not supported as Destroy Method");
                    }
                }
            }
            this.objectPostProcessors = FilterPostProcessors(postProcessors);
        }

        /**
	 * If the current value of the given beanDefinition's destroyMethodName property is
	 * {@link AbstractBeanDefinition#INFER_METHOD}, then attempt to infer a destroy method.
	 * Candidate methods are currently limited to public, no-arg methods named 'close'
	 * (whether declared locally or inherited). The given beanDefinition's
	 * destroyMethodName is updated to be null if no such method is found, otherwise set
	 * to the name of the inferred method. This constant serves as the default for the
	 * {@code @Bean#customDestroyMethod} attribute and the value of the constant may also be
	 * used in XML within the {@code <bean destroy-method="">} or {@code
	 * <beans default-destroy-method="">} attributes.
	 */
        private void InferDestroyMethodIfNecessary(RootObjectDefinition beanDefinition)
        {
            if ("(Inferred)".Equals(beanDefinition.DestroyMethodName))
            {
                try
                {
                    MethodInfo candidate = ReflectionUtils.GetMethod(instance.GetType(), "Close", null);
                    if (candidate.IsPublic)
                    {
                        beanDefinition.DestroyMethodName = candidate.Name;
                    }
                }
                catch (MissingMethodException ex)
                {
                    // no candidate destroy method found
                    beanDefinition.DestroyMethodName = null;
                }
            }
        }

        /// <summary>
        /// Filters the post processors.
        /// </summary>
        /// <param name="postProcessors">The post processors.</param>
        /// <returns></returns>
        /// Search for all DestructionAwareBeanPostProcessors in the List.
        /// @param postProcessors the List to search
        /// @return the filtered List of DestructionAwareBeanPostProcessors
        private List<IDestructionAwareObjectPostProcessor> FilterPostProcessors(ISet postProcessors)
        {
            List<IDestructionAwareObjectPostProcessor> filteredPostProcessors = null;
            if (postProcessors != null && postProcessors.Count != 0)
            {
                filteredPostProcessors = new List<IDestructionAwareObjectPostProcessor>(postProcessors.Count);
                filteredPostProcessors.AddRange(postProcessors.OfType<IDestructionAwareObjectPostProcessor>());
            }
            return filteredPostProcessors;
        }



        public void Dispose()
        {
            if (this.objectPostProcessors != null && this.objectPostProcessors.Count != 0)
            {
                foreach (IDestructionAwareObjectPostProcessor processor in this.objectPostProcessors)
                {
                    try
                    {
                        processor.PostProcessBeforeDestruction(this.instance, this.objectName);
                    }

                    catch (Exception ex)
                    {
                        logger.ErrorFormat(
                            string.Format("Error during execution of {0}.PostProcessBeforeDestruction for object {1}",
                                          processor.GetType().Name, this.objectName), ex);
                    }
                }
            }

            if (this.invokeDisposableObject)
            {
                if (logger.IsDebugEnabled)
                {
                    logger.Debug("Invoking Dispose() on object with name '" + this.objectName + "'");
                }
                try
                {
                    ((IDisposable)instance).Dispose();

                }

                catch (Exception ex)
                {
                    string msg = "Invocation of Dispose method failed on object with name '" + this.objectName + "'";
                    if (logger.IsDebugEnabled)
                    {
                        logger.Warn(msg, ex);
                    }
                    else
                    {
                        logger.Warn(msg + ": " + ex);
                    }
                }
            }

            if (this.destroyMethod != null)
            {
                InvokeCustomDestroyMethod(this.destroyMethod);
            }
            else if (this.destroyMethodName != null)
            {
                MethodInfo methodToCall = DetermineDestroyMethod();
                if (methodToCall != null)
                {
                    InvokeCustomDestroyMethod(methodToCall);
                }
            }
        }


        private MethodInfo DetermineDestroyMethod()
        {
            try
            {
                return FindDestroyMethod();
            }

            catch (ArgumentException ex)
            {
                throw new ObjectDefinitionValidationException("Couldn't find a unique Destroy Method on object with name '" +
                                                            this.objectName + ": " + ex.Message);
            }
        }

        private MethodInfo FindDestroyMethod()
        {
            try
            {
                return ReflectionUtils.GetMethod(instance.GetType(), this.destroyMethodName, null);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Invokes the custom destroy method.
        /// </summary>
        /// <param name="customDestroyMethod">The custom destroy method.</param>
        /// Invoke the specified custom destroy method on the given bean.
        /// This implementation invokes a no-arg method if found, else checking
        /// for a method with a single boolean argument (passing in "true",
        /// assuming a "force" parameter), else logging an error.
        private void InvokeCustomDestroyMethod(MethodInfo customDestroyMethod)
        {
            Type[] paramTypes = ReflectionUtils.GetParameterTypes(customDestroyMethod);
            object[] args = new object[paramTypes.Length];
            if (paramTypes.Length == 1)
            {
                args[0] = true;
            }
            if (logger.IsDebugEnabled)
            {
                logger.Debug("Invoking destroy method '" + this.destroyMethodName +
                             "' on object with name '" + this.objectName + "'");
            }
            try
            {

                customDestroyMethod.Invoke(instance, args);
            }
            catch (TargetInvocationException ex)
            {
                string msg = "Invocation of destroy method '" + this.destroyMethodName +
                             "' failed on object with name '" + this.objectName + "'";
                if (logger.IsDebugEnabled)
                {
                    logger.Warn(msg, ex.InnerException);
                }
                else
                {
                    logger.Warn(msg + ": " + ex.InnerException);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Couldn't invoke destroy method '" + this.destroyMethodName +
                             "' on object with name '" + this.objectName + "'", ex);
            }
        }


        /**
	 * Serializes a copy of the state of this class,
	 * filtering out non-serializable BeanPostProcessors.
	 */
        //protected Object writeReplace() {
        //    List<DestructionAwareBeanPostProcessor> serializablePostProcessors = null;
        //    if (this.objectPostProcessors != null) {
        //        serializablePostProcessors = new ArrayList<DestructionAwareBeanPostProcessor>();
        //        for (DestructionAwareBeanPostProcessor postProcessor : this.objectPostProcessors) {
        //            if (postProcessor instanceof Serializable) {
        //                serializablePostProcessors.add(postProcessor);
        //            }
        //        }
        //    }
        //    return new DisposableBeanAdapter(this.bean, this.beanName, this.invokeDisposableObject,
        //                                     this.nonPublicAccessAllowed, this.destroyMethodName, serializablePostProcessors);
        //}
    }
}