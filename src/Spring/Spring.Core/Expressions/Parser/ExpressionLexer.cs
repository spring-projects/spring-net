// $ANTLR 2.7.6 (2005-12-22): "Expression.g" -> "ExpressionLexer.cs"$

namespace Spring.Expressions
{
	// Generate header specific to lexer CSharp file
	using System;
	using Stream                          = System.IO.Stream;
	using TextReader                      = System.IO.TextReader;
	using Hashtable                       = System.Collections.Hashtable;
	using Comparer                        = System.Collections.Comparer;
	
	using TokenStreamException            = antlr.TokenStreamException;
	using TokenStreamIOException          = antlr.TokenStreamIOException;
	using TokenStreamRecognitionException = antlr.TokenStreamRecognitionException;
	using CharStreamException             = antlr.CharStreamException;
	using CharStreamIOException           = antlr.CharStreamIOException;
	using ANTLRException                  = antlr.ANTLRException;
	using CharScanner                     = antlr.CharScanner;
	using InputBuffer                     = antlr.InputBuffer;
	using ByteBuffer                      = antlr.ByteBuffer;
	using CharBuffer                      = antlr.CharBuffer;
	using Token                           = antlr.Token;
	using IToken                          = antlr.IToken;
	using CommonToken                     = antlr.CommonToken;
	using SemanticException               = antlr.SemanticException;
	using RecognitionException            = antlr.RecognitionException;
	using NoViableAltForCharException     = antlr.NoViableAltForCharException;
	using MismatchedCharException         = antlr.MismatchedCharException;
	using TokenStream                     = antlr.TokenStream;
	using LexerSharedInputState           = antlr.LexerSharedInputState;
	using BitSet                          = antlr.collections.impl.BitSet;
	
	internal 	class ExpressionLexer : antlr.CharScanner	, TokenStream
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
		public const int STRING_LITERAL = 36;
		public const int LBRACKET = 37;
		public const int RBRACKET = 38;
		public const int PROJECT = 39;
		public const int RCURLY = 40;
		public const int SELECT = 41;
		public const int SELECT_FIRST = 42;
		public const int SELECT_LAST = 43;
		public const int TYPE = 44;
		public const int LAMBDA = 45;
		public const int PIPE = 46;
		public const int LITERAL_new = 47;
		public const int LCURLY = 48;
		public const int INTEGER_LITERAL = 49;
		public const int HEXADECIMAL_INTEGER_LITERAL = 50;
		public const int REAL_LITERAL = 51;
		public const int LITERAL_date = 52;
		public const int EQUAL = 53;
		public const int NOT_EQUAL = 54;
		public const int LESS_THAN = 55;
		public const int LESS_THAN_OR_EQUAL = 56;
		public const int GREATER_THAN = 57;
		public const int GREATER_THAN_OR_EQUAL = 58;
		public const int WS = 59;
		public const int BACKTICK = 60;
		public const int BACKSLASH = 61;
		public const int DOT_ESCAPED = 62;
		public const int QUOTE = 63;
		public const int APOS = 64;
		public const int NUMERIC_LITERAL = 65;
		public const int DECIMAL_DIGIT = 66;
		public const int INTEGER_TYPE_SUFFIX = 67;
		public const int HEX_DIGIT = 68;
		public const int EXPONENT_PART = 69;
		public const int SIGN = 70;
		public const int REAL_TYPE_SUFFIX = 71;
		
		
    // CLOVER:OFF
		public ExpressionLexer(Stream ins) : this(new ByteBuffer(ins))
		{
		}
		
		public ExpressionLexer(TextReader r) : this(new CharBuffer(r))
		{
		}
		
		public ExpressionLexer(InputBuffer ib)		 : this(new LexerSharedInputState(ib))
		{
		}
		
		public ExpressionLexer(LexerSharedInputState state) : base(state)
		{
			initialize();
		}
		private void initialize()
		{
			caseSensitiveLiterals = true;
			setCaseSensitive(true);
			literals = new Hashtable(100, (float) 0.4, null, Comparer.Default);
			literals.Add("true", 7);
			literals.Add("and", 8);
			literals.Add("matches", 14);
			literals.Add("in", 10);
			literals.Add("null", 15);
			literals.Add("between", 12);
			literals.Add("or", 9);
			literals.Add("is", 11);
			literals.Add("like", 13);
			literals.Add("new", 47);
			literals.Add("date", 52);
			literals.Add("false", 6);
		}
		
