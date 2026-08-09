using Microsoft.Extensions.Logging;

namespace Spring.Messaging.Listener;

public class WaitingHandler
{
    private static readonly ILogger<WaitingHandler> LOG = LogManager.GetLogger<WaitingHandler>();

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
        LOG.LogDebug("Received text = [{MsgTxt}]", msgTxt);
        LOG.LogDebug("constructor set state string  = {StateVariable}", stateVariable);

        Thread.Sleep(10000);

        messageCount++;
        LOG.LogDebug("Message listener count = {MessageCount}", messageCount);
        return msgTxt + " - processed!";
    }
}