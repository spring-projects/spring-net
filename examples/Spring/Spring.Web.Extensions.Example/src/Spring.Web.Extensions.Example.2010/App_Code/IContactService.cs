#region License

/*
 * Copyright © 2002-2006 the original author or authors.
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

/// <summary>
/// Service interface for contact management.
/// </summary>
/// <author>Bruno Baia</author>
/// <version>$Id: IContactService.cs,v 1.1 2007/05/31 18:34:54 markpollack Exp $</version>
public interface IContactService
{
    /// <summary>
    /// Returns an array of emails, with a maximum of <paramref name="count"/> 
    /// values, that complete the given <paramref name="prefixText"/>.
    /// </summary>
    /// <remarks>
    /// This signature method is required 
    /// to apply the auto-completion behavior.
    /// </remarks>
    /// <param name="prefixText">Text entered by the user.</param>
    /// <param name="count">Maximum number of possible values.</param>
    /// <returns>The array of emails for completing the text.</returns>
    string[] GetEmails(string prefixText, int count);
}
