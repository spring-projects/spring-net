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

using System.Collections;

namespace Spring.Util
{
	/// <summary>
	/// An implementation of the Java Properties class.
	/// </summary>
	/// <remarks>
    /// For the complete syntax see <a href="http://java.sun.com/j2se/1.4.2/docs/api/java/util/Properties.html#load(java.io.InputStream)">java.util.Properties JavaDoc</a>.
    /// This class supports an extended syntax. There may also be sole keys on a line, in that case values are treated as <c>null</c>.
    /// <example>
    /// <code>
    /// key1 = value
    /// key2:
    /// key3
    /// </code>
    /// will result in the name/value pairs:
    /// <list>
    /// <item>key1:="value"</item>
    /// <item>key2:=string.Empty</item>
    /// <item>key3:=&lt;null&gt;</item>
    /// </list>
    /// note, that to specify a <c>null</c> value, the key <b>must not</b> be followed by any character except newline.
    /// </example>
	/// </remarks>
	/// <author>Simon White</author>
    [Serializable]
    public class Properties : Hashtable
	{
		#region Constants

		private const string Comments = "#!";
		private const string Separators = ":=";
		private const string Whitespace = " \t\r\n";
		private const string WhitespaceWithSeparators = Whitespace + Separators;

		#endregion

		#region Constructor (s) / Destructor

		/// <summary>
		/// Creates an empty property list with no default values.
		/// </summary>
		public Properties()
		{
		}

		/// <summary>
		/// Creates a property list with the specified initial properties.
		/// </summary>
		/// <param name="p">The initial properties.</param>
		public Properties(Properties p) : base(p)
		{
		}

		#endregion

		#region Methods

		/// <summary>
		/// Reads a property list (key and element pairs) from the input stream.
		/// </summary>
		/// <param name="stream">The stream to load from.</param>
		public void Load(Stream stream)
		{
			Load(this, stream);
		}

		/// <summary>
		/// Reads a property list (key and element pairs) from a text reader.
		/// </summary>
		/// <param name="textReader">The text reader to load from.</param>
		public void Load(TextReader textReader)
		{
			Load(this, textReader);
		}

		/// <summary>
		/// Reads a property list (key and element pairs) from the input stream.
		/// </summary>
		/// <param name="dictionary">the dictionary to put it in</param>
		/// <param name="stream">The stream to load from.</param>
		public static void Load(IDictionary dictionary, Stream stream)
		{
			using (StreamReader streamReader = new StreamReader(stream))
			{
				Load(dictionary, streamReader);
			}
		}

		/// <summary>
		/// Reads a property list (key and element pairs) from a text reader.
		/// </summary>
		/// <param name="dictionary">the dictionary to put it in</param>
		/// <param name="textReader">The text reader to load from.</param>
		public static void Load(IDictionary dictionary, TextReader textReader)
		{
			bool isContinuation = false;
			string key = null;
			string value = null;
			string line = null;
			while ((line = textReader.ReadLine()) != null)
			{
				line = RemoveLeadingWhitespace(line);
				if (line != null && line.Length>0 && Comments.IndexOf(line[0]) == -1)
				{
					if (!isContinuation)
					{
						string[] keyvalue = SplitLine(line);
						if (keyvalue == null)
						{
							continue;
						}
						key = keyvalue[0];
						value = keyvalue[1];

						if (value != null && value.EndsWith("\\"))
						{
							value = value.Substring(0, value.Length - 1);
							isContinuation = true;
						}
						else
						{
							dictionary[key] = StringUtils.ConvertEscapedCharacters(value);
						}
					}
					else
					{
						if (line.EndsWith("\\"))
						{
							value += line.Substring(0, line.Length - 1);
						}
						else
						{
							value += line;
							isContinuation = false;
							dictionary[key] = StringUtils.ConvertEscapedCharacters(value);
						}
					}
				}
			}
		}

