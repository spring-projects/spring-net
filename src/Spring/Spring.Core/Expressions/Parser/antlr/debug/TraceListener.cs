namespace Spring.Expressions.Parser.antlr.debug
{
    public interface TraceListener : Listener
	{
		void  enterRule	(object source, TraceEventArgs e);
		void  exitRule	(object source, TraceEventArgs e);
	}
}