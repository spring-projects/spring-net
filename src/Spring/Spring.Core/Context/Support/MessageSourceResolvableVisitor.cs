#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

#region Imports

using System.Text;
using Spring.Util;

#endregion

namespace Spring.Context.Support
{
	/// <summary>
	/// Visitor class to represent
	/// <see cref="Spring.Context.IMessageSourceResolvable"/> instances.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Used in the first instance to supply stringified versions of
	/// <see cref="Spring.Context.IMessageSourceResolvable"/> instances. 
	/// </p>
	/// <p>
	/// Other methods can be added here to return different representations,
	/// including XML, CSV, etc..
	/// </p>
	/// </remarks>
	/// <author>Griffin Caprio (.NET)</author>
	public class MessageSourceResolvableVisitor
	{
		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Context.Support.MessageSourceResolvableVisitor"/> class.
		/// </summary>
		public MessageSourceResolvableVisitor()
		{
		}

		/// <summary>
		/// Outputs the supplied <see cref="Spring.Context.IMessageSourceResolvable"/>
		/// as a nicely formatted <see cref="System.String"/>.
		/// </summary>
		/// <param name="resolvable">
		/// The <see cref="Spring.Context.IMessageSourceResolvable"/> to output.
		/// </param>
		public string VisitMessageSourceResolvableString(
			IMessageSourceResolvable resolvable)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("codes=[");
			builder.Append(StringUtils.ArrayToDelimitedString(resolvable.GetCodes(), ","));
			builder.Append("]; arguments=[");
			if (resolvable.GetArguments() == null)
			{
				builder.Append("null");
			}
			else
			{
				object[] arguments = resolvable.GetArguments();
				for (int i = 0; i < arguments.Length; i++)
				{
					builder.Append("(");
					builder.Append(arguments[i].GetType().Name);
					builder.Append(")[");
					builder.Append(arguments[i]);
					builder.Append("]");
					if (i < arguments.Length - 1)
					{
						builder.Append(", ");
					}
				}
			}
			builder.Append("]; defaultMessage=[");
			builder.Append(resolvable.DefaultMessage);
			builder.Append("]");
			return builder.ToString();
		}
	}
}