#region License

/*
 * Copyright © 2002-2011 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using Spring.Expressions.Parser;
using Spring.Expressions.Parser.antlr;
using Spring.Expressions.Parser.antlr.collections;
using Spring.Core;
using Spring.Reflection.Dynamic;
using Spring.Util;
using StringUtils = Spring.Util.StringUtils;

namespace Spring.Expressions
{
    /// <summary>
    /// Container object for the parsed expression.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Preparing this object once and reusing it many times for expression
    /// evaluation can result in significant performance improvements, as
    /// expression parsing and reflection lookups are only performed once.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class Expression : BaseNode
    {
        /// <summary>
        /// Contains a list of reserved variable names.
        /// You must not use any variable names with the reserved prefix!
        /// </summary>
        public class ReservedVariableNames
        {
            /// <summary>
            /// Variable Names using this prefix are reserved for internal framework use
            /// </summary>
            public static readonly string RESERVEDPREFIX = "____spring_";

            /// <summary>
            /// variable name of the currently processed object factory, if any
            /// </summary>
            internal static readonly string CurrentObjectFactory = RESERVEDPREFIX + "CurrentObjectFactory";
        }

        private class ASTNodeCreator : Parser.antlr.ASTNodeCreator
        {
            private readonly SafeConstructor ctor;
            private readonly string name;

            public ASTNodeCreator(ConstructorInfo ctor)
            {
                this.ctor = new SafeConstructor(ctor);
                this.name = ctor.DeclaringType.FullName;
            }

            public override AST Create()
            {
                return (AST) ctor.Invoke(new object[0]);
            }

            public override string ASTNodeTypeName
            {
                get { return name; }
            }
        }

        private class SpringASTFactory : ASTFactory
        {
            private static readonly Type BASENODE_TYPE;
            private static readonly Hashtable Typename2Creator;

            static SpringASTFactory()
            {
                BASENODE_TYPE = typeof (SpringAST);

                Typename2Creator = new Hashtable();
                foreach (Type type in typeof(SpringASTFactory).Assembly.GetTypes())
                {
                    if (BASENODE_TYPE.IsAssignableFrom(type) && !type.IsAbstract)
                    {
                        ConstructorInfo ctor = type.GetConstructor(System.Type.EmptyTypes);
                        if (ctor != null)
                        {
                            ASTNodeCreator creator = new ASTNodeCreator(ctor);
                            Typename2Creator[creator.ASTNodeTypeName] = creator;
                        }
                    }
                }
                Typename2Creator[BASENODE_TYPE.FullName] = SpringAST.Creator;
            }

            public SpringASTFactory() : base(BASENODE_TYPE)
            {
                base.defaultASTNodeTypeObject_ = BASENODE_TYPE;
                base.typename2creator_ = Typename2Creator;
            }
        }

        private class SpringExpressionParser : ExpressionParser
        {
            public SpringExpressionParser( TokenStream lexer )
                : base( lexer )
            {
                base.astFactory = new SpringASTFactory();
                base.initialize();
            }
        }

        static Expression()
        {
            // Ensure antlr is loaded (fixes GAC issues)!
            Assembly antlrAss = typeof( Parser.antlr.LLkParser ).Assembly;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Expression"/> class
        /// by parsing specified expression string.
        /// </summary>
        /// <param name="expression">Expression to parse.</param>
        public static IExpression Parse( string expression )
        {
            if (StringUtils.HasText( expression ))
            {
                ExpressionLexer lexer = new ExpressionLexer( new StringReader( expression ) );
                ExpressionParser parser = new SpringExpressionParser( lexer );

                try
                {
                    parser.expr();
                }
                catch (TokenStreamRecognitionException ex)
                {
                    throw new SyntaxErrorException( ex.recog.Message, ex.recog.getLine(), ex.recog.getColumn(), expression );
                }
                return (IExpression)parser.getAST();
            }
            else
            {
                return new Expression();
            }
        }

        /// <summary>
        /// Registers lambda expression under the specified <paramref name="functionName"/>.
        /// </summary>
        /// <param name="functionName">Function name to register expression as.</param>
        /// <param name="lambdaExpression">Lambda expression to register.</param>
        /// <param name="variables">Variables dictionary that the function will be registered in.</param>
        public static void RegisterFunction( string functionName, string lambdaExpression, IDictionary variables )
        {
            AssertUtils.ArgumentHasText( functionName, "functionName" );
            AssertUtils.ArgumentHasText( lambdaExpression, "lambdaExpression" );

            ExpressionLexer lexer = new ExpressionLexer( new StringReader( lambdaExpression ) );
            ExpressionParser parser = new SpringExpressionParser( lexer );

            try
            {
                parser.lambda();
            }
            catch (TokenStreamRecognitionException ex)
            {
                throw new SyntaxErrorException( ex.recog.Message, ex.recog.getLine(), ex.recog.getColumn(), lambdaExpression );
            }
            variables[functionName] = parser.getAST();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Expression"/> class
        /// by parsing specified primary expression string.
        /// </summary>
        /// <param name="expression">Primary expression to parse.</param>
        internal static IExpression ParsePrimary( string expression )
        {
            if (StringUtils.HasText( expression ))
            {
                ExpressionLexer lexer = new ExpressionLexer( new StringReader( expression ) );
                ExpressionParser parser = new SpringExpressionParser( lexer );

                try
                {
                    parser.primaryExpression();
                }
                catch (TokenStreamRecognitionException ex)
                {
                    throw new SyntaxErrorException( ex.recog.Message, ex.recog.getLine(), ex.recog.getColumn(), expression );
                }
                return (IExpression)parser.getAST();
            }
            else
            {
                return new Expression();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Expression"/> class
        /// by parsing specified property expression string.
        /// </summary>
        /// <param name="expression">Property expression to parse.</param>
        internal static IExpression ParseProperty( string expression )
        {
            if (StringUtils.HasText( expression ))
            {
                ExpressionLexer lexer = new ExpressionLexer( new StringReader( expression ) );
                ExpressionParser parser = new SpringExpressionParser( lexer );

                try
                {
                    parser.property();
                }
                catch (TokenStreamRecognitionException ex)
                {
                    throw new SyntaxErrorException( ex.recog.Message, ex.recog.getLine(), ex.recog.getColumn(), expression );
                }
                return (IExpression)parser.getAST();
            }
            else
            {
                return new Expression();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Expression"/> class.
        /// </summary>
        public Expression()
        { }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected Expression( SerializationInfo info, StreamingContext context )
            : base( info, context )
        { }

        /// <summary>
        /// Evaluates this expression for the specified root object and returns
        /// value of the last node.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>Value of the last node.</returns>
        protected override object Get(object context, EvaluationContext evalContext)
        {
            object result = context;

            if (this.getNumberOfChildren() > 0)
            {
                AST node = this.getFirstChild();
                while (node != null)
                {
                    result = GetValue(((BaseNode)node), result, evalContext );

                    node = node.getNextSibling();
                }
            }

            return result;
        }

        /// <summary>
        /// Evaluates this expression for the specified root object and sets
        /// value of the last node.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <param name="newValue">Value to set last node to.</param>
        /// <exception cref="NotSupportedException">If navigation expression is empty.</exception>
        protected override void Set( object context, EvaluationContext evalContext, object newValue )
        {
            object target = context;

            if (this.getNumberOfChildren() > 0)
            {
                AST node = this.getFirstChild();

                for (int i = 0; i < this.getNumberOfChildren() - 1; i++)
                {
                    try
                    {
                        target = GetValue(((BaseNode)node), target, evalContext);
                        node = node.getNextSibling();
                    }
                    catch (NotReadablePropertyException e)
                    {
                        throw new NotWritablePropertyException( "Cannot read the value of '" + node.getText() + "' property in the expression.", e );
                    }
                }
                SetValue(((BaseNode)node), target, evalContext, newValue);
            }
            else
            {
                throw new NotSupportedException( "You cannot set the value for an empty expression." );
            }
        }

        /// <summary>
        /// Evaluates this expression for the specified root object and returns
        /// <see cref="PropertyInfo"/> of the last node, if possible.
        /// </summary>
        /// <param name="context">Context to evaluate expression against.</param>
        /// <param name="variables">Expression variables map.</param>
        /// <returns>Value of the last node.</returns>
        internal PropertyInfo GetPropertyInfo( object context, IDictionary<string, object> variables )
        {
            if (this.getNumberOfChildren() > 0)
            {
                object target = context;
                AST node = this.getFirstChild();

                for (int i = 0; i < this.getNumberOfChildren() - 1; i++)
                {
                    target = ((IExpression)node).GetValue( target, variables );
                    node = node.getNextSibling();
                }

                if (node is PropertyOrFieldNode)
                {
                    return (PropertyInfo)((PropertyOrFieldNode)node).GetMemberInfo( target );
                }
                else if (node is IndexerNode)
                {
                    return ((IndexerNode)node).GetPropertyInfo( target, variables );
                }
                else
                {
                    throw new FatalReflectionException( "Cannot obtain PropertyInfo from an expression that does not resolve to a property or an indexer." );
                }
            }

            throw new FatalReflectionException( "Cannot obtain PropertyInfo for empty property name." );
        }
    }
}
