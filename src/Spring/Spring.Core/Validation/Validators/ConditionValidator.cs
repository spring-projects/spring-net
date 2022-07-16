#region License

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

#endregion

using Spring.Expressions;
using Spring.Util;

namespace Spring.Validation
{
	/// <summary>
	/// Evaluates validator test using condition evaluator.
	/// </summary>
	/// <author>Aleksandar Seovic</author>
	public class ConditionValidator : BaseSimpleValidator
	{
        #region Constructors

	    /// <summary>
	    /// Creates a new instance of the <see cref="ConditionValidator"/> class.
	    /// </summary>
	    public ConditionValidator()
	    {}

	    /// <summary>
	    /// Creates a new instance of the <see cref="ConditionValidator"/> class.
	    /// </summary>
	    /// <param name="test">The expression to validate.</param>
	    /// <param name="when">The expression that determines if this validator should be evaluated.</param>
	    public ConditionValidator(string test, string when) : base(test, when)
	    {
	        AssertUtils.ArgumentHasText(test, "test");
	    }

	    /// <summary>
	    /// Creates a new instance of the <see cref="ConditionValidator"/> class.
	    /// </summary>
	    /// <param name="test">The expression to validate.</param>
	    /// <param name="when">The expression that determines if this validator should be evaluated.</param>
	    public ConditionValidator(IExpression test, IExpression when) : base(test, when)
	    {
	        AssertUtils.ArgumentNotNull(test, "test");
	    }

	    #endregion

	    /// <summary>
		/// Evaluates the test using condition evaluator.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Test can be any logical expression that is supported by the Spring.NET logical
		/// expression evaluation engine, and can use any variables that can be resolved
		/// by the variable resolver used by the validation engine.
		/// </p>
		/// </remarks>
		/// <param name="objectToValidate">The object to validate.</param>
		/// <returns>
		/// <see lang="true"/> if the supplied <paramref name="objectToValidate"/> is valid.
		/// </returns>
		protected override bool Validate(object objectToValidate)
		{
			return Convert.ToBoolean(objectToValidate);
		}
	}
}
