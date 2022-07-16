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

using System.Collections;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Spring.Web.UI
{
    /// <summary>
    /// Convinience implementation of the wizard-like page controller.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Wizard steps are encapsulated within custom user controls. Wizard
    /// controller takes care of navigation through steps and loading of the 
    /// appropriate user control.
    /// </p>
    /// <p>
    /// Developer implementing wizard needs to inherit from this class and implement
    /// abstract, read-only StepPanel property that will be used as a container
    /// for the wizard steps. Navigation event handlers should call Previous and Next methods
    /// from this class to change current step.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public abstract class AbstractWizard : Page
    {
        private IList steps;

        /// <summary>
        /// Gets or sets a list of user controls representing wizard steps.
        /// </summary>
        public IList Steps
        {
            get { return steps; }
            set { steps = value; }
        }

        #region Wizard navigation members

        /// <summary>
        /// Gets or sets current step using step index.
        /// </summary>
        public int CurrentStep
        {
            get
            {
                if (ViewState["__wizard.CurrentStep"] == null)
                {
                    ViewState["__wizard.CurrentStep"] = 0;
                }
                return (int) ViewState["__wizard.CurrentStep"];
            }
            set { ViewState["__wizard.CurrentStep"] = value; }
        }

        /// <summary>
        /// Returns true if there are no steps before the current step.
        /// </summary>
        public bool IsFirst
        {
            get { return CurrentStep == 0; }
        }

        /// <summary>
        /// Returns true if there are no steps after the current step.
        /// </summary>
        public bool IsLast
        {
            get { return CurrentStep == Steps.Count - 1; }
        }

        /// <summary>
        /// Moves to the previous step in the list, if one exists.
        /// </summary>
        public void Previous()
        {
            if (!IsFirst)
            {
                CurrentStep--;
            }
        }

        /// <summary>
        /// Moves to the next step in the list, if one exists.
        /// </summary>
        public void Next()
        {
            if (!IsLast)
            {
                CurrentStep++;
            }
        }

        #endregion

        #region Page lifecycle methods

        /// <summary>
        /// Initializes wizard steps.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnInit(EventArgs e)
        {
            InitializeSteps(steps);
            base.OnInit(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        /// <summary>
        /// Loads new step into a step panel if step has changed.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnPreRender(EventArgs e)
        {
            for (int i = 0; i < StepPanel.Controls.Count; i++)
            {
                StepPanel.Controls[i].Visible = (i == CurrentStep);
            }
            base.OnPreRender(e);
        }

        /// <summary>
        /// Initializes all the steps.
        /// </summary>
        /// <param name="stepNames">List of step control names.</param>
        /// <returns>List of step control instances.</returns>
        private void InitializeSteps(IList stepNames)
        {
            foreach (string stepName in stepNames)
            {
                Control step = LoadStep(stepName);
                StepPanel.Controls.Add(step);
            }
        }

        /// <summary>
        /// Loads step control.
        /// </summary>
        private Control LoadStep(string stepControlName)
        {
            Control step = LoadControl(stepControlName);

            int start = stepControlName.LastIndexOf('/');
            int end = stepControlName.LastIndexOf('.');
            step.ID = stepControlName.Substring(start + 1, end - start - 1);

            return step;
        }

        #endregion

        #region Abstract members

        /// <summary>
        /// Panel that should serve as a container for wizard steps.
        /// </summary>
        protected abstract Panel StepPanel { get; }

        #endregion
    }
}
