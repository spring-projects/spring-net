

namespace Spring.Messaging.Core
{
    public enum QueueIdentifierType
    {
        /// <summary>
        /// Use a Label to identify the queue, for example new MessageQueue("Label:TheLabel");
        /// </summary>
        Label,

        /// <summary>
        /// Use a FormatName to idenitfy the queue, 
        /// </summary>
        FormatName,
        
        MachineName, 

        Path,

        QueueName,
    }
}