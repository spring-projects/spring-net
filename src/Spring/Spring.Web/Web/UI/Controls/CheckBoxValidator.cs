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

using System.Web.UI;
using System.Web.UI.WebControls;

#endregion

namespace Spring.Web.UI.Controls
{
	/// <summary>
	/// This validator allows for validating a CheckBox's "Checked" state.
	/// </summary>
	/// <author>Erich Eichinger</author>
	[ToolboxData("<{0}:RequiredCheckBoxValidator runat=\"server\" ErrorMessage=\"RequiredCheckBoxValidator\"></{0}:RequiredCheckBoxValidator>")]
    public class RequiredCheckBoxValidator : AbstractBaseValidator
    {
		/// <summary>
		/// Validates this validator's properties are set correctly.
		/// </summary>
		/// <returns></returns>
        protected override bool ControlPropertiesValid()
        {
            string controlName = this.ControlToValidate;
            if(controlName.Length == 0)
            {
                return base.ControlPropertiesValid();
            }
            return true;
        }

		/// <summary>
		/// Returns the state of the checkbox's Checked property
		/// </summary>
		/// <returns></returns>
        protected override bool EvaluateIsValid()
        {
            CheckBox controlToValidate = this.NamingContainer.FindControl(this.ControlToValidate) as CheckBox;
            if(controlToValidate == null)
            {
                return false;
            }

            return controlToValidate.Checked;
        }

		/// <summary>
		/// Adds attributes required for clientside validation
		/// </summary>
		/// <param name="writer"></param>
        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            base.AddAttributesToRender(writer);
            if(base.RenderUplevel)
            {
                string controlId = this.ClientID;
                writer = base.EnableLegacyRendering ? writer : null;
                base.AddExpandoAttribute(writer, controlId, "evaluationfunction", "RequiredCheckBoxValidatorEvaluateIsChecked", false);
                base.AddExpandoAttribute(writer, controlId, "initialvalue", "", true);
            }
        }

		/// <summary>
		/// Ensures the evaluation javascript functionblock is registered
		/// </summary>
		/// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (!base.IsClientScriptBlockRegistered(typeof(RequiredCheckBoxValidator), "RequiredCheckBoxValidatorEvaluateIsChecked"))
            {
                base.RegisterClientScriptBlock(
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
        }
    }
}
