
using System.Web.UI;
using UserControl=Spring.Web.UI.UserControl;

namespace Spring.TestSupport
{
    public class TestUserControl : UserControl
    {
        private static int instanceCount = 0;

        public TestUserControl()
            :this(string.Format("_ctl_{0}", instanceCount++), null)
        {
        }

        public TestUserControl(string id)
            :this(id, null)
        {
        }

        public TestUserControl(Control parent)
            :this(null, parent)
        {
        }

        public TestUserControl(string id, Control parent)
        {
            this.ID = id;
            if (parent != null) parent.Controls.Add(this);
        }

        public new void SetResult(string name)
        {
            base.SetResult(name);
        }

        public override string ToString()
        {
            return string.Format("{0}[{1}]", base.ToString(), this.ClientID);
        }

        public new void SaveModelToPersistenceMedium(object model)
        {
            base.SaveModelToPersistenceMedium(model);
        }

        public new object LoadModelFromPersistenceMedium()
        {
            return base.LoadModelFromPersistenceMedium();
        }
    }
}