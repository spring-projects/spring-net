using Spring.Context;

namespace Spring.Validation
{
    /// <summary>
    /// An interface that validation errors containers have to implement.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public interface IValidationErrors
    {
        /// <summary>
        /// Does this instance contain any validation errors?
        /// </summary>
        /// <remarks>
        /// <p>
        /// If this returns <see lang="true"/>, this means that it (obviously)
        /// contains no validation errors.
        /// </p>
        /// </remarks>
        /// <value><see lang="true"/> if this instance is empty.</value>
        bool IsEmpty { get; }

        /// <summary>
        /// Gets the list of all error providers.
        /// </summary>
        IList<string> Providers { get; }

        /// <summary>
        /// Adds the supplied <paramref name="message"/> to this
        /// instance's collection of errors.
        /// </summary>
        /// <param name="provider">
        /// The provider that should be used for message grouping; can't be
        /// <see lang="null"/>.
        /// </param>
        /// <param name="message">The error message to add.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="provider"/> or <paramref name="message"/> is <see langword="null"/>.
        /// </exception>
        void AddError(string provider, ErrorMessage message);

        /// <summary>
        /// Merges another instance of <see cref="ValidationErrors"/> into this one.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If the supplied <paramref name="errorsToMerge"/> is <see lang="null"/>,
        /// then no errors will be added to this instance, and this method will
        /// (silently) return.
        /// </p>
        /// </remarks>
        /// <param name="errorsToMerge">
        /// The validation errors to merge; can be <see lang="null"/>.
        /// </param>
        void MergeErrors(IValidationErrors errorsToMerge);

        /// <summary>
        /// Gets the list of errors for the supplied error <paramref name="provider"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If there are no errors for the supplied <paramref name="provider"/>,
        /// an <b>empty</b> <see cref="System.Collections.IList"/> will be returned.
        /// </p>
        /// </remarks>
        /// <param name="provider">Error key that was used to group messages.</param>
        /// <returns>
        /// A list of all <see cref="ErrorMessage"/>s for the supplied lookup <paramref name="provider"/>.
        /// </returns>
        IList<ErrorMessage> GetErrors(string provider);

        /// <summary>
        /// Gets the list of resolved error messages for the supplied lookup <paramref name="provider"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If there are no errors for the supplied lookup <paramref name="provider"/>,
        /// an <b>empty</b> <see cref="System.Collections.IList"/> will be returned.
        /// </p>
        /// </remarks>
        /// <param name="provider">Error key that was used to group messages.</param>
        /// <param name="messageSource"><see cref="IMessageSource"/> to resolve messages against.</param>
        /// <returns>
        /// A list of resolved error messages for the supplied lookup <paramref name="provider"/>.
        /// </returns>
        IList<string> GetResolvedErrors(string provider, IMessageSource messageSource);
    }
}
