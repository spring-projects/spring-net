namespace Spring.Expressions.Parser.antlr.debug
{
    public interface InputBufferListener : Listener
	{
		void  inputBufferConsume	(object source, InputBufferEventArgs e);
		void  inputBufferLA			(object source, InputBufferEventArgs e);
		void  inputBufferMark		(object source, InputBufferEventArgs e);
		void  inputBufferRewind		(object source, InputBufferEventArgs e);
	}
}