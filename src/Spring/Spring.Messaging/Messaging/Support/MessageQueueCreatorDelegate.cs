

using System.Messaging;
using Spring.Messaging.Core;

namespace Spring.Messaging.Support
{
    /// <summary>
    /// Delegate for creating MessageQueue instance.  Used by <see cref="DefaultMessageQueueFactory"/>
    /// to register a creation function with a given name.
    /// </summary>
    public delegate MessageQueue MessageQueueCreatorDelegate();
}