/*
 * Copyright � 2002-2011 the original author or authors.
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

using System.Runtime.Serialization;

namespace Spring.Expressions;

/// <summary>
/// Exception thrown by <see cref="Spring.Expressions.Expression.Parse"/> when detecting invalid SpEL syntax, including
/// an expression nested too deeply to be parsed.
/// </summary>
/// <author>Erich Eichinger</author>
[Serializable]
public class SyntaxErrorException : Exception, ISerializable
{
    private readonly string _expression;
    private readonly int _line;
    private readonly int _column;

    ///<summary>
    /// The line of the error, which is always 1: the columns are those of the whole expression
    ///</summary>
    public int Line
    {
        get { return _line; }
    }

    ///<summary>
    /// The column of the error in the expression, from 1, a tab moving it to the next multiple of 8 plus 1
    ///</summary>
    public int Column
    {
        get { return _column; }
    }

    ///<summary>
    ///Gets a message that provides details on the syntax error.
    ///</summary>
    public override string Message
    {
        get
        {
            return string.Format("Syntax Error on line {0}, column {1}: {2} in expression{3}'{4}'", Line, Column, base.Message, Environment.NewLine, _expression);
        }
    }

    ///<summary>
    /// The expression that caused the error
    ///</summary>
    public string Expression
    {
        get { return _expression; }
    }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="message">The text of the error, without its position.</param>
    /// <param name="line">The line of the error.</param>
    /// <param name="column">The column of the error.</param>
    /// <param name="expression">The expression that caused the error.</param>
    public SyntaxErrorException(string message, int line, int column, string expression)
        : base(message)
    {
        _line = line;
        _column = column;
        _expression = expression;
    }

    /// <summary>
    /// Creates a new instance from SerializationInfo.
    /// </summary>
#if NET
    [Obsolete(Obsoletions.LegacyFormatterImplMessage, DiagnosticId = Obsoletions.LegacyFormatterImplDiagId, UrlFormat = Obsoletions.SharedUrlFormat)]
#endif
    protected SyntaxErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        _line = info.GetInt32("Line");
        _column = info.GetInt32("Column");
        _expression = info.GetString("Expression");
    }

    /// <summary>
    /// Populates SerializationInfo from this instance.
    /// </summary>
    /// <param name="info">The object to populate.</param>
    /// <param name="context">The destination of this serialization.</param>
#if NET
    [Obsolete(Obsoletions.LegacyFormatterImplMessage, DiagnosticId = Obsoletions.LegacyFormatterImplDiagId, UrlFormat = Obsoletions.SharedUrlFormat)]
#endif
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue("Line", _line);
        info.AddValue("Column", _column);
        info.AddValue("Expression", _expression);
    }
}
