namespace Spring.Expressions.Parser.antlr.debug
{
    //using EventHandlerList	= System.ComponentModel.EventHandlerList;
	
	public interface IDebugSubject
	{
	/*	EventHandlerList Events 
		{
			get;
		}
*/
		event TraceEventHandler					EnterRule;
		event TraceEventHandler					ExitRule;
		event TraceEventHandler					Done;
		event MessageEventHandler				ErrorReported;
		event MessageEventHandler				WarningReported;
		event SemanticPredicateEventHandler		SemPredEvaluated;
		event SyntacticPredicateEventHandler	SynPredStarted;
		event SyntacticPredicateEventHandler	SynPredFailed;
		event SyntacticPredicateEventHandler	SynPredSucceeded;
	}
}
