namespace Spring.Expressions.Parser.antlr;
/*ANTLR Translator Generator
 * Project led by Terence Parr at http://www.jGuru.com
 * Software rights: http://www.antlr.org/license.html
 *
 * $Id:$
 */

//
// ANTLR C# Code Generator by Micheal Jordan
//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
//                            Anthony Oguntimehin
//
// With many thanks to Eric V. Smith from the ANTLR list.
//

public class CommonToken : Token
{
    public static readonly CommonToken.CommonTokenCreator Creator = new CommonTokenCreator();

    // most tokens will want line and text information
    protected internal int line;
    protected internal string text = null;
    protected internal int col;

    public CommonToken()
    {
    }

    public CommonToken(int t, string txt)
    {
        type_ = t;
        setText(txt);
    }

    public CommonToken(string s)
    {
        text = s;
    }

    override public int getLine()
    {
        return line;
    }

    override public string getText()
    {
        return text;
    }

    override public void setLine(int l)
    {
        line = l;
    }

    override public void setText(string s)
    {
        text = s;
    }

    override public string ToString()
    {
        return "[\"" + getText() + "\",<" + type_ + ">,line=" + line + ",col=" + col + "]";
    }

    /*Return token's start column */
    override public int getColumn()
    {
        return col;
    }

    override public void setColumn(int c)
    {
        col = c;
    }

    public class CommonTokenCreator : TokenCreator
    {
        public CommonTokenCreator() { }

        /// <summary>
        /// Returns the fully qualified name of the Token type that this
        /// class creates.
        /// </summary>
        public override string TokenTypeName
        {
            get
            {
                return typeof(antlr.CommonToken).FullName;
                ;
            }
        }

        /// <summary>
        /// Constructs a <see cref="Token"/> instance.
        /// </summary>
        public override IToken Create()
        {
            return new CommonToken();
        }
    }
}
