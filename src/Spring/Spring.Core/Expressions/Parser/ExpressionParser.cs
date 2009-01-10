// $ANTLR 2.7.6 (2005-12-22): "Expression.g" -> "ExpressionParser.cs"$

namespace Spring.Expressions
{
	// Generate the header common to all output files.
	using System;
	
	using TokenBuffer              = antlr.TokenBuffer;
	using TokenStreamException     = antlr.TokenStreamException;
	using TokenStreamIOException   = antlr.TokenStreamIOException;
	using ANTLRException           = antlr.ANTLRException;
	using LLkParser = antlr.LLkParser;
	using Token                    = antlr.Token;
	using IToken                   = antlr.IToken;
	using TokenStream              = antlr.TokenStream;
	using RecognitionException     = antlr.RecognitionException;
	using NoViableAltException     = antlr.NoViableAltException;
	using MismatchedTokenException = antlr.MismatchedTokenException;
	using SemanticException        = antlr.SemanticException;
	using ParserSharedInputState   = antlr.ParserSharedInputState;
	using BitSet                   = antlr.collections.impl.BitSet;
	using AST                      = antlr.collections.AST;
	using ASTPair                  = antlr.ASTPair;
	using ASTFactory               = antlr.ASTFactory;
	using ASTArray                 = antlr.collections.impl.ASTArray;
	
	internal 	class ExpressionParser : antlr.LLkParser
	{
		public const int EOF = 1;
		public const int NULL_TREE_LOOKAHEAD = 3;
		public const int EXPR = 4;
		public const int OPERAND = 5;
		public const int FALSE = 6;
		public const int TRUE = 7;
		public const int AND = 8;
		public const int OR = 9;
		public const int IN = 10;
		public const int IS = 11;
		public const int BETWEEN = 12;
		public const int LIKE = 13;
		public const int MATCHES = 14;
		public const int NULL_LITERAL = 15;
		public const int LPAREN = 16;
		public const int SEMI = 17;
		public const int RPAREN = 18;
		public const int ASSIGN = 19;
		public const int DEFAULT = 20;
		public const int QMARK = 21;
		public const int COLON = 22;
		public const int PLUS = 23;
		public const int MINUS = 24;
		public const int STAR = 25;
		public const int DIV = 26;
		public const int MOD = 27;
		public const int POWER = 28;
		public const int BANG = 29;
		public const int DOT = 30;
		public const int POUND = 31;
		public const int ID = 32;
		public const int DOLLAR = 33;
		public const int COMMA = 34;
		public const int AT = 35;
		public const int LBRACKET = 36;
		public const int RBRACKET = 37;
		public const int PROJECT = 38;
		public const int RCURLY = 39;
		public const int SELECT = 40;
		public const int SELECT_FIRST = 41;
		public const int SELECT_LAST = 42;
		public const int TYPE = 43;
		public const int QUOTE = 44;
		public const int STRING_LITERAL = 45;
		public const int LAMBDA = 46;
		public const int PIPE = 47;
		public const int LITERAL_new = 48;
		public const int LCURLY = 49;
		public const int INTEGER_LITERAL = 50;
		public const int HEXADECIMAL_INTEGER_LITERAL = 51;
		public const int REAL_LITERAL = 52;
		public const int LITERAL_date = 53;
		public const int EQUAL = 54;
		public const int NOT_EQUAL = 55;
		public const int LESS_THAN = 56;
		public const int LESS_THAN_OR_EQUAL = 57;
		public const int GREATER_THAN = 58;
		public const int GREATER_THAN_OR_EQUAL = 59;
		public const int WS = 60;
		public const int BACKTICK = 61;
		public const int BACKSLASH = 62;
		public const int DOT_ESCAPED = 63;
		public const int APOS = 64;
		public const int NUMERIC_LITERAL = 65;
		public const int DECIMAL_DIGIT = 66;
		public const int INTEGER_TYPE_SUFFIX = 67;
		public const int HEX_DIGIT = 68;
		public const int EXPONENT_PART = 69;
		public const int SIGN = 70;
		public const int REAL_TYPE_SUFFIX = 71;
		
		
    // CLOVER:OFF
    
    public override void reportError(RecognitionException ex)
    {
		//base.reportError(ex);
        throw new antlr.TokenStreamRecognitionException(ex);
    }

    public override void reportError(string error)
    {
		//base.reportError(error);
        throw new RecognitionException(error);
    }
    
    private string GetRelationalOperatorNodeType(string op)
    {
        switch (op)
        {
            case "==" : return "Spring.Expressions.OpEqual";
            case "!=" : return "Spring.Expressions.OpNotEqual";
            case "<" : return "Spring.Expressions.OpLess";
            case "<=" : return "Spring.Expressions.OpLessOrEqual";
            case ">" : return "Spring.Expressions.OpGreater";
            case ">=" : return "Spring.Expressions.OpGreaterOrEqual";
            case "in" : return "Spring.Expressions.OpIn";
            case "is" : return "Spring.Expressions.OpIs";
            case "between" : return "Spring.Expressions.OpBetween";
            case "like" : return "Spring.Expressions.OpLike";
            case "matches" : return "Spring.Expressions.OpMatches";
            default : 
                throw new ArgumentException("Node type for operator '" + op + "' is not defined.");
        }
    }
		
		protected void initialize()
		{
			tokenNames = tokenNames_;
			initializeFactory();
		}
		
		
		protected ExpressionParser(TokenBuffer tokenBuf, int k) : base(tokenBuf, k)
		{
			initialize();
		}
		
		public ExpressionParser(TokenBuffer tokenBuf) : this(tokenBuf,2)
		{
		}
		
		protected ExpressionParser(TokenStream lexer, int k) : base(lexer,k)
		{
			initialize();
		}
		
		public ExpressionParser(TokenStream lexer) : this(lexer,2)
		{
		}
		
		public ExpressionParser(ParserSharedInputState state) : base(state,2)
		{
			initialize();
		}
		
	public void expr() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST expr_AST = null;
		
		try {      // for error handling
			expression();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			match(Token.EOF_TYPE);
			expr_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_0_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = expr_AST;
	}
	
	public void expression() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST expression_AST = null;
		
		try {      // for error handling
			logicalOrExpression();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			{
				switch ( LA(1) )
				{
				case ASSIGN:
				{
					{
						Spring.Expressions.AssignNode tmp2_AST = null;
						tmp2_AST = (Spring.Expressions.AssignNode) astFactory.create(LT(1), "Spring.Expressions.AssignNode");
						astFactory.makeASTRoot(ref currentAST, (AST)tmp2_AST);
						match(ASSIGN);
						logicalOrExpression();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
					}
					break;
				}
				case DEFAULT:
				{
					{
						Spring.Expressions.DefaultNode tmp3_AST = null;
						tmp3_AST = (Spring.Expressions.DefaultNode) astFactory.create(LT(1), "Spring.Expressions.DefaultNode");
						astFactory.makeASTRoot(ref currentAST, (AST)tmp3_AST);
						match(DEFAULT);
						logicalOrExpression();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
					}
					break;
				}
				case QMARK:
				{
					{
						Spring.Expressions.TernaryNode tmp4_AST = null;
						tmp4_AST = (Spring.Expressions.TernaryNode) astFactory.create(LT(1), "Spring.Expressions.TernaryNode");
						astFactory.makeASTRoot(ref currentAST, (AST)tmp4_AST);
						match(QMARK);
						expression();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
						match(COLON);
						expression();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
					}
					break;
				}
				case EOF:
				case SEMI:
				case RPAREN:
				case COLON:
				case COMMA:
				case RBRACKET:
				case RCURLY:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			expression_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_1_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = expression_AST;
	}
	
	public void exprList() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST exprList_AST = null;
		
		try {      // for error handling
			match(LPAREN);
			expression();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			{ // ( ... )+
				int _cnt4=0;
				for (;;)
				{
					if ((LA(1)==SEMI))
					{
						match(SEMI);
						expression();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
					}
					else
					{
						if (_cnt4 >= 1) { goto _loop4_breakloop; } else { throw new NoViableAltException(LT(1), getFilename());; }
					}
					
					_cnt4++;
				}
_loop4_breakloop:				;
			}    // ( ... )+
			match(RPAREN);
			if (0==inputState.guessing)
			{
				exprList_AST = (Spring.Expressions.SpringAST)currentAST.root;
				exprList_AST = (Spring.Expressions.SpringAST) astFactory.make((AST)(Spring.Expressions.SpringAST) astFactory.create(EXPR,"expressionList","Spring.Expressions.ExpressionListNode"), (AST)exprList_AST);
				currentAST.root = exprList_AST;
				if ( (null != exprList_AST) && (null != exprList_AST.getFirstChild()) )
					currentAST.child = exprList_AST.getFirstChild();
				else
					currentAST.child = exprList_AST;
				currentAST.advanceChildToEnd();
			}
			exprList_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = exprList_AST;
	}
	
	public void logicalOrExpression() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST logicalOrExpression_AST = null;
		
		try {      // for error handling
			logicalAndExpression();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==OR))
					{
						Spring.Expressions.OpOR tmp9_AST = null;
						tmp9_AST = (Spring.Expressions.OpOR) astFactory.create(LT(1), "Spring.Expressions.OpOR");
						astFactory.makeASTRoot(ref currentAST, (AST)tmp9_AST);
						match(OR);
						logicalAndExpression();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
					}
					else
					{
						goto _loop13_breakloop;
					}
					
				}
_loop13_breakloop:				;
			}    // ( ... )*
			logicalOrExpression_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_3_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = logicalOrExpression_AST;
	}
	
	public void parenExpr() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST parenExpr_AST = null;
		
		try {      // for error handling
			match(LPAREN);
			expression();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			match(RPAREN);
			parenExpr_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = parenExpr_AST;
	}
	
	public void logicalAndExpression() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST logicalAndExpression_AST = null;
		
		try {      // for error handling
			relationalExpression();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==AND))
					{
						Spring.Expressions.OpAND tmp12_AST = null;
						tmp12_AST = (Spring.Expressions.OpAND) astFactory.create(LT(1), "Spring.Expressions.OpAND");
						astFactory.makeASTRoot(ref currentAST, (AST)tmp12_AST);
						match(AND);
						relationalExpression();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
					}
					else
					{
						goto _loop16_breakloop;
					}
					
				}
