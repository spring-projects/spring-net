using System.Runtime.Serialization;
using Spring.Expressions.Parser.antlr;
using Spring.Expressions.Parser.antlr.collections;

namespace Spring.Expressions
{
    /// <summary>
    /// For internal purposes only. Use <see cref="BaseNode"/> for expression node implementations.
    /// </summary>
    /// <remarks>
    /// This class is only required to enable serialization of parsed Spring expressions since antlr.CommonAST
    /// unfortunately is not marked as [Serializable].<br/>
    /// <br/>
    /// <b>Note:</b>Since SpringAST implements <see cref="ISerializable"/>, deriving classes
    /// have to explicitely override <see cref="GetObjectData"/> if they need to persist additional
    /// data during serialization.
    /// </remarks>
    [Serializable]
    public class SpringAST : Parser.antlr.BaseAST, ISerializable
    {
        #region Global SpringAST Factory

        internal class SpringASTCreator : Parser.antlr.ASTNodeCreator
        {
            public override Parser.antlr.collections.AST Create()
            {
                return new SpringAST();
            }

            public override string ASTNodeTypeName
            {
                get { return typeof(SpringAST).FullName; }
            }
        }

        /// <summary>
        /// The global SpringAST node factory
        /// </summary>
        internal static readonly SpringASTCreator Creator = new SpringASTCreator();

        #endregion

        #region Members

        private string text;
        private int ttype;

        #endregion

        /// <summary>
        /// Create an instance
        /// </summary>
        public SpringAST()
        {}

        /// <summary>
        /// Create an instance from a token
        /// </summary>
        public SpringAST(IToken token)
        {
            initialize(token);
        }

        /// <summary>
        /// initialize this instance from an AST
        /// </summary>
        public override void initialize(AST t)
        {
            this.setText(t.getText());
            this.Type = t.Type;
        }

        /// <summary>
        /// initialize this instance from an IToken
        /// </summary>
        public override void initialize(IToken tok)
        {
            this.setText(tok.getText());
            this.Type = tok.Type;
        }

        /// <summary>
        /// initialize this instance from a token type number and a text
        /// </summary>
        public override void initialize(int t, string txt)
        {
            this.Type = t;
            this.setText(txt);
        }

        /// <summary>
        /// gets or sets the token type of this node
        /// </summary>
        public override int Type
        {
            get { return this.ttype; }
            set { this.ttype = value; }
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
        public override void setText(string txt)
        {
            this.text = txt;
        }

        /// <summary>
        /// gets the text of this node
        /// </summary>
        public override string getText()
        {
            return this.text;
        }

        #region ISerializable Implementation

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected SpringAST(SerializationInfo info, StreamingContext context)
        {
            base.down = (BaseAST)info.GetValue("down", typeof(BaseAST));
            base.right = (BaseAST)info.GetValue("right", typeof(BaseAST));
            this.ttype = info.GetInt32("ttype");
            this.text = info.GetString("text");
        }

        /// <summary>
        /// populate SerializationInfo from this instance
        /// </summary>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("down", base.down, typeof(SpringAST));
            info.AddValue("right", base.right, typeof(SpringAST));
            info.AddValue("ttype", this.Type, typeof(int));
            info.AddValue("text", this.Text, typeof(string));
        }

        #endregion
    }
}
