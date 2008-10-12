namespace Spring.Web.Support
{
    public interface IResult
    {
        void Navigate( object context );
        string GetRedirectUri( object context );
    }
}