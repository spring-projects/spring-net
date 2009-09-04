options
{
	language = "CSharp";
	namespace = "Spring.Expressions.Parser";
}

class ExpressionParser extends Parser;

options {
	codeGenMakeSwitchThreshold = 3;
	codeGenBitsetTestThreshold = 4;
	classHeaderPrefix = "internal"; 
	buildAST=true;
	ASTLabelType = "Spring.Expressions.SpringAST";
	k = 2;
}

tokens {
	EXPR;
	OPERAND;
	FALSE = "false";
	TRUE = "true";
	AND = "and";
	OR = "or";
	XOR = "xor";
	IN = "in";
	IS = "is";
	BETWEEN = "between";
	LIKE = "like";
	MATCHES = "matches";
	NULL_LITERAL = "null";
}

{
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
}

expr : expression EOF!;

exprList 
    : LPAREN! expression (SEMI! expression)+ RPAREN!
        { #exprList = #([EXPR,"expressionList","Spring.Expressions.ExpressionListNode"], #exprList); }
    ;

expression	:	logicalOrExpression 
				(
					(ASSIGN^ <AST = Spring.Expressions.AssignNode> logicalOrExpression) 
				|   (DEFAULT^ <AST = Spring.Expressions.DefaultNode> logicalOrExpression) 
				|	(QMARK^ <AST = Spring.Expressions.TernaryNode> expression COLON! expression)
				)?
			;
			
parenExpr
    : LPAREN! expression RPAREN!;
    
logicalOrExpression : logicalXorExpression (OR^ <AST = Spring.Expressions.OpOR> logicalXorExpression)* ;

logicalXorExpression : logicalAndExpression (XOR^ <AST = Spring.Expressions.OpXOR> logicalAndExpression)* ;
                        
logicalAndExpression : relationalExpression (AND^ <AST = Spring.Expressions.OpAND> relationalExpression)* ;                        

relationalExpression
    :   e1:sumExpr 
          (op:relationalOperator! e2:sumExpr
            {#relationalExpression = #(#[EXPR, op_AST.getText(), GetRelationalOperatorNodeType(op_AST.getText())], #relationalExpression);} 
          )?
    ;

sumExpr  : prodExpr (
                        (PLUS^ <AST = Spring.Expressions.OpADD> 
                        | MINUS^ <AST = Spring.Expressions.OpSUBTRACT>) prodExpr)* ; 

prodExpr : powExpr (
                        (STAR^ <AST = Spring.Expressions.OpMULTIPLY> 
                        | DIV^ <AST = Spring.Expressions.OpDIVIDE> 
                        | MOD^ <AST = Spring.Expressions.OpMODULUS>) powExpr)* ;

powExpr  : unaryExpression (POWER^ <AST = Spring.Expressions.OpPOWER> unaryExpression)? ;

unaryExpression 
	:	(PLUS^ <AST = Spring.Expressions.OpUnaryPlus> 
	    | MINUS^ <AST = Spring.Expressions.OpUnaryMinus> 
	    | BANG^ <AST = Spring.Expressions.OpNOT>) unaryExpression	
	|	primaryExpression
	;
	
unaryOperator
	: PLUS | MINUS | BANG
    ;
    
primaryExpression : startNode (node)?
			{ #primaryExpression = #([EXPR,"expression","Spring.Expressions.Expression"], #primaryExpression); };

startNode 
    : 
    (   (LPAREN expression SEMI) => exprList 
    |   parenExpr
    |   methodOrProperty 
    |   functionOrVar 
    |   localFunctionOrVar
    |   reference
    |   indexer 
    |   literal 
    |   type 
    |   constructor
    |   projection 
    |   selection 
    |   firstSelection 
    |   lastSelection 
    |   listInitializer
    |   mapInitializer
    |   lambda
    |   attribute
    )
    ;

node : 
    (   methodOrProperty 
    |   indexer 
    |   projection 
    |   selection 
    |   firstSelection 
    |   lastSelection 
    |   exprList
    |   DOT! 
    )+
    ;

functionOrVar 
    : (POUND ID LPAREN) => function
    | var
    ;

function : POUND! ID^ <AST = Spring.Expressions.FunctionNode> methodArgs
    ;
    
var : POUND! ID^ <AST = Spring.Expressions.VariableNode>;

localFunctionOrVar 
    : (DOLLAR ID LPAREN) => localFunction
    | localVar
    ;

localFunction 
    : DOLLAR! ID^ <AST = Spring.Expressions.LocalFunctionNode> methodArgs
    ;

localVar 
    : DOLLAR! ID^ <AST = Spring.Expressions.LocalVariableNode>
    ;

methodOrProperty
	: (ID LPAREN)=> ID^ <AST = Spring.Expressions.MethodNode> methodArgs
	| property
	;

methodArgs
	:  LPAREN! (argument (COMMA! argument)*)? RPAREN!
	;

property
    :  ID <AST = Spring.Expressions.PropertyOrFieldNode>
    ;

reference
	:  (AT! LPAREN! quotableName COLON) =>
		AT! LPAREN! cn:quotableName! COLON! id:quotableName! RPAREN!
		{ #reference = #([EXPR, "ref", "Spring.Context.Support.ReferenceNode"], #cn, #id); }

	|  AT! LPAREN! localid:quotableName! RPAREN!
       { #reference = #([EXPR, "ref", "Spring.Context.Support.ReferenceNode"], null, #localid); }
	;

indexer
	:  LBRACKET^ <AST = Spring.Expressions.IndexerNode> argument (COMMA! argument)* RBRACKET!
	;

projection
	:	
		PROJECT^ <AST = Spring.Expressions.ProjectionNode> expression RCURLY!
	;

selection
	:	
		SELECT^ <AST = Spring.Expressions.SelectionNode> expression (COMMA! expression)* RCURLY!
	;

firstSelection
	:	
		SELECT_FIRST^ <AST = Spring.Expressions.SelectionFirstNode> expression RCURLY!
	;

lastSelection
	:	
		SELECT_LAST^ <AST = Spring.Expressions.SelectionLastNode> expression RCURLY!
	;

type
    :   TYPE! tn:name! RPAREN!
		{ #type = #([EXPR, tn_AST.getText(), "Spring.Expressions.TypeNode"], #type); } 
    ;
     
name
	:	ID^ <AST = Spring.Expressions.QualifiedIdentifier> (~(RPAREN|COLON|QUOTE))*
	;
	
quotableName
    :	STRING_LITERAL^ <AST = Spring.Expressions.QualifiedIdentifier>
    |	name
    ;
    
attribute
	:	AT! LBRACKET! tn:qualifiedId! (ctorArgs)? RBRACKET!
		   { #attribute = #([EXPR, tn_AST.getText(), "Spring.Expressions.AttributeNode"], #attribute); }
	;

lambda
    :   LAMBDA! (argList)? PIPE! expression RCURLY!
		   { #lambda = #([EXPR, "lambda", "Spring.Expressions.LambdaExpressionNode"], #lambda); }
	;

argList : (ID (COMMA! ID)*)
		   { #argList = #([EXPR, "args"], #argList); }
	;

constructor
	:	("new" qualifiedId LPAREN) => "new"! type:qualifiedId! ctorArgs
		   { #constructor = #([EXPR, type_AST.getText(), "Spring.Expressions.ConstructorNode"], #constructor); }
	|   arrayConstructor
	;

arrayConstructor
	:	 "new"! type:qualifiedId! arrayRank (listInitializer)?
	       { #arrayConstructor = #([EXPR, type_AST.getText(), "Spring.Expressions.ArrayConstructorNode"], #arrayConstructor); }
	;
    
arrayRank
    :   LBRACKET^ (expression (COMMA! expression)*)? RBRACKET!
    ;

listInitializer
    :   LCURLY^ <AST = Spring.Expressions.ListInitializerNode> expression (COMMA! expression)* RCURLY!
    ;

mapInitializer
    :   POUND! LCURLY^ <AST = Spring.Expressions.MapInitializerNode> mapEntry (COMMA! mapEntry)* RCURLY!
    ;
      
mapEntry
    :   expression COLON! expression
          { #mapEntry = #([EXPR, "entry", "Spring.Expressions.MapEntryNode"], #mapEntry); }
    ;
     
ctorArgs : LPAREN! (namedArgument (COMMA! namedArgument)*)? RPAREN!;
            
argument : expression;

namedArgument 
    :   (ID ASSIGN) => ID^ <AST = Spring.Expressions.NamedArgumentNode> ASSIGN! expression 
    |   argument 
    ;

qualifiedId 
	: ID^ <AST = Spring.Expressions.QualifiedIdentifier> (DOT ID)*
    ;
    
literal
	:	NULL_LITERAL <AST = Spring.Expressions.NullLiteralNode>
	|   INTEGER_LITERAL <AST = Spring.Expressions.IntLiteralNode>
	|   HEXADECIMAL_INTEGER_LITERAL <AST = Spring.Expressions.HexLiteralNode>
	|   REAL_LITERAL <AST = Spring.Expressions.RealLiteralNode>
	|	STRING_LITERAL <AST = Spring.Expressions.StringLiteralNode>
	|   boolLiteral
	;

boolLiteral
    :   TRUE <AST = Spring.Expressions.BooleanLiteralNode>
    |   FALSE <AST = Spring.Expressions.BooleanLiteralNode>
    ;
    
relationalOperator
    :   EQUAL 
    |   NOT_EQUAL
    |   LESS_THAN
    |   LESS_THAN_OR_EQUAL      
    |   GREATER_THAN            
    |   GREATER_THAN_OR_EQUAL 
    |   IN   
    |   IS   
    |   BETWEEN   
    |   LIKE   
    |   MATCHES   
    ; 
    


class ExpressionLexer extends Lexer;

options {
    charVocabulary = '\u0000' .. '\uFFFE'; 
	classHeaderPrefix = "internal"; 
	k = 2;
	testLiterals = false;
}

{
    // CLOVER:OFF
}

WS	:	(' '
	|	'\t'
	|	'\n'
	|	'\r')
		{ _ttype = Token.SKIP; }
	;

AT: '@'
  ;

BACKTICK: '`'
  ;
  
BACKSLASH: '\\'
  ;
    
PIPE: '|'
  ;

BANG: '!'
  ;

QMARK: '?'
  ;

DOLLAR: '$'
  ;

POUND: '#'
  ;
    
LPAREN:	'('
	;

RPAREN:	')'
	;

LBRACKET:	'['
	;

RBRACKET:	']'
	;

LCURLY:	'{'
	;

RCURLY:	'}'
	;

COMMA : ','
   ;

SEMI: ';'
  ;

COLON: ':'
  ;

ASSIGN: '='
  ;

DEFAULT: "??"
  ;
  
PLUS: '+'
  ;

MINUS: '-'
  ;
   
DIV: '/'
  ;

STAR: '*'
  ;

MOD: '%'
  ;

POWER: '^'
  ;
  
EQUAL: "=="
  ;

NOT_EQUAL: "!="
  ;

LESS_THAN: '<'
  ;

LESS_THAN_OR_EQUAL: "<="
  ;

GREATER_THAN: '>'
  ;

GREATER_THAN_OR_EQUAL: ">="
  ;

PROJECT: "!{"
  ;
  
SELECT: "?{"
  ;

SELECT_FIRST: "^{"
  ;
  
SELECT_LAST: "${"
  ;

TYPE: "T("
  ;

LAMBDA: "{|"
  ;

DOT_ESCAPED: "\\."
  ;
  
QUOTE: '\''
  ;
  
STRING_LITERAL
	:	QUOTE! (APOS|~'\'')* QUOTE!
	;

protected
APOS : QUOTE! QUOTE
    ;
  
ID
options {
	testLiterals = true;
}
	: ('a'..'z'|'A'..'Z'|'_') ('a'..'z'|'A'..'Z'|'_'|'0'..'9')*
	;

NUMERIC_LITERAL

	// real
	:	('.' DECIMAL_DIGIT) =>
		 '.' (DECIMAL_DIGIT)+ (EXPONENT_PART)? (REAL_TYPE_SUFFIX)?
		{$setType(REAL_LITERAL);}
			
	|	((DECIMAL_DIGIT)+ '.' DECIMAL_DIGIT) =>
		 (DECIMAL_DIGIT)+ '.' (DECIMAL_DIGIT)+ (EXPONENT_PART)? (REAL_TYPE_SUFFIX)?
		{$setType(REAL_LITERAL);}
		
	|	((DECIMAL_DIGIT)+ (EXPONENT_PART)) =>
		 (DECIMAL_DIGIT)+ (EXPONENT_PART) (REAL_TYPE_SUFFIX)?
		{$setType(REAL_LITERAL);}
		
	|	((DECIMAL_DIGIT)+ (REAL_TYPE_SUFFIX)) =>
		 (DECIMAL_DIGIT)+ (REAL_TYPE_SUFFIX)		
		{$setType(REAL_LITERAL);}
		 
	// integer
	|	 (DECIMAL_DIGIT)+ (INTEGER_TYPE_SUFFIX)?	
		{$setType(INTEGER_LITERAL);}
	
	// just a dot
	| '.'{$setType(DOT);}
	;

	
HEXADECIMAL_INTEGER_LITERAL
	:	"0x"   (HEX_DIGIT)+   (INTEGER_TYPE_SUFFIX)?
	;

protected
DECIMAL_DIGIT
	: 	'0'..'9'
	;
	
protected	
INTEGER_TYPE_SUFFIX
	:
	(	options {generateAmbigWarnings=false;}
		:	"UL"	| "LU" 	| "ul"	| "lu"
		|	"UL"	| "LU" 	| "uL"	| "lU"
		|	"U"		| "L"	| "u"	| "l"
	)
	;
		
protected
HEX_DIGIT
	:	'0' | '1' | '2' | '3' | '4' | '5' | '6' | '7' | '8' | '9' | 
		'A' | 'B' | 'C' | 'D' | 'E' | 'F'  |
		'a' | 'b' | 'c' | 'd' | 'e' | 'f'
	;	
	
protected	
EXPONENT_PART
	:	"e"  (SIGN)*  (DECIMAL_DIGIT)+
	|	"E"  (SIGN)*  (DECIMAL_DIGIT)+
	;	
	
protected
SIGN
	:	'+' | '-'
	;
	
protected
REAL_TYPE_SUFFIX
	: 'F' | 'f' | 'D' | 'd' | 'M' | 'm'
	;