_loop16_breakloop:				;
			}    // ( ... )*
			logicalAndExpression_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_4_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = logicalAndExpression_AST;
	}
	
	public void relationalExpression() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST relationalExpression_AST = null;
		Spring.Expressions.SpringAST e1_AST = null;
		Spring.Expressions.SpringAST op_AST = null;
		Spring.Expressions.SpringAST e2_AST = null;
		
		try {      // for error handling
			sumExpr();
			if (0 == inputState.guessing)
			{
				e1_AST = (Spring.Expressions.SpringAST)returnAST;
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			{
				if ((tokenSet_5_.member(LA(1))))
				{
					relationalOperator();
					if (0 == inputState.guessing)
					{
						op_AST = (Spring.Expressions.SpringAST)returnAST;
					}
					sumExpr();
					if (0 == inputState.guessing)
					{
						e2_AST = (Spring.Expressions.SpringAST)returnAST;
						astFactory.addASTChild(ref currentAST, (AST)returnAST);
					}
					if (0==inputState.guessing)
					{
						relationalExpression_AST = (Spring.Expressions.SpringAST)currentAST.root;
						relationalExpression_AST = (Spring.Expressions.SpringAST) astFactory.make((AST)(Spring.Expressions.SpringAST) astFactory.create(EXPR,op_AST.getText(),GetRelationalOperatorNodeType(op_AST.getText())), (AST)relationalExpression_AST);
						currentAST.root = relationalExpression_AST;
						if ( (null != relationalExpression_AST) && (null != relationalExpression_AST.getFirstChild()) )
							currentAST.child = relationalExpression_AST.getFirstChild();
						else
							currentAST.child = relationalExpression_AST;
						currentAST.advanceChildToEnd();
					}
				}
				else if ((tokenSet_6_.member(LA(1)))) {
				}
				else
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				
			}
			relationalExpression_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_6_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = relationalExpression_AST;
	}
	
	public void sumExpr() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST sumExpr_AST = null;
		
		try {      // for error handling
			prodExpr();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==PLUS||LA(1)==MINUS))
					{
						{
							if ((LA(1)==PLUS))
							{
								Spring.Expressions.OpADD tmp13_AST = null;
								tmp13_AST = (Spring.Expressions.OpADD) astFactory.create(LT(1), "Spring.Expressions.OpADD");
								astFactory.makeASTRoot(ref currentAST, (AST)tmp13_AST);
								match(PLUS);
							}
							else if ((LA(1)==MINUS)) {
								Spring.Expressions.OpSUBTRACT tmp14_AST = null;
								tmp14_AST = (Spring.Expressions.OpSUBTRACT) astFactory.create(LT(1), "Spring.Expressions.OpSUBTRACT");
								astFactory.makeASTRoot(ref currentAST, (AST)tmp14_AST);
								match(MINUS);
							}
							else
							{
								throw new NoViableAltException(LT(1), getFilename());
							}
							
						}
						prodExpr();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
					}
					else
					{
						goto _loop22_breakloop;
					}
					
				}
