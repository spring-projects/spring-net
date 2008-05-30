using System.Collections;
using Spring.Globalization;

namespace Spring.DataBinding
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBindingAwareFormatter : IFormatter
    {
        /// <summary>
        /// Sets the binding context of this formatter.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="variables"></param>
        /// <param name="direction"></param>
        void SetBindingContext(object source, object target, IDictionary variables, BindingDirection direction);
        /// <summary>
        /// Clears the binding context of this formatter.
        /// </summary>
        void ClearBindingContext();
    }
}