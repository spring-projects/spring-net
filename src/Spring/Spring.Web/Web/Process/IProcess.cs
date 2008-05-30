using System;

namespace Spring.Web.Process
{
    /// <summary>
    /// An interface that different process implementations need to support.
    /// </summary>
    public interface IProcess
    {
        /// <summary>
        /// Unique ID of this process instance.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Controller for the component.
        /// </summary>
        /// <remarks>
        /// Process controller will be shared by all the views 
        /// that belong to this process.
        /// </remarks>
        object Controller { get; set; }

        /// <summary>
        /// Gets the name of the current view.
        /// </summary>
        string CurrentView { get; }

        /// <summary>
        /// Gets the the flag that indicates if selected view 
        /// has changed during the current request.
        /// </summary>
        bool ViewChanged { get; }
        
        /// <summary>
        /// Starts the process.
        /// </summary>
        /// <param name="referrerUrl">Referrer URL.</param>
        void Start(string referrerUrl);
        
        /// <summary>
        /// Resolves view for the specified view name.
        /// </summary>
        /// <param name="viewName">Name of the view to go to.</param>
        void SetView(string viewName);

        /// <summary>
        /// Ends the process.
        /// </summary>
        void End();
    }
}