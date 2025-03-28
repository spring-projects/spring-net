using System.Runtime.Remoting.Messaging;
using System.Web;

namespace Spring.Threading;

/// <summary>
/// Implements <see cref="IThreadStorage"/> by using both <see cref="HttpContext"/> and <see cref="CallContext"/> and choosing dynamically between them.
/// </summary>
/// <remarks>
/// In web applications a single Request may be executed on different threads. In this case HttpContext.Current is the only invariant.<br/>
/// This implementation dynamically chooses between <see cref="System.Runtime.Remoting.Messaging.CallContext">System.Runtime.Remoting.Messaging.CallContext</see> 
/// and <see cref="System.Web.HttpContext">System.Web.HttpContext</see> to store data.
/// </remarks>
/// <author>Erich Eichinger</author>
public class HybridContextStorage : IThreadStorage
{
    /// <summary>
    /// Retrieves an object with the specified name.
    /// </summary>
    /// <param name="name">The name of the item.</param>
    /// <returns>The object in the context associated with the specified name or null if no object has been stored previously</returns>
    public object GetData(string name)
    {
        HttpContext ctx = HttpContext.Current;
        if (ctx == null)
        {
            return CallContext.GetData(name);
        }
        else
        {
            return ctx.Items[name];
        }
    }

    /// <summary>
    /// Stores a given object and associates it with the specified name.
    /// </summary>
    /// <param name="name">The name with which to associate the new item.</param>
    /// <param name="value">The object to store in the call context.</param>
    public void SetData(string name, object value)
    {
        HttpContext ctx = HttpContext.Current;
        if (ctx == null)
        {
            CallContext.SetData(name, value);
        }
        else
        {
            ctx.Items[name] = value;
        }
    }

    /// <summary>
    /// Empties a data slot with the specified name.
    /// </summary>
    /// <param name="name">The name of the data slot to empty.</param>
    public void FreeNamedDataSlot(string name)
    {
        HttpContext ctx = HttpContext.Current;
        if (ctx == null)
        {
            CallContext.FreeNamedDataSlot(name);
        }
        else
        {
            ctx.Items.Remove(name);
        }
    }
}