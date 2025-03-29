using IEnumerator = System.Collections.IEnumerator;
using AST = Spring.Expressions.Parser.antlr.collections.AST;

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

/*There is only one instance of this class **/
public class ASTNULLType : AST
{
    public virtual void addChild(AST c) { }

    public virtual bool Equals(AST t)
    {
        return false;
    }

    public virtual bool EqualsList(AST t)
    {
        return false;
    }

    public virtual bool EqualsListPartial(AST t)
    {
        return false;
    }

    public virtual bool EqualsTree(AST t)
    {
        return false;
    }

    public virtual bool EqualsTreePartial(AST t)
    {
        return false;
    }

    public virtual IEnumerator findAll(AST tree)
    {
        return null;
    }

    public virtual IEnumerator findAllPartial(AST subtree)
    {
        return null;
    }

    public virtual AST getFirstChild()
    {
        return this;
    }

    public virtual AST getNextSibling()
    {
        return this;
    }

    public virtual string getText()
    {
        return "<ASTNULL>";
    }

    public virtual int Type
    {
        get { return Token.NULL_TREE_LOOKAHEAD; }
        set { ; }
    }

    public int getNumberOfChildren()
    {
        return 0;
    }

    public virtual void initialize(int t, string txt)
    {
    }

    public virtual void initialize(AST t)
    {
    }

    public virtual void initialize(IToken t)
    {
    }

    public virtual void setFirstChild(AST c)
    {
        ;
    }

    public virtual void setNextSibling(AST n)
    {
        ;
    }

    public virtual void setText(string text)
    {
        ;
    }

    public virtual void setType(int ttype)
    {
        this.Type = ttype;
    }

    override public string ToString()
    {
        return getText();
    }

    public virtual string ToStringList()
    {
        return getText();
    }

    public virtual string ToStringTree()
    {
        return getText();
    }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
