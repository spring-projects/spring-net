

using System;
using Common.Logging;

namespace Spring.Messaging.Listener
{
    public class SimpleHandler
    {
        #region Logging

        private static readonly ILog LOG = LogManager.GetLogger(typeof(SimpleHandler));

        #endregion

        private int messageCount;


        public int MessageCount
        {
            get { return messageCount; }
            set { messageCount = value; }
        }

        public string HandleMessage(string msgTxt)
        {
            LOG.Debug("Received text = [" + msgTxt + "]");
            if (msgTxt.Contains("Goodbye"))
            {
                throw new ArgumentException("Don't like saying goodbye!");
            }
            messageCount++;
            LOG.Debug("Message listener count = " + messageCount);
            return msgTxt + " - processed!";
        }
    }
}