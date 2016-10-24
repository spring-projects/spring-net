namespace Spring.Expressions.Parser.antlr.debug
{
    public interface SyntacticPredicateListener : Listener
	{
		void  syntacticPredicateFailed		(object source, SyntacticPredicateEventArgs e);
		void  syntacticPredicateStarted		(object source, SyntacticPredicateEventArgs e);
		void  syntacticPredicateSucceeded	(object source, SyntacticPredicateEventArgs e);
	}
}