_loop22_breakloop:				;
			}    // ( ... )*
			sumExpr_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_7_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = sumExpr_AST;
	}
	
	public void relationalOperator() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST relationalOperator_AST = null;
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case EQUAL:
			{
				Spring.Expressions.SpringAST tmp15_AST = null;
				tmp15_AST = (Spring.Expressions.SpringAST) astFactory.create(LT(1));
				astFactory.addASTChild(ref currentAST, (AST)tmp15_AST);
				match(EQUAL);
				relationalOperator_AST = (Spring.Expressions.SpringAST)currentAST.root;
				break;
			}
			case NOT_EQUAL:
			{
				Spring.Expressions.SpringAST tmp16_AST = null;
				tmp16_AST = (Spring.Expressions.SpringAST) astFactory.create(LT(1));
				astFactory.addASTChild(ref currentAST, (AST)tmp16_AST);
				match(NOT_EQUAL);
				relationalOperator_AST = (Spring.Expressions.SpringAST)currentAST.root;
				break;
			}
			case LESS_THAN:
			{
				Spring.Expressions.SpringAST tmp17_AST = null;
				tmp17_AST = (Spring.Expressions.SpringAST) astFactory.create(LT(1));
				astFactory.addASTChild(ref currentAST, (AST)tmp17_AST);
				match(LESS_THAN);
				relationalOperator_AST = (Spring.Expressions.SpringAST)currentAST.root;
				break;
			}
			case LESS_THAN_OR_EQUAL:
			{
				Spring.Expressions.SpringAST tmp18_AST = null;
				tmp18_AST = (Spring.Expressions.SpringAST) astFactory.create(LT(1));
				astFactory.addASTChild(ref currentAST, (AST)tmp18_AST);
				match(LESS_THAN_OR_EQUAL);
				relationalOperator_AST = (Spring.Expressions.SpringAST)currentAST.root;
				break;
			}
			case GREATER_THAN:
			{
				Spring.Expressions.SpringAST tmp19_AST = null;
				tmp19_AST = (Spring.Expressions.SpringAST) astFactory.create(LT(1));
				astFactory.addASTChild(ref currentAST, (AST)tmp19_AST);
				match(GREATER_THAN);
				relationalOperator_AST = (Spring.Expressions.SpringAST)currentAST.root;
				break;
			}
			case GREATER_THAN_OR_EQUAL:
			{
				Spring.Expressions.SpringAST tmp20_AST = null;
				tmp20_AST = (Spring.Expressions.SpringAST) astFactory.create(LT(1));
				astFactory.addASTChild(ref currentAST, (AST)tmp20_AST);
				match(GREATER_THAN_OR_EQUAL);
				relationalOperator_AST = (Spring.Expressions.SpringAST)currentAST.root;
				break;
			}
			case IN:
			{
				Spring.Expressions.SpringAST tmp21_AST = null;
				tmp21_AST = (Spring.Expressions.SpringAST) astFactory.create(LT(1));
				astFactory.addASTChild(ref currentAST, (AST)tmp21_AST);
				match(IN);
				relationalOperator_AST = (Spring.Expressions.SpringAST)currentAST.root;
				break;
			}
			case IS:
			{
				Spring.Expressions.SpringAST tmp22_AST = null;
				tmp22_AST = (Spring.Expressions.SpringAST) astFactory.create(LT(1));
				astFactory.addASTChild(ref currentAST, (AST)tmp22_AST);
				match(IS);
				relationalOperator_AST = (Spring.Expressions.SpringAST)currentAST.root;
				break;
			}
			case BETWEEN:
			{
				Spring.Expressions.SpringAST tmp23_AST = null;
				tmp23_AST = (Spring.Expressions.SpringAST) astFactory.create(LT(1));
				astFactory.addASTChild(ref currentAST, (AST)tmp23_AST);
				match(BETWEEN);
				relationalOperator_AST = (Spring.Expressions.SpringAST)currentAST.root;
				break;
			}
			case LIKE:
			{
				Spring.Expressions.SpringAST tmp24_AST = null;
				tmp24_AST = (Spring.Expressions.SpringAST) astFactory.create(LT(1));
				astFactory.addASTChild(ref currentAST, (AST)tmp24_AST);
				match(LIKE);
				relationalOperator_AST = (Spring.Expressions.SpringAST)currentAST.root;
				break;
			}
			case MATCHES:
			{
				Spring.Expressions.SpringAST tmp25_AST = null;
				tmp25_AST = (Spring.Expressions.SpringAST) astFactory.create(LT(1));
				astFactory.addASTChild(ref currentAST, (AST)tmp25_AST);
				match(MATCHES);
				relationalOperator_AST = (Spring.Expressions.SpringAST)currentAST.root;
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_8_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = relationalOperator_AST;
	}
	
	public void prodExpr() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST prodExpr_AST = null;
		
		try {      // for error handling
			powExpr();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			{    // ( ... )*
				for (;;)
				{
					if (((LA(1) >= STAR && LA(1) <= MOD)))
					{
						{
							switch ( LA(1) )
							{
							case STAR:
							{
								Spring.Expressions.OpMULTIPLY tmp26_AST = null;
								tmp26_AST = (Spring.Expressions.OpMULTIPLY) astFactory.create(LT(1), "Spring.Expressions.OpMULTIPLY");
								astFactory.makeASTRoot(ref currentAST, (AST)tmp26_AST);
								match(STAR);
								break;
							}
							case DIV:
							{
								Spring.Expressions.OpDIVIDE tmp27_AST = null;
								tmp27_AST = (Spring.Expressions.OpDIVIDE) astFactory.create(LT(1), "Spring.Expressions.OpDIVIDE");
								astFactory.makeASTRoot(ref currentAST, (AST)tmp27_AST);
								match(DIV);
								break;
							}
							case MOD:
							{
								Spring.Expressions.OpMODULUS tmp28_AST = null;
								tmp28_AST = (Spring.Expressions.OpMODULUS) astFactory.create(LT(1), "Spring.Expressions.OpMODULUS");
								astFactory.makeASTRoot(ref currentAST, (AST)tmp28_AST);
								match(MOD);
								break;
							}
							default:
							{
								throw new NoViableAltException(LT(1), getFilename());
							}
							 }
						}
						powExpr();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
					}
					else
					{
						goto _loop26_breakloop;
					}
					
				}
_loop26_breakloop:				;
			}    // ( ... )*
			prodExpr_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_9_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = prodExpr_AST;
	}
	
	public void powExpr() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST powExpr_AST = null;
		
		try {      // for error handling
			unaryExpression();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			{
				if ((LA(1)==POWER))
				{
					Spring.Expressions.OpPOWER tmp29_AST = null;
					tmp29_AST = (Spring.Expressions.OpPOWER) astFactory.create(LT(1), "Spring.Expressions.OpPOWER");
					astFactory.makeASTRoot(ref currentAST, (AST)tmp29_AST);
					match(POWER);
					unaryExpression();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(ref currentAST, (AST)returnAST);
					}
				}
				else if ((tokenSet_10_.member(LA(1)))) {
				}
				else
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				
			}
			powExpr_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_10_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = powExpr_AST;
	}
	
	public void unaryExpression() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST unaryExpression_AST = null;
		
		try {      // for error handling
			if ((LA(1)==PLUS||LA(1)==MINUS||LA(1)==BANG))
			{
				{
					switch ( LA(1) )
					{
					case PLUS:
					{
						Spring.Expressions.OpUnaryPlus tmp30_AST = null;
						tmp30_AST = (Spring.Expressions.OpUnaryPlus) astFactory.create(LT(1), "Spring.Expressions.OpUnaryPlus");
						astFactory.makeASTRoot(ref currentAST, (AST)tmp30_AST);
						match(PLUS);
						break;
					}
					case MINUS:
					{
						Spring.Expressions.OpUnaryMinus tmp31_AST = null;
						tmp31_AST = (Spring.Expressions.OpUnaryMinus) astFactory.create(LT(1), "Spring.Expressions.OpUnaryMinus");
						astFactory.makeASTRoot(ref currentAST, (AST)tmp31_AST);
						match(MINUS);
						break;
					}
					case BANG:
					{
						Spring.Expressions.OpNOT tmp32_AST = null;
						tmp32_AST = (Spring.Expressions.OpNOT) astFactory.create(LT(1), "Spring.Expressions.OpNOT");
						astFactory.makeASTRoot(ref currentAST, (AST)tmp32_AST);
						match(BANG);
						break;
					}
					default:
					{
						throw new NoViableAltException(LT(1), getFilename());
					}
					 }
				}
				unaryExpression();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(ref currentAST, (AST)returnAST);
				}
				unaryExpression_AST = (Spring.Expressions.SpringAST)currentAST.root;
			}
			else if ((tokenSet_11_.member(LA(1)))) {
				primaryExpression();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(ref currentAST, (AST)returnAST);
				}
				unaryExpression_AST = (Spring.Expressions.SpringAST)currentAST.root;
			}
			else
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_12_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = unaryExpression_AST;
	}
	
	public void primaryExpression() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST primaryExpression_AST = null;
		
		try {      // for error handling
			startNode();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			{
				if ((tokenSet_13_.member(LA(1))))
				{
					node();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(ref currentAST, (AST)returnAST);
					}
				}
				else if ((tokenSet_12_.member(LA(1)))) {
				}
				else
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				
			}
			if (0==inputState.guessing)
			{
				primaryExpression_AST = (Spring.Expressions.SpringAST)currentAST.root;
				primaryExpression_AST = (Spring.Expressions.SpringAST) astFactory.make((AST)(Spring.Expressions.SpringAST) astFactory.create(EXPR,"expression","Spring.Expressions.Expression"), (AST)primaryExpression_AST);
				currentAST.root = primaryExpression_AST;
				if ( (null != primaryExpression_AST) && (null != primaryExpression_AST.getFirstChild()) )
					currentAST.child = primaryExpression_AST.getFirstChild();
				else
					currentAST.child = primaryExpression_AST;
				currentAST.advanceChildToEnd();
			}
			primaryExpression_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_12_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = primaryExpression_AST;
	}
	
	public void unaryOperator() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST unaryOperator_AST = null;
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case PLUS:
			{
				Spring.Expressions.SpringAST tmp33_AST = null;
				tmp33_AST = (Spring.Expressions.SpringAST) astFactory.create(LT(1));
				astFactory.addASTChild(ref currentAST, (AST)tmp33_AST);
				match(PLUS);
				unaryOperator_AST = (Spring.Expressions.SpringAST)currentAST.root;
				break;
			}
			case MINUS:
			{
				Spring.Expressions.SpringAST tmp34_AST = null;
				tmp34_AST = (Spring.Expressions.SpringAST) astFactory.create(LT(1));
				astFactory.addASTChild(ref currentAST, (AST)tmp34_AST);
				match(MINUS);
				unaryOperator_AST = (Spring.Expressions.SpringAST)currentAST.root;
				break;
			}
			case BANG:
			{
				Spring.Expressions.SpringAST tmp35_AST = null;
				tmp35_AST = (Spring.Expressions.SpringAST) astFactory.create(LT(1));
				astFactory.addASTChild(ref currentAST, (AST)tmp35_AST);
				match(BANG);
				unaryOperator_AST = (Spring.Expressions.SpringAST)currentAST.root;
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_0_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = unaryOperator_AST;
	}
	
	public void startNode() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST startNode_AST = null;
		
		try {      // for error handling
			{
				switch ( LA(1) )
				{
				case ID:
				{
					methodOrProperty();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(ref currentAST, (AST)returnAST);
					}
					break;
				}
				case DOLLAR:
				{
					localFunctionOrVar();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(ref currentAST, (AST)returnAST);
					}
					break;
				}
				case LBRACKET:
				{
					indexer();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(ref currentAST, (AST)returnAST);
					}
					break;
				}
				case FALSE:
				case TRUE:
				case NULL_LITERAL:
				case STRING_LITERAL:
				case INTEGER_LITERAL:
				case HEXADECIMAL_INTEGER_LITERAL:
				case REAL_LITERAL:
				case LITERAL_date:
				{
					literal();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(ref currentAST, (AST)returnAST);
					}
					break;
				}
				case TYPE:
				{
					type();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(ref currentAST, (AST)returnAST);
					}
					break;
				}
				case LITERAL_new:
				{
					constructor();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(ref currentAST, (AST)returnAST);
					}
					break;
				}
				case PROJECT:
				{
					projection();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(ref currentAST, (AST)returnAST);
					}
					break;
				}
				case SELECT:
				{
					selection();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(ref currentAST, (AST)returnAST);
					}
					break;
				}
				case SELECT_FIRST:
				{
					firstSelection();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(ref currentAST, (AST)returnAST);
					}
					break;
				}
				case SELECT_LAST:
				{
					lastSelection();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(ref currentAST, (AST)returnAST);
					}
					break;
				}
				case LCURLY:
				{
					listInitializer();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(ref currentAST, (AST)returnAST);
					}
					break;
				}
				case LAMBDA:
				{
					lambda();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(ref currentAST, (AST)returnAST);
					}
					break;
				}
				default:
					bool synPredMatched37 = false;
					if (((LA(1)==LPAREN) && (tokenSet_8_.member(LA(2)))))
					{
						int _m37 = mark();
						synPredMatched37 = true;
						inputState.guessing++;
						try {
							{
								match(LPAREN);
								expression();
								match(SEMI);
							}
						}
						catch (RecognitionException)
						{
							synPredMatched37 = false;
						}
						rewind(_m37);
						inputState.guessing--;
					}
					if ( synPredMatched37 )
					{
						exprList();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
					}
					else if ((LA(1)==LPAREN) && (tokenSet_8_.member(LA(2)))) {
						parenExpr();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
					}
					else if ((LA(1)==POUND) && (LA(2)==ID)) {
						functionOrVar();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
					}
					else if ((LA(1)==AT) && (LA(2)==LPAREN)) {
						reference();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
					}
					else if ((LA(1)==POUND) && (LA(2)==LCURLY)) {
						mapInitializer();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
					}
					else if ((LA(1)==AT) && (LA(2)==LBRACKET)) {
						attribute();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
					}
				else
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				break; }
			}
			startNode_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = startNode_AST;
	}
	
	public void node() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST node_AST = null;
		
		try {      // for error handling
			{ // ( ... )+
				int _cnt40=0;
				for (;;)
				{
					switch ( LA(1) )
					{
					case ID:
					{
						methodOrProperty();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
						break;
					}
					case LBRACKET:
					{
						indexer();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
						break;
					}
					case PROJECT:
					{
						projection();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
						break;
					}
					case SELECT:
					{
						selection();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
						break;
					}
					case SELECT_FIRST:
					{
						firstSelection();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
						break;
					}
					case SELECT_LAST:
					{
						lastSelection();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
						break;
					}
					case LPAREN:
					{
						exprList();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
						break;
					}
					case DOT:
					{
						match(DOT);
						break;
					}
					default:
					{
						if (_cnt40 >= 1) { goto _loop40_breakloop; } else { throw new NoViableAltException(LT(1), getFilename());; }
					}
					break; }
					_cnt40++;
				}
_loop40_breakloop:				;
			}    // ( ... )+
			node_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_12_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = node_AST;
	}
	
	public void methodOrProperty() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST methodOrProperty_AST = null;
		
		try {      // for error handling
			bool synPredMatched53 = false;
			if (((LA(1)==ID) && (LA(2)==LPAREN)))
			{
				int _m53 = mark();
				synPredMatched53 = true;
				inputState.guessing++;
				try {
					{
						match(ID);
						match(LPAREN);
					}
				}
				catch (RecognitionException)
				{
					synPredMatched53 = false;
				}
				rewind(_m53);
				inputState.guessing--;
			}
			if ( synPredMatched53 )
			{
				Spring.Expressions.MethodNode tmp37_AST = null;
				tmp37_AST = (Spring.Expressions.MethodNode) astFactory.create(LT(1), "Spring.Expressions.MethodNode");
				astFactory.makeASTRoot(ref currentAST, (AST)tmp37_AST);
				match(ID);
				methodArgs();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(ref currentAST, (AST)returnAST);
				}
				methodOrProperty_AST = (Spring.Expressions.SpringAST)currentAST.root;
			}
			else if ((LA(1)==ID) && (tokenSet_2_.member(LA(2)))) {
				property();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(ref currentAST, (AST)returnAST);
				}
				methodOrProperty_AST = (Spring.Expressions.SpringAST)currentAST.root;
			}
			else
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = methodOrProperty_AST;
	}
	
	public void functionOrVar() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST functionOrVar_AST = null;
		
		try {      // for error handling
			bool synPredMatched43 = false;
			if (((LA(1)==POUND) && (LA(2)==ID)))
			{
				int _m43 = mark();
				synPredMatched43 = true;
				inputState.guessing++;
				try {
					{
						match(POUND);
						match(ID);
						match(LPAREN);
					}
				}
				catch (RecognitionException)
				{
					synPredMatched43 = false;
				}
				rewind(_m43);
				inputState.guessing--;
			}
			if ( synPredMatched43 )
			{
				function();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(ref currentAST, (AST)returnAST);
				}
				functionOrVar_AST = (Spring.Expressions.SpringAST)currentAST.root;
			}
			else if ((LA(1)==POUND) && (LA(2)==ID)) {
				var();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(ref currentAST, (AST)returnAST);
				}
				functionOrVar_AST = (Spring.Expressions.SpringAST)currentAST.root;
			}
			else
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = functionOrVar_AST;
	}
	
	public void localFunctionOrVar() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST localFunctionOrVar_AST = null;
		
		try {      // for error handling
			bool synPredMatched48 = false;
			if (((LA(1)==DOLLAR) && (LA(2)==ID)))
			{
				int _m48 = mark();
				synPredMatched48 = true;
				inputState.guessing++;
				try {
					{
						match(DOLLAR);
						match(ID);
						match(LPAREN);
					}
				}
				catch (RecognitionException)
				{
					synPredMatched48 = false;
				}
				rewind(_m48);
				inputState.guessing--;
			}
			if ( synPredMatched48 )
			{
				localFunction();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(ref currentAST, (AST)returnAST);
				}
				localFunctionOrVar_AST = (Spring.Expressions.SpringAST)currentAST.root;
			}
			else if ((LA(1)==DOLLAR) && (LA(2)==ID)) {
				localVar();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(ref currentAST, (AST)returnAST);
				}
				localFunctionOrVar_AST = (Spring.Expressions.SpringAST)currentAST.root;
			}
			else
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = localFunctionOrVar_AST;
	}
	
	public void reference() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST reference_AST = null;
		Spring.Expressions.SpringAST cn_AST = null;
		Spring.Expressions.SpringAST id_AST = null;
		Spring.Expressions.SpringAST localid_AST = null;
		
		try {      // for error handling
			bool synPredMatched61 = false;
			if (((LA(1)==AT) && (LA(2)==LPAREN)))
			{
				int _m61 = mark();
				synPredMatched61 = true;
				inputState.guessing++;
				try {
					{
						match(AT);
						match(LPAREN);
						quotableName();
						match(COLON);
					}
				}
				catch (RecognitionException)
				{
					synPredMatched61 = false;
				}
				rewind(_m61);
				inputState.guessing--;
			}
			if ( synPredMatched61 )
			{
				match(AT);
				match(LPAREN);
				quotableName();
				if (0 == inputState.guessing)
				{
					cn_AST = (Spring.Expressions.SpringAST)returnAST;
				}
				match(COLON);
				quotableName();
				if (0 == inputState.guessing)
				{
					id_AST = (Spring.Expressions.SpringAST)returnAST;
				}
				match(RPAREN);
				if (0==inputState.guessing)
				{
					reference_AST = (Spring.Expressions.SpringAST)currentAST.root;
					reference_AST = (Spring.Expressions.SpringAST) astFactory.make((AST)(Spring.Expressions.SpringAST) astFactory.create(EXPR,"ref","Spring.Context.Support.ReferenceNode"), (AST)cn_AST, (AST)id_AST);
					currentAST.root = reference_AST;
					if ( (null != reference_AST) && (null != reference_AST.getFirstChild()) )
						currentAST.child = reference_AST.getFirstChild();
					else
						currentAST.child = reference_AST;
					currentAST.advanceChildToEnd();
				}
				reference_AST = (Spring.Expressions.SpringAST)currentAST.root;
			}
			else if ((LA(1)==AT) && (LA(2)==LPAREN)) {
				match(AT);
				match(LPAREN);
				quotableName();
				if (0 == inputState.guessing)
				{
					localid_AST = (Spring.Expressions.SpringAST)returnAST;
				}
				match(RPAREN);
				if (0==inputState.guessing)
				{
					reference_AST = (Spring.Expressions.SpringAST)currentAST.root;
					reference_AST = (Spring.Expressions.SpringAST) astFactory.make((AST)(Spring.Expressions.SpringAST) astFactory.create(EXPR,"ref","Spring.Context.Support.ReferenceNode"), (AST)null, (AST)localid_AST);
					currentAST.root = reference_AST;
					if ( (null != reference_AST) && (null != reference_AST.getFirstChild()) )
						currentAST.child = reference_AST.getFirstChild();
					else
						currentAST.child = reference_AST;
					currentAST.advanceChildToEnd();
				}
				reference_AST = (Spring.Expressions.SpringAST)currentAST.root;
			}
			else
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = reference_AST;
	}
	
	public void indexer() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST indexer_AST = null;
		
		try {      // for error handling
			Spring.Expressions.IndexerNode tmp45_AST = null;
			tmp45_AST = (Spring.Expressions.IndexerNode) astFactory.create(LT(1), "Spring.Expressions.IndexerNode");
			astFactory.makeASTRoot(ref currentAST, (AST)tmp45_AST);
			match(LBRACKET);
			argument();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==COMMA))
					{
						match(COMMA);
						argument();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
					}
					else
					{
						goto _loop64_breakloop;
					}
					
				}
