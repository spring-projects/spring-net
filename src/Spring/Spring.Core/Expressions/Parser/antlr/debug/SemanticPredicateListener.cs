namespace Spring.Expressions.Parser.antlr.debug
{
    public interface SemanticPredicateListener : Listener
	{
		void  semanticPredicateEvaluated(object source, SemanticPredicateEventArgs e);
	}
}