using Spring.Globalization;

namespace Spring.DataBinding
{
    /// <summary>
    /// An interface that has to be implemented by all data binding containers.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public interface IBindingContainer : IBinding
    {
        /// <summary>
        /// Gets a value indicating whether this data binding container 
        /// has bindings.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this data binding container has bindings; 
        ///     <c>false</c> otherwise.
        /// </value>
        bool HasBindings { get; }

        /// <summary>
        /// Adds the binding.
        /// </summary>
        /// <param name="binding">
        /// Binding definition to add.
        /// </param>
        /// <returns>
        /// Added <see cref="IBinding"/> instance.
        /// </returns>
        IBinding AddBinding(IBinding binding);

        /// <summary>
        /// Adds the <see cref="SimpleExpressionBinding"/> binding with a default 
        /// binding direction of <see cref="BindingDirection.Bidirectional"/>.
        /// </summary>
        /// <remarks>
        /// This is a convinience method for adding <b>SimpleExpressionBinding</b>,
        /// one of the most often used binding types, to the bindings list.
        /// </remarks>
        /// <param name="sourceExpression">
        /// The source expression.
        /// </param>
        /// <param name="targetExpression">
        /// The target expression.
        /// </param>
        /// <returns>
        /// Added <see cref="SimpleExpressionBinding"/> instance.
        /// </returns>
        IBinding AddBinding(string sourceExpression, string targetExpression);

        /// <summary>
        /// Adds the <see cref="SimpleExpressionBinding"/> binding.
        /// </summary>
        /// <remarks>
        /// This is a convinience method for adding <b>SimpleExpressionBinding</b>,
        /// one of the most often used binding types, to the bindings list.
        /// </remarks>
        /// <param name="sourceExpression">
        /// The source expression.
        /// </param>
        /// <param name="targetExpression">
        /// The target expression.
        /// </param>
        /// <param name="direction">
        /// Binding direction.
        /// </param>
        /// <returns>
        /// Added <see cref="SimpleExpressionBinding"/> instance.
        /// </returns>
        IBinding AddBinding(string sourceExpression, string targetExpression, BindingDirection direction);

        /// <summary>
        /// Adds the <see cref="SimpleExpressionBinding"/> binding with a default 
        /// binding direction of <see cref="BindingDirection.Bidirectional"/>.
        /// </summary>
        /// <remarks>
        /// This is a convinience method for adding <b>SimpleExpressionBinding</b>,
        /// one of the most often used binding types, to the bindings list.
        /// </remarks>
        /// <param name="sourceExpression">
        /// The source expression.
        /// </param>
        /// <param name="targetExpression">
        /// The target expression.
        /// </param>
        /// <param name="formatter">
        /// <see cref="IFormatter"/> to use for value formatting and parsing.
        /// </param>
        /// <returns>
        /// Added <see cref="SimpleExpressionBinding"/> instance.
        /// </returns>
        IBinding AddBinding(string sourceExpression, string targetExpression, IFormatter formatter);

        /// <summary>
        /// Adds the <see cref="SimpleExpressionBinding"/> binding.
        /// </summary>
        /// <remarks>
        /// This is a convinience method for adding <b>SimpleExpressionBinding</b>,
        /// one of the most often used binding types, to the bindings list.
        /// </remarks>
        /// <param name="sourceExpression">
        /// The source expression.
        /// </param>
        /// <param name="targetExpression">
        /// The target expression.
        /// </param>
        /// <param name="direction">
        /// Binding direction.
        /// </param>
        /// <param name="formatter">
        /// <see cref="IFormatter"/> to use for value formatting and parsing.
        /// </param>
        /// <returns>
        /// Added <see cref="SimpleExpressionBinding"/> instance.
        /// </returns>
        IBinding AddBinding(string sourceExpression, string targetExpression, BindingDirection direction,
                            IFormatter formatter);
    }
}