_loop64_breakloop:				;
			}    // ( ... )*
			match(RBRACKET);
			indexer_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = indexer_AST;
	}
	
	public void literal() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST literal_AST = null;
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case NULL_LITERAL:
			{
				Spring.Expressions.NullLiteralNode tmp48_AST = null;
				tmp48_AST = (Spring.Expressions.NullLiteralNode) astFactory.create(LT(1), "Spring.Expressions.NullLiteralNode");
				astFactory.addASTChild(ref currentAST, (AST)tmp48_AST);
				match(NULL_LITERAL);
				literal_AST = (Spring.Expressions.SpringAST)currentAST.root;
				break;
			}
			case INTEGER_LITERAL:
			{
				Spring.Expressions.IntLiteralNode tmp49_AST = null;
				tmp49_AST = (Spring.Expressions.IntLiteralNode) astFactory.create(LT(1), "Spring.Expressions.IntLiteralNode");
				astFactory.addASTChild(ref currentAST, (AST)tmp49_AST);
				match(INTEGER_LITERAL);
				literal_AST = (Spring.Expressions.SpringAST)currentAST.root;
				break;
			}
			case HEXADECIMAL_INTEGER_LITERAL:
			{
				Spring.Expressions.HexLiteralNode tmp50_AST = null;
				tmp50_AST = (Spring.Expressions.HexLiteralNode) astFactory.create(LT(1), "Spring.Expressions.HexLiteralNode");
				astFactory.addASTChild(ref currentAST, (AST)tmp50_AST);
				match(HEXADECIMAL_INTEGER_LITERAL);
				literal_AST = (Spring.Expressions.SpringAST)currentAST.root;
				break;
			}
			case REAL_LITERAL:
			{
				Spring.Expressions.RealLiteralNode tmp51_AST = null;
				tmp51_AST = (Spring.Expressions.RealLiteralNode) astFactory.create(LT(1), "Spring.Expressions.RealLiteralNode");
				astFactory.addASTChild(ref currentAST, (AST)tmp51_AST);
				match(REAL_LITERAL);
				literal_AST = (Spring.Expressions.SpringAST)currentAST.root;
				break;
			}
			case STRING_LITERAL:
			{
				Spring.Expressions.StringLiteralNode tmp52_AST = null;
				tmp52_AST = (Spring.Expressions.StringLiteralNode) astFactory.create(LT(1), "Spring.Expressions.StringLiteralNode");
				astFactory.addASTChild(ref currentAST, (AST)tmp52_AST);
				match(STRING_LITERAL);
				literal_AST = (Spring.Expressions.SpringAST)currentAST.root;
				break;
			}
			case FALSE:
			case TRUE:
			{
				boolLiteral();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(ref currentAST, (AST)returnAST);
				}
				literal_AST = (Spring.Expressions.SpringAST)currentAST.root;
				break;
			}
			case LITERAL_date:
			{
				dateLiteral();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(ref currentAST, (AST)returnAST);
				}
				literal_AST = (Spring.Expressions.SpringAST)currentAST.root;
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = literal_AST;
	}
	
	public void type() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST type_AST = null;
		Spring.Expressions.SpringAST tn_AST = null;
		
		try {      // for error handling
			match(TYPE);
			name();
			if (0 == inputState.guessing)
			{
				tn_AST = (Spring.Expressions.SpringAST)returnAST;
			}
			match(RPAREN);
			if (0==inputState.guessing)
			{
				type_AST = (Spring.Expressions.SpringAST)currentAST.root;
				type_AST = (Spring.Expressions.SpringAST) astFactory.make((AST)(Spring.Expressions.SpringAST) astFactory.create(EXPR,tn_AST.getText(),"Spring.Expressions.TypeNode"), (AST)type_AST);
				currentAST.root = type_AST;
				if ( (null != type_AST) && (null != type_AST.getFirstChild()) )
					currentAST.child = type_AST.getFirstChild();
				else
					currentAST.child = type_AST;
				currentAST.advanceChildToEnd();
			}
			type_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = type_AST;
	}
	
	public void constructor() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST constructor_AST = null;
		Spring.Expressions.SpringAST type_AST = null;
		
		try {      // for error handling
			bool synPredMatched87 = false;
			if (((LA(1)==LITERAL_new) && (LA(2)==ID)))
			{
				int _m87 = mark();
				synPredMatched87 = true;
				inputState.guessing++;
				try {
					{
						match(LITERAL_new);
						qualifiedId();
						match(LPAREN);
					}
				}
				catch (RecognitionException)
				{
					synPredMatched87 = false;
				}
				rewind(_m87);
				inputState.guessing--;
			}
			if ( synPredMatched87 )
			{
				match(LITERAL_new);
				qualifiedId();
				if (0 == inputState.guessing)
				{
					type_AST = (Spring.Expressions.SpringAST)returnAST;
				}
				ctorArgs();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(ref currentAST, (AST)returnAST);
				}
				if (0==inputState.guessing)
				{
					constructor_AST = (Spring.Expressions.SpringAST)currentAST.root;
					constructor_AST = (Spring.Expressions.SpringAST) astFactory.make((AST)(Spring.Expressions.SpringAST) astFactory.create(EXPR,type_AST.getText(),"Spring.Expressions.ConstructorNode"), (AST)constructor_AST);
					currentAST.root = constructor_AST;
					if ( (null != constructor_AST) && (null != constructor_AST.getFirstChild()) )
						currentAST.child = constructor_AST.getFirstChild();
					else
						currentAST.child = constructor_AST;
					currentAST.advanceChildToEnd();
				}
				constructor_AST = (Spring.Expressions.SpringAST)currentAST.root;
			}
			else if ((LA(1)==LITERAL_new) && (LA(2)==ID)) {
				arrayConstructor();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(ref currentAST, (AST)returnAST);
				}
				constructor_AST = (Spring.Expressions.SpringAST)currentAST.root;
			}
			else
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = constructor_AST;
	}
	
	public void projection() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST projection_AST = null;
		
		try {      // for error handling
			Spring.Expressions.ProjectionNode tmp56_AST = null;
			tmp56_AST = (Spring.Expressions.ProjectionNode) astFactory.create(LT(1), "Spring.Expressions.ProjectionNode");
			astFactory.makeASTRoot(ref currentAST, (AST)tmp56_AST);
			match(PROJECT);
			expression();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			match(RCURLY);
			projection_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = projection_AST;
	}
	
	public void selection() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST selection_AST = null;
		
		try {      // for error handling
			Spring.Expressions.SelectionNode tmp58_AST = null;
			tmp58_AST = (Spring.Expressions.SelectionNode) astFactory.create(LT(1), "Spring.Expressions.SelectionNode");
			astFactory.makeASTRoot(ref currentAST, (AST)tmp58_AST);
			match(SELECT);
			expression();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==COMMA))
					{
						match(COMMA);
						expression();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
					}
					else
					{
						goto _loop68_breakloop;
					}
					
				}