		override public IToken nextToken()			//throws TokenStreamException
		{
			IToken theRetToken = null;
tryAgain:
			for (;;)
			{
				IToken _token = null;
				int _ttype = Token.INVALID_TYPE;
				resetText();
				try     // for char stream error handling
				{
					try     // for lexical error handling
					{
						switch ( cached_LA1 )
						{
						case '\t':  case '\n':  case '\r':  case ' ':
						{
							mWS(true);
							theRetToken = returnToken_;
							break;
						}
						case '@':
						{
							mAT(true);
							theRetToken = returnToken_;
							break;
						}
						case '`':
						{
							mBACKTICK(true);
							theRetToken = returnToken_;
							break;
						}
						case '|':
						{
							mPIPE(true);
							theRetToken = returnToken_;
							break;
						}
						case '#':
						{
							mPOUND(true);
							theRetToken = returnToken_;
							break;
						}
						case '(':
						{
							mLPAREN(true);
							theRetToken = returnToken_;
							break;
						}
						case ')':
						{
							mRPAREN(true);
							theRetToken = returnToken_;
							break;
						}
						case '[':
						{
							mLBRACKET(true);
							theRetToken = returnToken_;
							break;
						}
						case ']':
						{
							mRBRACKET(true);
							theRetToken = returnToken_;
							break;
						}
						case '}':
						{
							mRCURLY(true);
							theRetToken = returnToken_;
							break;
						}
						case ',':
						{
							mCOMMA(true);
							theRetToken = returnToken_;
							break;
						}
						case ';':
						{
							mSEMI(true);
							theRetToken = returnToken_;
							break;
						}
						case ':':
						{
							mCOLON(true);
							theRetToken = returnToken_;
							break;
						}
						case '+':
						{
							mPLUS(true);
							theRetToken = returnToken_;
							break;
						}
						case '-':
						{
							mMINUS(true);
							theRetToken = returnToken_;
							break;
						}
						case '/':
						{
							mDIV(true);
							theRetToken = returnToken_;
							break;
						}
						case '*':
						{
							mSTAR(true);
							theRetToken = returnToken_;
							break;
						}
						case '%':
						{
							mMOD(true);
							theRetToken = returnToken_;
							break;
						}
						default:
							if ((cached_LA1=='?') && (cached_LA2=='?'))
							{
								mDEFAULT(true);
								theRetToken = returnToken_;
							}
							else if ((cached_LA1=='=') && (cached_LA2=='=')) {
								mEQUAL(true);
								theRetToken = returnToken_;
							}
							else if ((cached_LA1=='!') && (cached_LA2=='=')) {
								mNOT_EQUAL(true);
								theRetToken = returnToken_;
							}
							else if ((cached_LA1=='<') && (cached_LA2=='=')) {
								mLESS_THAN_OR_EQUAL(true);
								theRetToken = returnToken_;
							}
							else if ((cached_LA1=='>') && (cached_LA2=='=')) {
								mGREATER_THAN_OR_EQUAL(true);
								theRetToken = returnToken_;
							}
							else if ((cached_LA1=='!') && (cached_LA2=='{')) {
								mPROJECT(true);
								theRetToken = returnToken_;
							}
							else if ((cached_LA1=='?') && (cached_LA2=='{')) {
								mSELECT(true);
								theRetToken = returnToken_;
							}
							else if ((cached_LA1=='^') && (cached_LA2=='{')) {
								mSELECT_FIRST(true);
								theRetToken = returnToken_;
							}
							else if ((cached_LA1=='$') && (cached_LA2=='{')) {
								mSELECT_LAST(true);
								theRetToken = returnToken_;
							}
							else if ((cached_LA1=='T') && (cached_LA2=='(')) {
								mTYPE(true);
								theRetToken = returnToken_;
							}
							else if ((cached_LA1=='{') && (cached_LA2=='|')) {
								mLAMBDA(true);
								theRetToken = returnToken_;
							}
							else if ((cached_LA1=='\\') && (cached_LA2=='.')) {
								mDOT_ESCAPED(true);
								theRetToken = returnToken_;
							}
							else if ((cached_LA1=='\'') && ((cached_LA2 >= '\u0000' && cached_LA2 <= '\ufffe'))) {
								mSTRING_LITERAL(true);
								theRetToken = returnToken_;
							}
							else if ((cached_LA1=='0') && (cached_LA2=='x')) {
								mHEXADECIMAL_INTEGER_LITERAL(true);
								theRetToken = returnToken_;
							}
							else if ((cached_LA1=='\\') && (true)) {
								mBACKSLASH(true);
								theRetToken = returnToken_;
							}
							else if ((cached_LA1=='!') && (true)) {
								mBANG(true);
								theRetToken = returnToken_;
							}
							else if ((cached_LA1=='?') && (true)) {
								mQMARK(true);
								theRetToken = returnToken_;
							}
							else if ((cached_LA1=='$') && (true)) {
								mDOLLAR(true);
								theRetToken = returnToken_;
							}
							else if ((cached_LA1=='{') && (true)) {
								mLCURLY(true);
								theRetToken = returnToken_;
							}
							else if ((cached_LA1=='=') && (true)) {
								mASSIGN(true);
								theRetToken = returnToken_;
							}
							else if ((cached_LA1=='^') && (true)) {
								mPOWER(true);
								theRetToken = returnToken_;
							}
							else if ((cached_LA1=='<') && (true)) {
								mLESS_THAN(true);
								theRetToken = returnToken_;
							}
							else if ((cached_LA1=='>') && (true)) {
								mGREATER_THAN(true);
								theRetToken = returnToken_;
							}
							else if ((cached_LA1=='\'') && (true)) {
								mQUOTE(true);
								theRetToken = returnToken_;
							}
							else if ((tokenSet_0_.member(cached_LA1)) && (true)) {
								mID(true);
								theRetToken = returnToken_;
							}
							else if ((tokenSet_1_.member(cached_LA1)) && (true)) {
								mNUMERIC_LITERAL(true);
								theRetToken = returnToken_;
							}
						else
						{
							if (cached_LA1==EOF_CHAR) { uponEOF(); returnToken_ = makeToken(Token.EOF_TYPE); }
				else {throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());}
						}
						break; }
						if ( null==returnToken_ ) goto tryAgain; // found SKIP token
						_ttype = returnToken_.Type;
						returnToken_.Type = _ttype;
						return returnToken_;
					}
					catch (RecognitionException e) {
							throw new TokenStreamRecognitionException(e);
					}
				}
				catch (CharStreamException cse) {
					if ( cse is CharStreamIOException ) {
						throw new TokenStreamIOException(((CharStreamIOException)cse).io);
					}
					else {
						throw new TokenStreamException(cse.Message);
					}
				}
			}
		}
		
	public void mWS(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = WS;
		
		{
			switch ( cached_LA1 )
			{
			case ' ':
			{
				match(' ');
				break;
			}
			case '\t':
			{
				match('\t');
				break;
			}
			case '\n':
			{
				match('\n');
				break;
			}
			case '\r':
			{
				match('\r');
				break;
			}
			default:
			{
				throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
			}
			 }
		}
		if (0==inputState.guessing)
		{
			_ttype = Token.SKIP;
		}
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mAT(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = AT;
		
		match('@');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mBACKTICK(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = BACKTICK;
		
		match('`');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mBACKSLASH(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = BACKSLASH;
		
		match('\\');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mPIPE(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = PIPE;
		
		match('|');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mBANG(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = BANG;
		
		match('!');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mQMARK(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = QMARK;
		
		match('?');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mDOLLAR(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = DOLLAR;
		
		match('$');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mPOUND(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = POUND;
		
		match('#');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mLPAREN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = LPAREN;
		
		match('(');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mRPAREN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = RPAREN;
		
		match(')');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mLBRACKET(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = LBRACKET;
		
		match('[');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mRBRACKET(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = RBRACKET;
		
		match(']');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mLCURLY(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = LCURLY;
		
		match('{');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mRCURLY(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = RCURLY;
		
		match('}');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mCOMMA(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = COMMA;
		
		match(',');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mSEMI(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = SEMI;
		
		match(';');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mCOLON(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = COLON;
		
		match(':');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mASSIGN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = ASSIGN;
		
		match('=');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mDEFAULT(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = DEFAULT;
		
		match("??");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mPLUS(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = PLUS;
		
		match('+');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mMINUS(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = MINUS;
		
		match('-');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mDIV(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = DIV;
		
		match('/');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mSTAR(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = STAR;
		
		match('*');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mMOD(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = MOD;
		
		match('%');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mPOWER(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = POWER;
		
		match('^');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mEQUAL(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = EQUAL;
		
		match("==");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mNOT_EQUAL(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = NOT_EQUAL;
		
		match("!=");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mLESS_THAN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = LESS_THAN;
		
		match('<');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mLESS_THAN_OR_EQUAL(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = LESS_THAN_OR_EQUAL;
		
		match("<=");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mGREATER_THAN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = GREATER_THAN;
		
		match('>');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mGREATER_THAN_OR_EQUAL(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = GREATER_THAN_OR_EQUAL;
		
		match(">=");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mPROJECT(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = PROJECT;
		
		match("!{");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mSELECT(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = SELECT;
		
		match("?{");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mSELECT_FIRST(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = SELECT_FIRST;
		
		match("^{");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mSELECT_LAST(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = SELECT_LAST;
		
		match("${");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mTYPE(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = TYPE;
		
		match("T(");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mLAMBDA(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = LAMBDA;
		
		match("{|");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mDOT_ESCAPED(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = DOT_ESCAPED;
		
		match("\\.");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mQUOTE(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = QUOTE;
		
		match('\'');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mSTRING_LITERAL(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = STRING_LITERAL;
		
		int _saveIndex = 0;
		_saveIndex = text.Length;
		mQUOTE(false);
		text.Length = _saveIndex;
		{    // ( ... )*
			for (;;)
			{
				if ((cached_LA1=='\'') && (cached_LA2=='\''))
				{
					mAPOS(false);
				}
				else if ((tokenSet_2_.member(cached_LA1))) {
					matchNot('\'');
				}
				else
				{
					goto _loop160_breakloop;
				}
				
			}
_loop160_breakloop:			;
		}    // ( ... )*
		_saveIndex = text.Length;
		mQUOTE(false);
		text.Length = _saveIndex;
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	protected void mAPOS(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = APOS;
		
		int _saveIndex = 0;
		_saveIndex = text.Length;
		mQUOTE(false);
		text.Length = _saveIndex;
		mQUOTE(false);
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mID(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = ID;
		
		{
			switch ( cached_LA1 )
			{
			case 'a':  case 'b':  case 'c':  case 'd':
			case 'e':  case 'f':  case 'g':  case 'h':
			case 'i':  case 'j':  case 'k':  case 'l':
			case 'm':  case 'n':  case 'o':  case 'p':
			case 'q':  case 'r':  case 's':  case 't':
			case 'u':  case 'v':  case 'w':  case 'x':
			case 'y':  case 'z':
			{
				matchRange('a','z');
				break;
			}
			case 'A':  case 'B':  case 'C':  case 'D':
			case 'E':  case 'F':  case 'G':  case 'H':
			case 'I':  case 'J':  case 'K':  case 'L':
			case 'M':  case 'N':  case 'O':  case 'P':
			case 'Q':  case 'R':  case 'S':  case 'T':
			case 'U':  case 'V':  case 'W':  case 'X':
			case 'Y':  case 'Z':
			{
				matchRange('A','Z');
				break;
			}
			case '_':
			{
				match('_');
				break;
			}
			default:
			{
				throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
			}
			 }
		}
		{    // ( ... )*
			for (;;)
			{
				switch ( cached_LA1 )
				{
				case 'a':  case 'b':  case 'c':  case 'd':
				case 'e':  case 'f':  case 'g':  case 'h':
				case 'i':  case 'j':  case 'k':  case 'l':
				case 'm':  case 'n':  case 'o':  case 'p':
				case 'q':  case 'r':  case 's':  case 't':
				case 'u':  case 'v':  case 'w':  case 'x':
				case 'y':  case 'z':
				{
					matchRange('a','z');
					break;
				}
				case 'A':  case 'B':  case 'C':  case 'D':
				case 'E':  case 'F':  case 'G':  case 'H':
				case 'I':  case 'J':  case 'K':  case 'L':
				case 'M':  case 'N':  case 'O':  case 'P':
				case 'Q':  case 'R':  case 'S':  case 'T':
				case 'U':  case 'V':  case 'W':  case 'X':
				case 'Y':  case 'Z':
				{
					matchRange('A','Z');
					break;
				}
				case '_':
				{
					match('_');
					break;
				}
				case '0':  case '1':  case '2':  case '3':
				case '4':  case '5':  case '6':  case '7':
				case '8':  case '9':
				{
					matchRange('0','9');
					break;
				}
				case '\\':
				{
					mDOT_ESCAPED(false);
					break;
				}
				default:
				{
					goto _loop165_breakloop;
				}
				 }
			}
_loop165_breakloop:			;
		}    // ( ... )*
		_ttype = testLiteralsTable(_ttype);
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mNUMERIC_LITERAL(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = NUMERIC_LITERAL;
		
		bool synPredMatched168 = false;
		if (((cached_LA1=='.') && ((cached_LA2 >= '0' && cached_LA2 <= '9'))))
		{
			int _m168 = mark();
			synPredMatched168 = true;
			inputState.guessing++;
			try {
				{
					match('.');
					mDECIMAL_DIGIT(false);
				}
			}
			catch (RecognitionException)
			{
				synPredMatched168 = false;
			}
			rewind(_m168);
			inputState.guessing--;
		}
		if ( synPredMatched168 )
		{
			match('.');
			{ // ( ... )+
				int _cnt170=0;
				for (;;)
				{
					if (((cached_LA1 >= '0' && cached_LA1 <= '9')))
					{
						mDECIMAL_DIGIT(false);
					}
					else
					{
						if (_cnt170 >= 1) { goto _loop170_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
					}
					
					_cnt170++;
				}
_loop170_breakloop:				;
			}    // ( ... )+
			{
				if ((cached_LA1=='E'||cached_LA1=='e'))
				{
					mEXPONENT_PART(false);
				}
				else {
				}
				
			}
			{
				if ((tokenSet_3_.member(cached_LA1)))
				{
					mREAL_TYPE_SUFFIX(false);
				}
				else {
				}
				
			}
			if (0==inputState.guessing)
			{
				_ttype = REAL_LITERAL;
			}
		}
		else {
			bool synPredMatched176 = false;
			if ((((cached_LA1 >= '0' && cached_LA1 <= '9')) && (tokenSet_1_.member(cached_LA2))))
			{
				int _m176 = mark();
				synPredMatched176 = true;
				inputState.guessing++;
				try {
					{
						{ // ( ... )+
							int _cnt175=0;
							for (;;)
							{
								if (((cached_LA1 >= '0' && cached_LA1 <= '9')))
								{
									mDECIMAL_DIGIT(false);
								}
								else
								{
									if (_cnt175 >= 1) { goto _loop175_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
								}
								
								_cnt175++;
							}
_loop175_breakloop:							;
						}    // ( ... )+
						match('.');
						mDECIMAL_DIGIT(false);
					}
				}
				catch (RecognitionException)
				{
					synPredMatched176 = false;
				}
				rewind(_m176);
				inputState.guessing--;
			}
			if ( synPredMatched176 )
			{
				{ // ( ... )+
					int _cnt178=0;
					for (;;)
					{
						if (((cached_LA1 >= '0' && cached_LA1 <= '9')))
						{
							mDECIMAL_DIGIT(false);
						}
						else
						{
							if (_cnt178 >= 1) { goto _loop178_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
						}
						
						_cnt178++;
					}
_loop178_breakloop:					;
				}    // ( ... )+
				match('.');
				{ // ( ... )+
					int _cnt180=0;
					for (;;)
					{
						if (((cached_LA1 >= '0' && cached_LA1 <= '9')))
						{
							mDECIMAL_DIGIT(false);
						}
						else
						{
							if (_cnt180 >= 1) { goto _loop180_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
						}
						
						_cnt180++;
					}
_loop180_breakloop:					;
				}    // ( ... )+
				{
					if ((cached_LA1=='E'||cached_LA1=='e'))
					{
						mEXPONENT_PART(false);
					}
					else {
					}
					
				}
				{
					if ((tokenSet_3_.member(cached_LA1)))
					{
						mREAL_TYPE_SUFFIX(false);
					}
					else {
					}
					
				}
				if (0==inputState.guessing)
				{
					_ttype = REAL_LITERAL;
				}
			}
			else {
				bool synPredMatched187 = false;
				if ((((cached_LA1 >= '0' && cached_LA1 <= '9')) && (tokenSet_4_.member(cached_LA2))))
				{
					int _m187 = mark();
					synPredMatched187 = true;
					inputState.guessing++;
					try {
						{
							{ // ( ... )+
								int _cnt185=0;
								for (;;)
								{
									if (((cached_LA1 >= '0' && cached_LA1 <= '9')))
									{
										mDECIMAL_DIGIT(false);
									}
									else
									{
										if (_cnt185 >= 1) { goto _loop185_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
									}
									
									_cnt185++;
								}
_loop185_breakloop:								;
							}    // ( ... )+
							{
								mEXPONENT_PART(false);
							}
						}
					}
					catch (RecognitionException)
					{
						synPredMatched187 = false;
					}
					rewind(_m187);
					inputState.guessing--;
				}
				if ( synPredMatched187 )
				{
					{ // ( ... )+
						int _cnt189=0;
						for (;;)
						{
							if (((cached_LA1 >= '0' && cached_LA1 <= '9')))
							{
								mDECIMAL_DIGIT(false);
							}
							else
							{
								if (_cnt189 >= 1) { goto _loop189_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
							}
							
							_cnt189++;
						}
_loop189_breakloop:						;
					}    // ( ... )+
					{
						mEXPONENT_PART(false);
					}
					{
						if ((tokenSet_3_.member(cached_LA1)))
						{
							mREAL_TYPE_SUFFIX(false);
						}
						else {
						}
						
					}
					if (0==inputState.guessing)
					{
						_ttype = REAL_LITERAL;
					}
				}
				else {
					bool synPredMatched196 = false;
					if ((((cached_LA1 >= '0' && cached_LA1 <= '9')) && (tokenSet_5_.member(cached_LA2))))
					{
						int _m196 = mark();
						synPredMatched196 = true;
						inputState.guessing++;
						try {
							{
								{ // ( ... )+
									int _cnt194=0;
									for (;;)
									{
										if (((cached_LA1 >= '0' && cached_LA1 <= '9')))
										{
											mDECIMAL_DIGIT(false);
										}
										else
										{
											if (_cnt194 >= 1) { goto _loop194_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
										}
										
										_cnt194++;
									}
_loop194_breakloop:									;
								}    // ( ... )+
								{
									mREAL_TYPE_SUFFIX(false);
								}
							}
						}
						catch (RecognitionException)
						{
							synPredMatched196 = false;
						}
						rewind(_m196);
						inputState.guessing--;
					}
					if ( synPredMatched196 )
					{
						{ // ( ... )+
							int _cnt198=0;
							for (;;)
							{
								if (((cached_LA1 >= '0' && cached_LA1 <= '9')))
								{
									mDECIMAL_DIGIT(false);
								}
								else
								{
									if (_cnt198 >= 1) { goto _loop198_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
								}
								
								_cnt198++;
							}
_loop198_breakloop:							;
						}    // ( ... )+
						{
							mREAL_TYPE_SUFFIX(false);
						}
						if (0==inputState.guessing)
						{
							_ttype = REAL_LITERAL;
						}
					}
					else if (((cached_LA1 >= '0' && cached_LA1 <= '9')) && (true)) {
						{ // ( ... )+
							int _cnt201=0;
							for (;;)
							{
								if (((cached_LA1 >= '0' && cached_LA1 <= '9')))
								{
									mDECIMAL_DIGIT(false);
								}
								else
								{
									if (_cnt201 >= 1) { goto _loop201_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
								}
								
								_cnt201++;
							}
_loop201_breakloop:							;
						}    // ( ... )+
						{
							if ((tokenSet_6_.member(cached_LA1)))
							{
								mINTEGER_TYPE_SUFFIX(false);
							}
							else {
							}
							
						}
						if (0==inputState.guessing)
						{
							_ttype = INTEGER_LITERAL;
						}
					}
					else if ((cached_LA1=='.') && (true)) {
						match('.');
						if (0==inputState.guessing)
						{
							_ttype = DOT;
						}
					}
					else
					{
						throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
					}
					}}}
					if (_createToken && (null == _token) && (_ttype != Token.SKIP))
					{
						_token = makeToken(_ttype);
						_token.setText(text.ToString(_begin, text.Length-_begin));
					}
					returnToken_ = _token;
				}
				
	protected void mDECIMAL_DIGIT(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = DECIMAL_DIGIT;
		
		matchRange('0','9');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	protected void mEXPONENT_PART(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = EXPONENT_PART;
		
		switch ( cached_LA1 )
		{
		case 'e':
		{
			match("e");
			{    // ( ... )*
				for (;;)
				{
					if ((cached_LA1=='+'||cached_LA1=='-'))
					{
						mSIGN(false);
					}
					else
					{
						goto _loop213_breakloop;
					}
					
				}
_loop213_breakloop:				;
			}    // ( ... )*
			{ // ( ... )+
				int _cnt215=0;
				for (;;)
				{
					if (((cached_LA1 >= '0' && cached_LA1 <= '9')))
					{
						mDECIMAL_DIGIT(false);
					}
					else
					{
						if (_cnt215 >= 1) { goto _loop215_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
					}
					
					_cnt215++;
				}
_loop215_breakloop:				;
			}    // ( ... )+
			break;
		}
		case 'E':
		{
			match("E");
			{    // ( ... )*
				for (;;)
				{
					if ((cached_LA1=='+'||cached_LA1=='-'))
					{
						mSIGN(false);
					}
					else
					{
						goto _loop217_breakloop;
					}
					
				}
_loop217_breakloop:				;
			}    // ( ... )*
			{ // ( ... )+
				int _cnt219=0;
				for (;;)
				{
					if (((cached_LA1 >= '0' && cached_LA1 <= '9')))
					{
						mDECIMAL_DIGIT(false);
					}
					else
					{
						if (_cnt219 >= 1) { goto _loop219_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
					}
					
					_cnt219++;
				}
_loop219_breakloop:				;
			}    // ( ... )+
			break;
		}
		default:
		{
			throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
		}
		 }
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	protected void mREAL_TYPE_SUFFIX(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = REAL_TYPE_SUFFIX;
		
		switch ( cached_LA1 )
		{
		case 'F':
		{
			match('F');
			break;
		}
		case 'f':
		{
			match('f');
			break;
		}
		case 'D':
		{
			match('D');
			break;
		}
		case 'd':
		{
			match('d');
			break;
		}
		case 'M':
		{
			match('M');
			break;
		}
		case 'm':
		{
			match('m');
			break;
		}
		default:
		{
			throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
		}
		 }
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	protected void mINTEGER_TYPE_SUFFIX(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = INTEGER_TYPE_SUFFIX;
		
		{
			if ((cached_LA1=='U') && (cached_LA2=='L'))
			{
				match("UL");
			}
			else if ((cached_LA1=='L') && (cached_LA2=='U')) {
				match("LU");
			}
			else if ((cached_LA1=='u') && (cached_LA2=='l')) {
				match("ul");
			}
			else if ((cached_LA1=='l') && (cached_LA2=='u')) {
				match("lu");
			}
			else if ((cached_LA1=='U') && (cached_LA2=='L')) {
				match("UL");
			}
			else if ((cached_LA1=='L') && (cached_LA2=='U')) {
				match("LU");
			}
			else if ((cached_LA1=='u') && (cached_LA2=='L')) {
				match("uL");
			}
			else if ((cached_LA1=='l') && (cached_LA2=='U')) {
				match("lU");
			}
			else if ((cached_LA1=='U') && (true)) {
				match("U");
			}
			else if ((cached_LA1=='L') && (true)) {
				match("L");
			}
			else if ((cached_LA1=='u') && (true)) {
				match("u");
			}
			else if ((cached_LA1=='l') && (true)) {
				match("l");
			}
			else
			{
				throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
			}
			
		}
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mHEXADECIMAL_INTEGER_LITERAL(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = HEXADECIMAL_INTEGER_LITERAL;
		
		match("0x");
		{ // ( ... )+
			int _cnt205=0;
			for (;;)
			{
				if ((tokenSet_7_.member(cached_LA1)))
				{
					mHEX_DIGIT(false);
				}
				else
				{
					if (_cnt205 >= 1) { goto _loop205_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
				}
				
				_cnt205++;
			}
_loop205_breakloop:			;
		}    // ( ... )+
		{
			if ((tokenSet_6_.member(cached_LA1)))
			{
				mINTEGER_TYPE_SUFFIX(false);
			}
			else {
			}
			
		}
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	protected void mHEX_DIGIT(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = HEX_DIGIT;
		
		switch ( cached_LA1 )
		{
		case '0':
		{
			match('0');
			break;
		}
		case '1':
		{
			match('1');
			break;
		}
		case '2':
		{
			match('2');
			break;
		}
		case '3':
		{
			match('3');
			break;
		}
		case '4':
		{
			match('4');
			break;
		}
		case '5':
		{
			match('5');
			break;
		}
		case '6':
		{
			match('6');
			break;
		}
		case '7':
		{
			match('7');
			break;
		}
		case '8':
		{
			match('8');
			break;
		}
		case '9':
		{
			match('9');
			break;
		}
		case 'A':
		{
			match('A');
			break;
		}
		case 'B':
		{
			match('B');
			break;
		}
		case 'C':
		{
			match('C');
			break;
		}
		case 'D':
		{
			match('D');
			break;
		}
		case 'E':
		{
			match('E');
			break;
		}
		case 'F':
		{
			match('F');
			break;
		}
		case 'a':
		{
			match('a');
			break;
		}
		case 'b':
		{
			match('b');
			break;
		}
		case 'c':
		{
			match('c');
			break;
		}
		case 'd':
		{
			match('d');
			break;
		}
		case 'e':
		{
			match('e');
			break;
		}
		case 'f':
		{
			match('f');
			break;
		}
		default:
		{
			throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
		}
		 }
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	protected void mSIGN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = SIGN;
		
		switch ( cached_LA1 )
		{
		case '+':
		{
			match('+');
			break;
		}
		case '-':
		{
			match('-');
			break;
		}
		default:
		{
			throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
		}
		 }
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	
	private static long[] mk_tokenSet_0_()
	{
		long[] data = new long[1025];
		data[0]=0L;
		data[1]=576460745995190270L;
		for (int i = 2; i<=1024; i++) { data[i]=0L; }
		return data;
	}
	public static readonly BitSet tokenSet_0_ = new BitSet(mk_tokenSet_0_());
	private static long[] mk_tokenSet_1_()
	{
		long[] data = new long[1025];
		data[0]=288019269919178752L;
		for (int i = 1; i<=1024; i++) { data[i]=0L; }
		return data;
	}
	public static readonly BitSet tokenSet_1_ = new BitSet(mk_tokenSet_1_());
	private static long[] mk_tokenSet_2_()
	{
		long[] data = new long[2048];
		data[0]=-549755813889L;
		for (int i = 1; i<=1022; i++) { data[i]=-1L; }
		data[1023]=9223372036854775807L;
		for (int i = 1024; i<=2047; i++) { data[i]=0L; }
		return data;
	}
	public static readonly BitSet tokenSet_2_ = new BitSet(mk_tokenSet_2_());
	private static long[] mk_tokenSet_3_()
	{
		long[] data = new long[1025];
		data[0]=0L;
		data[1]=35527969480784L;
		for (int i = 2; i<=1024; i++) { data[i]=0L; }
		return data;
	}
	public static readonly BitSet tokenSet_3_ = new BitSet(mk_tokenSet_3_());
	private static long[] mk_tokenSet_4_()
	{
		long[] data = new long[1025];
		data[0]=287948901175001088L;
		data[1]=137438953504L;
		for (int i = 2; i<=1024; i++) { data[i]=0L; }
		return data;
	}
	public static readonly BitSet tokenSet_4_ = new BitSet(mk_tokenSet_4_());
	private static long[] mk_tokenSet_5_()
	{
		long[] data = new long[1025];
		data[0]=287948901175001088L;
		data[1]=35527969480784L;
		for (int i = 2; i<=1024; i++) { data[i]=0L; }
		return data;
	}
	public static readonly BitSet tokenSet_5_ = new BitSet(mk_tokenSet_5_());
	private static long[] mk_tokenSet_6_()
	{
		long[] data = new long[1025];
		data[0]=0L;
		data[1]=9024791442886656L;
		for (int i = 2; i<=1024; i++) { data[i]=0L; }
		return data;
	}
	public static readonly BitSet tokenSet_6_ = new BitSet(mk_tokenSet_6_());
	private static long[] mk_tokenSet_7_()
	{
		long[] data = new long[1025];
		data[0]=287948901175001088L;
		data[1]=541165879422L;
		for (int i = 2; i<=1024; i++) { data[i]=0L; }
		return data;
	}
	public static readonly BitSet tokenSet_7_ = new BitSet(mk_tokenSet_7_());
	
}
}
