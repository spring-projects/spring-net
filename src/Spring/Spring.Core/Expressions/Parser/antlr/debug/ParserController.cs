namespace Spring.Expressions.Parser.antlr.debug
{
    public interface ParserController : ParserListener
		{
			ParserEventSupport ParserEventSupport
			{
				set;
			}

			void  checkBreak();
		}
}