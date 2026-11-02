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
/// Exception thrown when detecting invalid SpEL syntax
/// </summary>
/// <author>Erich Eichinger</author>
[Serializable]
public class SyntaxErrorException : Exception, ISerializable
{
    private readonly string _expression;
    private readonly int line;
    private readonly int column;

    ///<summary>
    ///</summary>
    public int Line
    {
        get { return line; }
    }

    ///<summary>
    ///</summary>
    public int Column
    {
        get { return column; }
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
    /// TODO
    /// </summary>
    public SyntaxErrorException(string message, int line, int column, string expression)
        : base(message)
    {
        this.line = line;
        this.column = column;
        _expression = expression;
    }

    /// <summary>
    /// TODO
    /// </summary>
#if NET
    [Obsolete(Obsoletions.LegacyFormatterImplMessage, DiagnosticId = Obsoletions.LegacyFormatterImplDiagId, UrlFormat = Obsoletions.SharedUrlFormat)]
#endif
    protected SyntaxErrorException(SerializationInfo info, StreamingContext context) : base(info.GetString("Message"))
    {
        this.line = info.GetInt32("Line");
        this.column = info.GetInt32("Column");
        this._expression = info.GetString("Expression");
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
#if NET
    [Obsolete(Obsoletions.LegacyFormatterImplMessage, DiagnosticId = Obsoletions.LegacyFormatterImplDiagId, UrlFormat = Obsoletions.SharedUrlFormat)]
#endif
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        // Message is the text without the position, which the constructor gets again
        info.AddValue("Line", this.line);
        info.AddValue("Column", this.column);
        info.AddValue("Message", base.Message);
        info.AddValue("Expression", this._expression);
    }
}
