using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Spring.Web.UI.Controls
{
    /// <summary>
    /// May be used to wrap controls for databinding that don't accept unknown attributes.
    /// </summary>
    [ParseChildren(false)]
    public class DataBindingAdapter : WebControl
    {
        private Control wrappedControl;

        /// <summary>
        /// Overridden to ensure only 1 control is wrapped by this adapter
        /// </summary>
        /// <param name="obj"></param>
        protected override void AddParsedSubObject(object obj)
        {
            if(obj is Control && (wrappedControl == null) && (!(obj is LiteralControl)))
            {
                wrappedControl = (Control) obj;
                this.Controls.Add(wrappedControl);
            }
            else if(!(obj is LiteralControl))
            {
                throw new HttpException(
                    string.Format("DataBindingAdapter can only have 1 non-literal child", new object[] { obj.GetType().Name }));
            }
        }

		/// <summary>
		/// Returns the control wrapped by this adapter or null.
		/// </summary>
        public Control WrappedControl
        {
            get { return this.wrappedControl; }
        }

		/// <summary>
		/// Overridden to render wrapped control only.
		/// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            if (wrappedControl != null)
            {
                wrappedControl.RenderControl(writer);
            }
        }
    }
}