_loop68_breakloop:				;
			}    // ( ... )*
			match(RCURLY);
			selection_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = selection_AST;
	}
	
	public void firstSelection() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST firstSelection_AST = null;
		
		try {      // for error handling
			Spring.Expressions.SelectionFirstNode tmp61_AST = null;
			tmp61_AST = (Spring.Expressions.SelectionFirstNode) astFactory.create(LT(1), "Spring.Expressions.SelectionFirstNode");
			astFactory.makeASTRoot(ref currentAST, (AST)tmp61_AST);
			match(SELECT_FIRST);
			expression();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			match(RCURLY);
			firstSelection_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = firstSelection_AST;
	}
	
	public void lastSelection() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST lastSelection_AST = null;
		
		try {      // for error handling
			Spring.Expressions.SelectionLastNode tmp63_AST = null;
			tmp63_AST = (Spring.Expressions.SelectionLastNode) astFactory.create(LT(1), "Spring.Expressions.SelectionLastNode");
			astFactory.makeASTRoot(ref currentAST, (AST)tmp63_AST);
			match(SELECT_LAST);
			expression();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			match(RCURLY);
			lastSelection_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = lastSelection_AST;
	}
	
	public void listInitializer() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST listInitializer_AST = null;
		
		try {      // for error handling
			Spring.Expressions.ListInitializerNode tmp65_AST = null;
			tmp65_AST = (Spring.Expressions.ListInitializerNode) astFactory.create(LT(1), "Spring.Expressions.ListInitializerNode");
			astFactory.makeASTRoot(ref currentAST, (AST)tmp65_AST);
			match(LCURLY);
			expression();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==COMMA))
					{
						match(COMMA);
						expression();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
					}
					else
					{
						goto _loop96_breakloop;
					}
					
				}
