namespace Spring.Expressions.Parser.antlr.collections.impl;
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

/*ASTArray is a class that allows ANTLR to
 * generate code that can create and initialize an array
 * in one expression, like:
 *    (new ASTArray(3)).add(x).add(y).add(z)
 */
public class ASTArray
{
    public int size = 0;
    public AST[] array;

    public ASTArray(int capacity)
    {
        array = new AST[capacity];
    }

    public virtual ASTArray add(AST node)
    {
        array[size++] = node;
        return this;
    }
}
