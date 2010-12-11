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

#region Imports

using System;

#endregion

namespace Spring.Web.UI.Controls
{
#if !NET_2_0
	/// <summary>
	/// Enumeration representing a control's current initialization state.
	/// </summary>
	/// <author>Erich Eichinger</author>
	internal enum ControlState
	{
		Constructed,
		ChildrenInitialized,
		Initialized,
		ViewStateLoaded,
		Loaded,
		PreRendered
	}
#endif //!NET_2_0
}
