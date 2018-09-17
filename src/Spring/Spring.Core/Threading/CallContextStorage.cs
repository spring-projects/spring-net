using System.Runtime.Remoting.Messaging;

namespace Spring.Threading
{
    /// <summary>
    /// Implements <see cref="IThreadStorage"/> by using <see cref="CallContext"/>.
    /// </summary>
    /// <author>Erich Eichinger</author>
    public class CallContextStorage : IThreadStorage
    {
        /// <summary>
        /// Retrieves an object with the specified name.
        /// </summary>
        /// <param name="name">The name of the item.</param>
        /// <returns>The object in the call context associated with the specified name or null if no object has been stored previously</returns>
        public object GetData(string name)
        {
            return CallContext.GetData(name);
        }

        /// <summary>
        /// Stores a given object and associates it with the specified name.
        /// </summary>
        /// <param name="name">The name with which to associate the new item.</param>
        /// <param name="value">The object to store in the call context.</param>
        public void SetData(string name, object value)
        {
            CallContext.SetData(name, value);
        }

        /// <summary>
        /// Empties a data slot with the specified name.
        /// </summary>
        /// <param name="name">The name of the data slot to empty.</param>
        public void FreeNamedDataSlot(string name)
        {
            CallContext.FreeNamedDataSlot(name);
        }
    }
}
