using System.Web;

namespace Spring.Threading
{
    /// <summary>
    /// Implements <see cref="IThreadStorage"/> by using <see cref="HttpContext"/>.
    /// </summary>
    /// <author>Erich Eichinger</author>
    /// <version>$Id: HttpContextStorage.cs,v 1.1 2007/02/02 21:31:25 oakinger Exp $</version>  
    public class HttpContextStorage : IThreadStorage
    {
        /// <summary>
        /// Retrieves an object with the specified name.
        /// </summary>
        /// <param name="name">The name of the item.</param>
        /// <returns>The object in the context associated with the specified name or null if no object has been stored previously</returns>
        public object GetData(string name)
        {
            return HttpContext.Current.Items[name];
        }

        /// <summary>
        /// Stores a given object and associates it with the specified name.
        /// </summary>
        /// <param name="name">The name with which to associate the new item.</param>
        /// <param name="value">The object to store in the call context.</param>
        public void SetData(string name, object value)
        {
            HttpContext.Current.Items[name] = value;
        }

        /// <summary>
        /// Empties a data slot with the specified name.
        /// </summary>
        /// <param name="name">The name of the data slot to empty.</param>
        public void FreeNamedDataSlot(string name)
        {
            HttpContext.Current.Items.Remove(name);
        }
    }
}