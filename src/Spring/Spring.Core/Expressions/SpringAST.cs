using System.Runtime.Serialization;
using System.Text;

namespace Spring.Expressions;

/// <summary>
/// For internal purposes only. Use <see cref="BaseNode"/> for expression node implementations.
/// </summary>
/// <remarks>
/// A node of a parsed expression: its first child, its next sibling, a token type and a text.<br/>
/// <br/>
/// <b>Note:</b>Since SpringAST implements <see cref="ISerializable"/>, deriving classes
/// have to explicitely override <see cref="GetObjectData"/> if they need to persist additional
/// data during serialization.
/// </remarks>
[Serializable]
public class SpringAST : ISerializable
{
    private SpringAST down;
    private SpringAST right;
    private string text;
    private int ttype;

    /// <summary>
    /// Create an instance
    /// </summary>
    public SpringAST()
    {
    }

    /// <summary>
    /// initialize this instance from an AST
    /// </summary>
    public virtual void initialize(SpringAST t)
    {
        this.setText(t.getText());
        this.Type = t.Type;
    }

    /// <summary>
    /// initialize this instance from a token type number and a text
    /// </summary>
    public virtual void initialize(int t, string txt)
    {
        this.Type = t;
        this.setText(txt);
    }

    /// <summary>
    /// gets or sets the token type of this node
    /// </summary>
    public virtual int Type
    {
        get { return this.ttype; }
        set { this.ttype = value; }
    }

    /// <summary>
    /// sets the token type of this node
    /// </summary>
    public virtual void setType(int ttype)
    {
        this.Type = ttype;
    }

    /// <summary>
    /// gets or sets the text of this node
    /// </summary>
    public string Text
    {
        get { return this.getText(); }
        set { this.setText(value); }
    }

    /// <summary>
    /// sets the text of this node
    /// </summary>
    public virtual void setText(string txt)
    {
        this.text = txt;
    }

    /// <summary>
    /// gets the text of this node
    /// </summary>
    public virtual string getText()
    {
        return this.text;
    }

    /// <summary>
    /// adds a node to the end of the children of this node
    /// </summary>
    public virtual void addChild(SpringAST node)
    {
        if (node == null)
        {
            return;
        }

        var t = this.down;
        if (t == null)
        {
            this.down = node;
            return;
        }

        while (t.right != null)
        {
            t = t.right;
        }

        t.right = node;
    }

    /// <summary>
    /// gets the first child of this node
    /// </summary>
    public virtual SpringAST getFirstChild()
    {
        return this.down;
    }

    /// <summary>
    /// sets the first child of this node
    /// </summary>
    public virtual void setFirstChild(SpringAST c)
    {
        this.down = c;
    }

    /// <summary>
    /// gets the next sibling of this node
    /// </summary>
    public virtual SpringAST getNextSibling()
    {
        return this.right;
    }

    /// <summary>
    /// sets the next sibling of this node
    /// </summary>
    public virtual void setNextSibling(SpringAST n)
    {
        this.right = n;
    }

    /// <summary>
    /// removes the children of this node
    /// </summary>
    public virtual void removeChildren()
    {
        this.down = null;
    }

    /// <summary>
    /// gets the number of children of this node, 0 for a leaf
    /// </summary>
    public int getNumberOfChildren()
    {
        var n = 0;
        for (var t = this.down; t != null; t = t.right)
        {
            n++;
        }

        return n;
    }

    /// <summary>
    /// whether <paramref name="obj"/> is a node of the same class, token type and text, whatever its children
    /// </summary>
    public override bool Equals(object obj)
    {
        return obj != null && this.GetType() == obj.GetType() && Equals((SpringAST) obj);
    }

    /// <summary>
    /// whether <paramref name="t"/> has the same token type and text, whatever its children
    /// </summary>
    public virtual bool Equals(SpringAST t)
    {
        return t != null && object.Equals(this.getText(), t.getText()) && this.Type == t.Type;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    /// <summary>
    /// returns the text of this node
    /// </summary>
    public override string ToString()
    {
        return getText();
    }

    /// <summary>
    /// prints this node, its children and its next siblings in LISP notation
    /// </summary>
    public virtual string ToStringList()
    {
        var builder = new StringBuilder();
        for (var t = this; t != null; t = t.right)
        {
            builder.Append(t.ToStringTree());
        }

        return builder.ToString();
    }

    /// <summary>
    /// prints this node and its children in LISP notation
    /// </summary>
    public virtual string ToStringTree()
    {
        return this.down == null
            ? " " + ToString()
            : " ( " + ToString() + this.down.ToStringList() + " )";
    }

    /// <summary>
    /// Create a new instance from SerializationInfo
    /// </summary>
#if NET
    [Obsolete(Obsoletions.LegacyFormatterImplMessage, DiagnosticId = Obsoletions.LegacyFormatterImplDiagId, UrlFormat = Obsoletions.SharedUrlFormat)]
#endif
    protected SpringAST(SerializationInfo info, StreamingContext context)
    {
        this.down = (SpringAST) info.GetValue("down", typeof(SpringAST));
        this.right = (SpringAST) info.GetValue("right", typeof(SpringAST));
        this.ttype = info.GetInt32("ttype");
        this.text = info.GetString("text");
    }

    /// <summary>
    /// populate SerializationInfo from this instance
    /// </summary>
#if NET
    [Obsolete(Obsoletions.LegacyFormatterImplMessage, DiagnosticId = Obsoletions.LegacyFormatterImplDiagId, UrlFormat = Obsoletions.SharedUrlFormat)]
#endif
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("down", this.down, typeof(SpringAST));
        info.AddValue("right", this.right, typeof(SpringAST));
        info.AddValue("ttype", this.Type, typeof(int));
        info.AddValue("text", this.Text, typeof(string));
    }
}
