using Apache.NMS;

namespace Spring.Messaging.Nms.Listener
{
    public interface ISessionAwareMessageListener
    {

        /// <summary> Callback for processing a received NMS message.
        /// Implementors are supposed to process the given IMessage,
        /// typically sending reply messages through the given ISession.
        /// </summary>
        /// <param name="message">the received NMS message
        /// </param>
        /// <param name="session">the underlying NMS ISession
        /// </param>
        /// <throws>  NMSException if thrown by NMS methods </throws>
        void OnMessage(IMessage message, ISession session);
    }
}
