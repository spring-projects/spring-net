namespace Spring.Messaging.Listener
{
    public class WaitingHandler
    {
        #region Logging

        private static readonly ILog LOG = LogManager.GetLogger(typeof(WaitingHandler));

        #endregion

        private int messageCount;

        private string stateVariable;

        public WaitingHandler()
        {
            this.stateVariable = "hello";
        }
        public WaitingHandler(string stateVariable)
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
            LOG.Debug(String.Format("Received text = [{0}]", msgTxt));
            LOG.Debug("constructor set state string  = " + stateVariable);

            Thread.Sleep(10000);

            messageCount++;
            LOG.Debug("Message listener count = " + messageCount);
            return msgTxt + " - processed!";
        }
    }
}
