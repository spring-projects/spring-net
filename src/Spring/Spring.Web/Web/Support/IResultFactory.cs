namespace Spring.Web.Support
{
    public interface IResultFactory
    {
        IResult CreateResult( string resultMode, string resultText );
    }
}