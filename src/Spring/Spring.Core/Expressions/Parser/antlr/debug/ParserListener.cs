namespace Spring.Expressions.Parser.antlr.debug
{
	using System;
	
	public interface ParserListener : SemanticPredicateListener, ParserMatchListener, MessageListener, ParserTokenListener, TraceListener, SyntacticPredicateListener
	{
	}
}