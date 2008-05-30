#region License

/*
 * Copyright 2002-2004 the original author or authors.
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

using System;
using System.ComponentModel;

namespace Spring.Transaction.Interceptor
{
	/// <summary>
	/// This interface adds a
	/// <see cref="Spring.Transaction.Interceptor.ITransactionAttribute.RollbackOn(Exception)"/>
	/// specification to <see cref="Spring.Transaction.ITransactionDefinition"/>.
	/// </summary>
	/// <author>Griffin Caprio (.NET)</author>
	/// <version>$Id: ITransactionAttribute.cs,v 1.6 2007/12/29 00:28:53 markpollack Exp $</version>
	[TypeConverter(typeof(TransactionAttributeConverter))]
	public interface ITransactionAttribute : ITransactionDefinition
	{
		/// <summary>
		/// Decides if rollback is required for the supplied <paramref name="exception"/>.
		/// </summary>
		/// <param name="exception">The <see cref="System.Exception"/> to evaluate.</param>
		/// <returns>True if the exception causes a rollback, false otherwise.</returns>
		bool RollbackOn( Exception exception );
	}
}
