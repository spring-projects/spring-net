namespace Spring.Expressions.Parser.antlr.debug
{
    public interface IParserDebugSubject : IDebugSubject
	{
		event MatchEventHandler					MatchedToken;
		event MatchEventHandler					MatchedNotToken;
		event MatchEventHandler					MisMatchedToken;
		event MatchEventHandler					MisMatchedNotToken;
		event TokenEventHandler					ConsumedToken;
		event TokenEventHandler					TokenLA;
	}
}
