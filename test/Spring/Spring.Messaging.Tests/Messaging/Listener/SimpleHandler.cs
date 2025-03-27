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
            LOG.Debug("Received text = [" + msgTxt + "]");
            LOG.Debug("constructor set state string  = " + stateVariable);
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
