#region License

/*
 * Copyright 2002-2006 the original author or authors.
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

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

#endregion

namespace Spring.IocQuickStart.AppContext
{
	/// <summary>
	/// Summary description for ShowImageForm.
	/// </summary>
	public class ResourcesDisplayForm : Form
	{
        private PictureBox pictureBox1;
        private RichTextBox richTextBox1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private Container components = null;

        /// <summary>
        /// Property Image (Image)
        /// </summary>
        public Image Image
        {
            set
            {
                this.pictureBox1.Image = value;
            }
        }

        public void AppendText(string text)
        {
            richTextBox1.AppendText(text);
        }
        
		public ResourcesDisplayForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
       

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(208, 328);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(146, 119);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.richTextBox1.Location = new System.Drawing.Point(16, 16);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(536, 288);
            this.richTextBox1.TabIndex = 1;
            this.richTextBox1.Text = "";
            // 
            // ResourcesDisplayForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(568, 470);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.pictureBox1);
            this.Name = "ResourcesDisplayForm";
            this.Text = "ResourcesDisplayForm";
            this.ResumeLayout(false);

        }
		#endregion
	}
}