_loop96_breakloop:				;
			}    // ( ... )*
			match(RCURLY);
			listInitializer_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = listInitializer_AST;
	}
	
	public void mapInitializer() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST mapInitializer_AST = null;
		
		try {      // for error handling
			match(POUND);
			Spring.Expressions.MapInitializerNode tmp69_AST = null;
			tmp69_AST = (Spring.Expressions.MapInitializerNode) astFactory.create(LT(1), "Spring.Expressions.MapInitializerNode");
			astFactory.makeASTRoot(ref currentAST, (AST)tmp69_AST);
			match(LCURLY);
			mapEntry();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==COMMA))
					{
						match(COMMA);
						mapEntry();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
					}
					else
					{
						goto _loop99_breakloop;
					}
					
				}
_loop99_breakloop:				;
			}    // ( ... )*
			match(RCURLY);
			mapInitializer_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = mapInitializer_AST;
	}
	
	public void lambda() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST lambda_AST = null;
		
		try {      // for error handling
			match(LAMBDA);
			{
				if ((LA(1)==ID))
				{
					argList();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(ref currentAST, (AST)returnAST);
					}
				}
				else if ((LA(1)==PIPE)) {
				}
				else
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				
			}
			match(PIPE);
			expression();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			match(RCURLY);
			if (0==inputState.guessing)
			{
				lambda_AST = (Spring.Expressions.SpringAST)currentAST.root;
				lambda_AST = (Spring.Expressions.SpringAST) astFactory.make((AST)(Spring.Expressions.SpringAST) astFactory.create(EXPR,"lambda","Spring.Expressions.LambdaExpressionNode"), (AST)lambda_AST);
				currentAST.root = lambda_AST;
				if ( (null != lambda_AST) && (null != lambda_AST.getFirstChild()) )
					currentAST.child = lambda_AST.getFirstChild();
				else
					currentAST.child = lambda_AST;
				currentAST.advanceChildToEnd();
			}
			lambda_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = lambda_AST;
	}
	
	public void attribute() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST attribute_AST = null;
		Spring.Expressions.SpringAST tn_AST = null;
		
		try {      // for error handling
			match(AT);
			match(LBRACKET);
			qualifiedId();
			if (0 == inputState.guessing)
			{
				tn_AST = (Spring.Expressions.SpringAST)returnAST;
			}
			{
				if ((LA(1)==LPAREN))
				{
					ctorArgs();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(ref currentAST, (AST)returnAST);
					}
				}
				else if ((LA(1)==RBRACKET)) {
				}
				else
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				
			}
			match(RBRACKET);
			if (0==inputState.guessing)
			{
				attribute_AST = (Spring.Expressions.SpringAST)currentAST.root;
				attribute_AST = (Spring.Expressions.SpringAST) astFactory.make((AST)(Spring.Expressions.SpringAST) astFactory.create(EXPR,tn_AST.getText(),"Spring.Expressions.AttributeNode"), (AST)attribute_AST);
				currentAST.root = attribute_AST;
				if ( (null != attribute_AST) && (null != attribute_AST.getFirstChild()) )
					currentAST.child = attribute_AST.getFirstChild();
				else
					currentAST.child = attribute_AST;
				currentAST.advanceChildToEnd();
			}
			attribute_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = attribute_AST;
	}
	
	public void function() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST function_AST = null;
		
		try {      // for error handling
			match(POUND);
			Spring.Expressions.FunctionNode tmp79_AST = null;
			tmp79_AST = (Spring.Expressions.FunctionNode) astFactory.create(LT(1), "Spring.Expressions.FunctionNode");
			astFactory.makeASTRoot(ref currentAST, (AST)tmp79_AST);
			match(ID);
			methodArgs();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			function_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = function_AST;
	}
	
	public void var() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST var_AST = null;
		
		try {      // for error handling
			match(POUND);
			Spring.Expressions.VariableNode tmp81_AST = null;
			tmp81_AST = (Spring.Expressions.VariableNode) astFactory.create(LT(1), "Spring.Expressions.VariableNode");
			astFactory.makeASTRoot(ref currentAST, (AST)tmp81_AST);
			match(ID);
			var_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = var_AST;
	}
	
	public void methodArgs() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST methodArgs_AST = null;
		
		try {      // for error handling
			match(LPAREN);
			{
				if ((tokenSet_8_.member(LA(1))))
				{
					argument();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(ref currentAST, (AST)returnAST);
					}
					{    // ( ... )*
						for (;;)
						{
							if ((LA(1)==COMMA))
							{
								match(COMMA);
								argument();
								if (0 == inputState.guessing)
								{
									astFactory.addASTChild(ref currentAST, (AST)returnAST);
								}
							}
							else
							{
								goto _loop57_breakloop;
							}
							
						}
_loop57_breakloop:						;
					}    // ( ... )*
				}
				else if ((LA(1)==RPAREN)) {
				}
				else
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				
			}
			match(RPAREN);
			methodArgs_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = methodArgs_AST;
	}
	
	public void localFunction() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST localFunction_AST = null;
		
		try {      // for error handling
			match(DOLLAR);
			Spring.Expressions.LocalFunctionNode tmp86_AST = null;
			tmp86_AST = (Spring.Expressions.LocalFunctionNode) astFactory.create(LT(1), "Spring.Expressions.LocalFunctionNode");
			astFactory.makeASTRoot(ref currentAST, (AST)tmp86_AST);
			match(ID);
			methodArgs();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			localFunction_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = localFunction_AST;
	}
	
	public void localVar() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST localVar_AST = null;
		
		try {      // for error handling
			match(DOLLAR);
			Spring.Expressions.LocalVariableNode tmp88_AST = null;
			tmp88_AST = (Spring.Expressions.LocalVariableNode) astFactory.create(LT(1), "Spring.Expressions.LocalVariableNode");
			astFactory.makeASTRoot(ref currentAST, (AST)tmp88_AST);
			match(ID);
			localVar_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = localVar_AST;
	}
	
	public void property() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST property_AST = null;
		
		try {      // for error handling
			Spring.Expressions.PropertyOrFieldNode tmp89_AST = null;
			tmp89_AST = (Spring.Expressions.PropertyOrFieldNode) astFactory.create(LT(1), "Spring.Expressions.PropertyOrFieldNode");
			astFactory.addASTChild(ref currentAST, (AST)tmp89_AST);
			match(ID);
			property_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = property_AST;
	}
	
	public void argument() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST argument_AST = null;
		
		try {      // for error handling
			expression();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			argument_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_14_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = argument_AST;
	}
	
	public void quotableName() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST quotableName_AST = null;
		
		try {      // for error handling
			if ((LA(1)==STRING_LITERAL))
			{
				Spring.Expressions.QualifiedIdentifier tmp90_AST = null;
				tmp90_AST = (Spring.Expressions.QualifiedIdentifier) astFactory.create(LT(1), "Spring.Expressions.QualifiedIdentifier");
				astFactory.makeASTRoot(ref currentAST, (AST)tmp90_AST);
				match(STRING_LITERAL);
				quotableName_AST = (Spring.Expressions.SpringAST)currentAST.root;
			}
			else if ((LA(1)==ID)) {
				name();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(ref currentAST, (AST)returnAST);
				}
				quotableName_AST = (Spring.Expressions.SpringAST)currentAST.root;
			}
			else
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_15_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = quotableName_AST;
	}
	
	public void name() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST name_AST = null;
		
		try {      // for error handling
			Spring.Expressions.QualifiedIdentifier tmp91_AST = null;
			tmp91_AST = (Spring.Expressions.QualifiedIdentifier) astFactory.create(LT(1), "Spring.Expressions.QualifiedIdentifier");
			astFactory.makeASTRoot(ref currentAST, (AST)tmp91_AST);
			match(ID);
			{    // ( ... )*
				for (;;)
				{
					if ((tokenSet_16_.member(LA(1))))
					{
						{
							Spring.Expressions.SpringAST tmp92_AST = null;
							tmp92_AST = (Spring.Expressions.SpringAST) astFactory.create(LT(1));
							astFactory.addASTChild(ref currentAST, (AST)tmp92_AST);
							match(tokenSet_16_);
						}
					}
					else
					{
						goto _loop75_breakloop;
					}
					
				}
_loop75_breakloop:				;
			}    // ( ... )*
			name_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_15_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = name_AST;
	}
	
	public void qualifiedId() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST qualifiedId_AST = null;
		
		try {      // for error handling
			Spring.Expressions.QualifiedIdentifier tmp93_AST = null;
			tmp93_AST = (Spring.Expressions.QualifiedIdentifier) astFactory.create(LT(1), "Spring.Expressions.QualifiedIdentifier");
			astFactory.makeASTRoot(ref currentAST, (AST)tmp93_AST);
			match(ID);
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==DOT))
					{
						Spring.Expressions.SpringAST tmp94_AST = null;
						tmp94_AST = (Spring.Expressions.SpringAST) astFactory.create(LT(1));
						astFactory.addASTChild(ref currentAST, (AST)tmp94_AST);
						match(DOT);
						Spring.Expressions.SpringAST tmp95_AST = null;
						tmp95_AST = (Spring.Expressions.SpringAST) astFactory.create(LT(1));
						astFactory.addASTChild(ref currentAST, (AST)tmp95_AST);
						match(ID);
					}
					else
					{
						goto _loop111_breakloop;
					}
					
				}
