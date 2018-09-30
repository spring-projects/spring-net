

using Spring.Messaging.Core;

#if NETSTANDARD
using Experimental.System.Messaging;
#else
using System.Messaging;
#endif

namespace Spring.Messaging.Support
{
    /// <summary>
    /// Delegate for creating MessageQueue instance.  Used by <see cref="DefaultMessageQueueFactory"/>
    /// to register a creation function with a given name.
    /// </summary>
    public delegate MessageQueue MessageQueueCreatorDelegate();
}