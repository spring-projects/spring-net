namespace Spring.Util
{
    /// <summary>
    /// Simple strategy interface for resolving a String value.
    /// </summary>
    public interface IStringValueResolver
    {
        /// <summary>
        /// Resolve the given String value, for example parsing placeholders.
        /// </summary>
        /// <param name="value">the original String value</param>
        /// <returns>the resolved String value</returns>
        string ParseAndResolveVariables(string value);
    }
}