_loop111_breakloop:				;
			}    // ( ... )*
			qualifiedId_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_17_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = qualifiedId_AST;
	}
	
	public void ctorArgs() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST ctorArgs_AST = null;
		
		try {      // for error handling
			match(LPAREN);
			{
				if ((tokenSet_8_.member(LA(1))))
				{
					namedArgument();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(ref currentAST, (AST)returnAST);
					}
					{    // ( ... )*
						for (;;)
						{
							if ((LA(1)==COMMA))
							{
								match(COMMA);
								namedArgument();
								if (0 == inputState.guessing)
								{
									astFactory.addASTChild(ref currentAST, (AST)returnAST);
								}
							}
							else
							{
								goto _loop104_breakloop;
							}
							
						}
_loop104_breakloop:						;
					}    // ( ... )*
				}
				else if ((LA(1)==RPAREN)) {
				}
				else
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				
			}
			match(RPAREN);
			ctorArgs_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = ctorArgs_AST;
	}
	
	public void argList() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST argList_AST = null;
		
		try {      // for error handling
			{
				Spring.Expressions.SpringAST tmp99_AST = null;
				tmp99_AST = (Spring.Expressions.SpringAST) astFactory.create(LT(1));
				astFactory.addASTChild(ref currentAST, (AST)tmp99_AST);
				match(ID);
				{    // ( ... )*
					for (;;)
					{
						if ((LA(1)==COMMA))
						{
							match(COMMA);
							Spring.Expressions.SpringAST tmp101_AST = null;
							tmp101_AST = (Spring.Expressions.SpringAST) astFactory.create(LT(1));
							astFactory.addASTChild(ref currentAST, (AST)tmp101_AST);
							match(ID);
						}
						else
						{
							goto _loop84_breakloop;
						}
						
					}
_loop84_breakloop:					;
				}    // ( ... )*
			}
			if (0==inputState.guessing)
			{
				argList_AST = (Spring.Expressions.SpringAST)currentAST.root;
				argList_AST = (Spring.Expressions.SpringAST) astFactory.make((AST)(Spring.Expressions.SpringAST) astFactory.create(EXPR,"args"), (AST)argList_AST);
				currentAST.root = argList_AST;
				if ( (null != argList_AST) && (null != argList_AST.getFirstChild()) )
					currentAST.child = argList_AST.getFirstChild();
				else
					currentAST.child = argList_AST;
				currentAST.advanceChildToEnd();
			}
			argList_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_18_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = argList_AST;
	}
	
	public void arrayConstructor() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST arrayConstructor_AST = null;
		Spring.Expressions.SpringAST type_AST = null;
		
		try {      // for error handling
			match(LITERAL_new);
			qualifiedId();
			if (0 == inputState.guessing)
			{
				type_AST = (Spring.Expressions.SpringAST)returnAST;
			}
			arrayRank();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			{
				if ((LA(1)==LCURLY))
				{
					listInitializer();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(ref currentAST, (AST)returnAST);
					}
				}
				else if ((tokenSet_2_.member(LA(1)))) {
				}
				else
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				
			}
			if (0==inputState.guessing)
			{
				arrayConstructor_AST = (Spring.Expressions.SpringAST)currentAST.root;
				arrayConstructor_AST = (Spring.Expressions.SpringAST) astFactory.make((AST)(Spring.Expressions.SpringAST) astFactory.create(EXPR,type_AST.getText(),"Spring.Expressions.ArrayConstructorNode"), (AST)arrayConstructor_AST);
				currentAST.root = arrayConstructor_AST;
				if ( (null != arrayConstructor_AST) && (null != arrayConstructor_AST.getFirstChild()) )
					currentAST.child = arrayConstructor_AST.getFirstChild();
				else
					currentAST.child = arrayConstructor_AST;
				currentAST.advanceChildToEnd();
			}
			arrayConstructor_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = arrayConstructor_AST;
	}
	
	public void arrayRank() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST arrayRank_AST = null;
		
		try {      // for error handling
			Spring.Expressions.SpringAST tmp103_AST = null;
			tmp103_AST = (Spring.Expressions.SpringAST) astFactory.create(LT(1));
			astFactory.makeASTRoot(ref currentAST, (AST)tmp103_AST);
			match(LBRACKET);
			{
				if ((tokenSet_8_.member(LA(1))))
				{
					expression();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(ref currentAST, (AST)returnAST);
					}
					{    // ( ... )*
						for (;;)
						{
							if ((LA(1)==COMMA))
							{
								match(COMMA);
								expression();
								if (0 == inputState.guessing)
								{
									astFactory.addASTChild(ref currentAST, (AST)returnAST);
								}
							}
							else
							{
								goto _loop93_breakloop;
							}
							
						}
_loop93_breakloop:						;
					}    // ( ... )*
				}
				else if ((LA(1)==RBRACKET)) {
				}
				else
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				
			}
			match(RBRACKET);
			arrayRank_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_19_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = arrayRank_AST;
	}
	
	public void mapEntry() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST mapEntry_AST = null;
		
		try {      // for error handling
			expression();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			match(COLON);
			expression();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			if (0==inputState.guessing)
			{
				mapEntry_AST = (Spring.Expressions.SpringAST)currentAST.root;
				mapEntry_AST = (Spring.Expressions.SpringAST) astFactory.make((AST)(Spring.Expressions.SpringAST) astFactory.create(EXPR,"entry","Spring.Expressions.MapEntryNode"), (AST)mapEntry_AST);
				currentAST.root = mapEntry_AST;
				if ( (null != mapEntry_AST) && (null != mapEntry_AST.getFirstChild()) )
					currentAST.child = mapEntry_AST.getFirstChild();
				else
					currentAST.child = mapEntry_AST;
				currentAST.advanceChildToEnd();
			}
			mapEntry_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_20_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = mapEntry_AST;
	}
	
	public void namedArgument() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST namedArgument_AST = null;
		
		try {      // for error handling
			bool synPredMatched108 = false;
			if (((LA(1)==ID) && (LA(2)==ASSIGN)))
			{
				int _m108 = mark();
				synPredMatched108 = true;
				inputState.guessing++;
				try {
					{
						match(ID);
						match(ASSIGN);
					}
				}
				catch (RecognitionException)
				{
					synPredMatched108 = false;
				}
				rewind(_m108);
				inputState.guessing--;
			}
			if ( synPredMatched108 )
			{
				Spring.Expressions.NamedArgumentNode tmp107_AST = null;
				tmp107_AST = (Spring.Expressions.NamedArgumentNode) astFactory.create(LT(1), "Spring.Expressions.NamedArgumentNode");
				astFactory.makeASTRoot(ref currentAST, (AST)tmp107_AST);
				match(ID);
				match(ASSIGN);
				expression();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(ref currentAST, (AST)returnAST);
				}
				namedArgument_AST = (Spring.Expressions.SpringAST)currentAST.root;
			}
			else if ((tokenSet_8_.member(LA(1))) && (tokenSet_21_.member(LA(2)))) {
				argument();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(ref currentAST, (AST)returnAST);
				}
				namedArgument_AST = (Spring.Expressions.SpringAST)currentAST.root;
			}
			else
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_22_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = namedArgument_AST;
	}
	
	public void boolLiteral() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST boolLiteral_AST = null;
		
		try {      // for error handling
			if ((LA(1)==TRUE))
			{
				Spring.Expressions.BooleanLiteralNode tmp109_AST = null;
				tmp109_AST = (Spring.Expressions.BooleanLiteralNode) astFactory.create(LT(1), "Spring.Expressions.BooleanLiteralNode");
				astFactory.addASTChild(ref currentAST, (AST)tmp109_AST);
				match(TRUE);
				boolLiteral_AST = (Spring.Expressions.SpringAST)currentAST.root;
			}
			else if ((LA(1)==FALSE)) {
				Spring.Expressions.BooleanLiteralNode tmp110_AST = null;
				tmp110_AST = (Spring.Expressions.BooleanLiteralNode) astFactory.create(LT(1), "Spring.Expressions.BooleanLiteralNode");
				astFactory.addASTChild(ref currentAST, (AST)tmp110_AST);
				match(FALSE);
				boolLiteral_AST = (Spring.Expressions.SpringAST)currentAST.root;
			}
			else
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = boolLiteral_AST;
	}
	
	public void dateLiteral() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		Spring.Expressions.SpringAST dateLiteral_AST = null;
		
		try {      // for error handling
			Spring.Expressions.DateLiteralNode tmp111_AST = null;
			tmp111_AST = (Spring.Expressions.DateLiteralNode) astFactory.create(LT(1), "Spring.Expressions.DateLiteralNode");
			astFactory.makeASTRoot(ref currentAST, (AST)tmp111_AST);
			match(LITERAL_date);
			match(LPAREN);
			Spring.Expressions.SpringAST tmp113_AST = null;
			tmp113_AST = (Spring.Expressions.SpringAST) astFactory.create(LT(1));
			astFactory.addASTChild(ref currentAST, (AST)tmp113_AST);
			match(STRING_LITERAL);
			{
				if ((LA(1)==COMMA))
				{
					match(COMMA);
					Spring.Expressions.SpringAST tmp115_AST = null;
					tmp115_AST = (Spring.Expressions.SpringAST) astFactory.create(LT(1));
					astFactory.addASTChild(ref currentAST, (AST)tmp115_AST);
					match(STRING_LITERAL);
				}
				else if ((LA(1)==RPAREN)) {
				}
				else
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				
			}
			match(RPAREN);
			dateLiteral_AST = (Spring.Expressions.SpringAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = dateLiteral_AST;
	}
	
	public new Spring.Expressions.SpringAST getAST()
	{
		return (Spring.Expressions.SpringAST) returnAST;
	}
	
	private void initializeFactory()
	{
		if (astFactory == null)
		{
			astFactory = new ASTFactory("Spring.Expressions.SpringAST");
		}
		initializeASTFactory( astFactory );
	}
	static public void initializeASTFactory( ASTFactory factory )
	{
		factory.setMaxNodeType(71);
	}
	
	public static readonly string[] tokenNames_ = new string[] {
		@"""<0>""",
		@"""EOF""",
		@"""<2>""",
		@"""NULL_TREE_LOOKAHEAD""",
		@"""EXPR""",
		@"""OPERAND""",
		@"""false""",
		@"""true""",
		@"""and""",
		@"""or""",
		@"""in""",
		@"""is""",
		@"""between""",
		@"""like""",
		@"""matches""",
		@"""null""",
		@"""LPAREN""",
		@"""SEMI""",
		@"""RPAREN""",
		@"""ASSIGN""",
		@"""DEFAULT""",
		@"""QMARK""",
		@"""COLON""",
		@"""PLUS""",
		@"""MINUS""",
		@"""STAR""",
		@"""DIV""",
		@"""MOD""",
		@"""POWER""",
		@"""BANG""",
		@"""DOT""",
		@"""POUND""",
		@"""ID""",
		@"""DOLLAR""",
		@"""COMMA""",
		@"""AT""",
		@"""LBRACKET""",
		@"""RBRACKET""",
		@"""PROJECT""",
		@"""RCURLY""",
		@"""SELECT""",
		@"""SELECT_FIRST""",
		@"""SELECT_LAST""",
		@"""TYPE""",
		@"""QUOTE""",
		@"""STRING_LITERAL""",
		@"""LAMBDA""",
		@"""PIPE""",
		@"""new""",
		@"""LCURLY""",
		@"""INTEGER_LITERAL""",
		@"""HEXADECIMAL_INTEGER_LITERAL""",
		@"""REAL_LITERAL""",
		@"""date""",
		@"""EQUAL""",
		@"""NOT_EQUAL""",
		@"""LESS_THAN""",
		@"""LESS_THAN_OR_EQUAL""",
		@"""GREATER_THAN""",
		@"""GREATER_THAN_OR_EQUAL""",
		@"""WS""",
		@"""BACKTICK""",
		@"""BACKSLASH""",
		@"""DOT_ESCAPED""",
		@"""APOS""",
		@"""NUMERIC_LITERAL""",
		@"""DECIMAL_DIGIT""",
		@"""INTEGER_TYPE_SUFFIX""",
		@"""HEX_DIGIT""",
		@"""EXPONENT_PART""",
		@"""SIGN""",
		@"""REAL_TYPE_SUFFIX"""
	};
	
	private static long[] mk_tokenSet_0_()
	{
		long[] data = { 2L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_0_ = new BitSet(mk_tokenSet_0_());
	private static long[] mk_tokenSet_1_()
	{
		long[] data = { 704379224066L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_1_ = new BitSet(mk_tokenSet_1_());
	private static long[] mk_tokenSet_2_()
	{
		long[] data = { 1134915856556326658L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_2_ = new BitSet(mk_tokenSet_2_());
	private static long[] mk_tokenSet_3_()
	{
		long[] data = { 704382894082L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_3_ = new BitSet(mk_tokenSet_3_());
	private static long[] mk_tokenSet_4_()
	{
		long[] data = { 704382894594L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_4_ = new BitSet(mk_tokenSet_4_());
	private static long[] mk_tokenSet_5_()
	{
		long[] data = { 1134907106097396736L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_5_ = new BitSet(mk_tokenSet_5_());
	private static long[] mk_tokenSet_6_()
	{
		long[] data = { 704382894850L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_6_ = new BitSet(mk_tokenSet_6_());
	private static long[] mk_tokenSet_7_()
	{
		long[] data = { 1134907810480291586L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_7_ = new BitSet(mk_tokenSet_7_());
	private static long[] mk_tokenSet_8_()
	{
		long[] data = { 17855362875097280L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_8_ = new BitSet(mk_tokenSet_8_());
	private static long[] mk_tokenSet_9_()
	{
		long[] data = { 1134907810505457410L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_9_ = new BitSet(mk_tokenSet_9_());
	private static long[] mk_tokenSet_10_()
	{
		long[] data = { 1134907810740338434L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_10_ = new BitSet(mk_tokenSet_10_());
	private static long[] mk_tokenSet_11_()
	{
		long[] data = { 17855362313060544L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_11_ = new BitSet(mk_tokenSet_11_());
	private static long[] mk_tokenSet_12_()
	{
		long[] data = { 1134907811008773890L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_12_ = new BitSet(mk_tokenSet_12_());
	private static long[] mk_tokenSet_13_()
	{
		long[] data = { 8045547552768L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_13_ = new BitSet(mk_tokenSet_13_());
	private static long[] mk_tokenSet_14_()
	{
		long[] data = { 154619084800L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_14_ = new BitSet(mk_tokenSet_14_());
	private static long[] mk_tokenSet_15_()
	{
		long[] data = { 4456448L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_15_ = new BitSet(mk_tokenSet_15_());
	private static long[] mk_tokenSet_16_()
	{
		long[] data = { -17592190500880L, 255L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_16_ = new BitSet(mk_tokenSet_16_());
	private static long[] mk_tokenSet_17_()
	{
		long[] data = { 206158495744L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_17_ = new BitSet(mk_tokenSet_17_());
	private static long[] mk_tokenSet_18_()
	{
		long[] data = { 140737488355328L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_18_ = new BitSet(mk_tokenSet_18_());
	private static long[] mk_tokenSet_19_()
	{
		long[] data = { 1135478806509747970L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_19_ = new BitSet(mk_tokenSet_19_());
	private static long[] mk_tokenSet_20_()
	{
		long[] data = { 566935683072L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_20_ = new BitSet(mk_tokenSet_20_());
	private static long[] mk_tokenSet_21_()
	{
		long[] data = { 1152903225221709760L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_21_ = new BitSet(mk_tokenSet_21_());
	private static long[] mk_tokenSet_22_()
	{
		long[] data = { 17180131328L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_22_ = new BitSet(mk_tokenSet_22_());
	
}
}
