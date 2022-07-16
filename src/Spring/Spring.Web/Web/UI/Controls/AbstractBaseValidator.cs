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

#region Import

using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;

#endregion

namespace Spring.Web.UI.Controls
{
	/// <summary>
	/// Provides functions required for implementing validators
	/// but are unfortunately not accessible from <see cref="BaseValidator"/>
	/// </summary>
	/// <author>Erich Eichinger</author>
	public abstract class AbstractBaseValidator : BaseValidator
    {
		/// <summary>
		/// Registers a javascript-block to be rendered.
		/// </summary>
		protected void RegisterClientScriptBlock(Type type, string key, string script)
		{
                this.Page.ClientScript.RegisterClientScriptBlock(
                    typeof(RequiredCheckBoxValidator)
                    , "RequiredCheckBoxValidatorEvaluateIsChecked"
                    ,
@"
<script type=""text/javascript"">
function RequiredCheckBoxValidatorEvaluateIsChecked(val)
{
  var control;
  control = document.getElementById(val.controltovalidate);
  return control.checked;
}
</script>
"
                    );
		}

		/// <summary>
		/// Checks, if a certain javascript-block is already registered.
		/// </summary>
    	protected bool IsClientScriptBlockRegistered(Type type, string key)
    	{
			return this.Page.ClientScript.IsClientScriptBlockRegistered(type, key);
    	}

		/// <summary>
		/// Adds an attribute to be rendered for clientside validation.
		/// </summary>
        protected void AddExpandoAttribute(HtmlTextWriter writer, string controlId, string attributeName, string attributeValue, bool encode)
        {
            typeof(BaseValidator).InvokeMember("AddExpandoAttribute"
                                                ,
                                                BindingFlags.InvokeMethod | BindingFlags.NonPublic
                                                | BindingFlags.Instance
                                                , null
                                                , this
                                                , new object[] {writer, controlId, attributeName, attributeValue, encode}
                );
        }

		/// <summary>
		/// Is "XHTML 1.0 Transitional" rendering allowed?
		/// </summary>
        protected bool EnableLegacyRendering
        {
            get
            {
                return (bool) typeof(BaseValidator).GetProperty("EnableLegacyRendering",
                                                                                       BindingFlags.Instance
                                                                                       | BindingFlags.NonPublic).GetValue(this, null);
            }
        }
    }
}