		/// <summary>
		/// Strips whitespace from the front of the specified string.
		/// </summary>
		/// <param name="line">The string.</param>
		/// <returns>The string with all leading whitespace removed.</returns>
		private static string RemoveLeadingWhitespace(string line)
		{
            if (line == null) return null;

			string trimmed = string.Empty;
			for (int i = 0; i < line.Length; i++)
			{
				if (Whitespace.IndexOf(line[i]) == -1)
				{
					trimmed = line.Substring(i);
					break;
				}
			}
			return trimmed;
		}

		/// <summary>
		/// Splits the specified string into a key / value pair.
		/// </summary>
		/// <param name="line">The line to split.</param>
		/// <returns>An array containing the key / value pair.</returns>
		private static string[] SplitLine(string line)
		{
			string key = line;
			string value = null;

			int index = 0;
			int len = line.Length;
			for (; index < len; index++)
			{
				if (WhitespaceWithSeparators.IndexOf(line[index]) != -1)
				{
					if (line[index - 1].Equals('\\'))
					{
						line = line.Remove(index - 1, 1);
						len--;
						index--;
					}
					else
					{
						key = line.Substring(0, index);
						break;
					}
				}
			}

			// got key, now find the start of the value
			// first ignore leading whitespace and initial separator
			// (if one's there)
			index++;
			if (index > len)
			{
                // this is an extension to support key-only lines and specifying null values
			    value = null;
			}
            else if (index == len)
            {
                value = string.Empty;
            }
            else
            {
                value = line.Substring(index);
                value = RemoveLeadingWhitespace(value);
            }

		    if (value != null && value.Length > 0
                && Separators.IndexOf(value[0]) != -1)
			{
				value = value.Substring(1);
				value = RemoveLeadingWhitespace(value);
			}

			return new string[] {key, value};
		}

		/// <summary>
		/// Searches for the property with the specified key in this property list.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>The property, or null if the key was not found.</returns>
		public string GetProperty(string key)
		{
			return this[key] as string;
		}

		/// <summary>
		/// Searches for the property with the specified key in this property list.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="def">
		/// The default value to be returned if the key is not found.
		/// </param>
		/// <returns>The property, or the default value.</returns>
		public string GetProperty(string key, string def)
		{
			string val = this[key] as string;
			return (val != null ? val : def);
		}

		/// <summary>
		/// Writes this property list out to the specified stream.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		public void List(Stream stream)
		{
			using (StreamWriter sw = new StreamWriter(stream))
			{
				foreach (DictionaryEntry de in this)
				{
					sw.WriteLine(de.Key + "=" + de.Value);
				}
			}
		}

		/// <summary>
		/// Sets the specified property key / value pair.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="theValue">The value.</param>
		public void SetProperty(string key, string theValue)
		{
			this[key] = theValue;
		}

		/// <summary>
		/// Writes the properties in this instance out to the supplied stream.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		/// <param name="header">Arbitrary header information.</param>
		public void Store(Stream stream, string header)
		{
			using (StreamWriter sw = new StreamWriter(stream))
			{
				sw.WriteLine(header);

				foreach (DictionaryEntry de in this)
				{
					sw.WriteLine(de.Key + "=" + de.Value);
				}
			}
		}

		#endregion

		#region IDictionary Members

		/// <summary>
		/// Adds the specified key / object pair to this collection.
		/// </summary>
		public override object this[object key]
		{
			get { return base[key as string]; }
			set { base[key as string] = value as string; }
		}

		/// <summary>
		/// Removes the key / value pair identified by the supplied key.
		/// </summary>
		/// <param name="key">
		/// The key identifying the key / value pair to be removed.
		/// </param>
		public override void Remove(object key)
		{
			base.Remove(key as string);
		}

		/// <summary>
		/// Adds the specified key / object pair to this collection.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		public override void Add(object key, object value)
		{
			base.Add(key as string, value as string);
		}

		#endregion
	}
}
