

using System;
using Microsoft.Extensions.Logging;

namespace Spring.Messaging.Listener
{
    public class SimpleHandler
    {
        #region Logging

        private static readonly ILog LOG = LogManager.GetLogger(typeof(SimpleHandler));

        #endregion

        private int messageCount;

        private string stateVariable;

        public SimpleHandler()
        {
            this.stateVariable = "hello";
        }
        public SimpleHandler(string stateVariable)
        {
            this.stateVariable = stateVariable;
        }


        public int MessageCount
        {
            get { return messageCount; }
            set { messageCount = value; }
        }

        public string HandleMessage(string msgTxt)
        {
            LOG.LogDebug("Received text = [" + msgTxt + "]");
            LOG.LogDebug("constructor set state string  = " + stateVariable);
            if (msgTxt.Contains("Goodbye"))
            {
                throw new ArgumentException("Don't like saying goodbye!");
            }
            messageCount++;
            LOG.LogDebug("Message listener count = " + messageCount);
            return msgTxt + " - processed!";
        }
    }
}
