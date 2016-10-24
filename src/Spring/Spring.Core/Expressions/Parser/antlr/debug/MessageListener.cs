namespace Spring.Expressions.Parser.antlr.debug
{
    public interface MessageListener : Listener
	{
		void  reportError	(object source, MessageEventArgs e);
		void  reportWarning	(object source, MessageEventArgs e);
	}
}