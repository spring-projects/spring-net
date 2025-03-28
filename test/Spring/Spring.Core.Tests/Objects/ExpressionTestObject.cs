using System.Collections;
using Spring.Expressions;

namespace Spring.Objects;

public class ExpressionTestObject
{
    private IExpression expressionOne;
    private IExpression expressionTwo;
    private string someString;
    private DateTime someDate;
    private IDictionary someDictionary;

    public ExpressionTestObject(string someString)
    {
        this.someString = someString;
    }

    public IExpression ExpressionOne
    {
        get { return expressionOne; }
        set { expressionOne = value; }
    }

    public IExpression ExpressionTwo
    {
        get { return expressionTwo; }
        set { expressionTwo = value; }
    }

    public string SomeString
    {
        get { return someString; }
    }

    public DateTime SomeDate
    {
        get { return someDate; }
        set { someDate = value; }
    }

    public IDictionary SomeDictionary
    {
        get { return someDictionary; }
        set { someDictionary = value; }
    }
}
