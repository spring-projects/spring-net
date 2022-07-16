#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

namespace Spring.Transaction.Interceptor
{
	/// <summary>
	/// Tag class. Its class means it has the opposite behaviour to the
	/// <see cref="Spring.Transaction.Interceptor.RollbackRuleAttribute"/> superclass.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Griffin Caprio (.NET)</author>
	public class NoRollbackRuleAttribute : RollbackRuleAttribute
	{
		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Transaction.Interceptor.NameMatchTransactionAttributeSource"/> class.
		/// </summary>
		public NoRollbackRuleAttribute( string exceptionType ) : base( exceptionType ){}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Transaction.Interceptor.NameMatchTransactionAttributeSource"/> class.
		/// </summary>
		/// <param name="exceptionType">
		/// The <see cref="System.Exception"/> class that will trigger a rollback.
		/// </param>
		public NoRollbackRuleAttribute( Type exceptionType ) : base( exceptionType ){}
	}
}
