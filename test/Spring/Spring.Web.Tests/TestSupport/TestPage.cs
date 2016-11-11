using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.UI;
using Spring.Collections;
using Page=Spring.Web.UI.Page;

namespace Spring.TestSupport
{
    public class TestPage : Page
    {
        public TestPage()
        {
            this.SharedState = new CaseInsensitiveHashtable(); //CollectionsUtil.CreateCaseInsensitiveHashtable();
        }

        public TestPage( HttpContext context ) 
            :this()
        {
            SetIntrinsics(context);
        }

        public virtual void SetIntrinsics(HttpContext context)
        {
            MethodInfo miSetIntrinsics = typeof(System.Web.UI.Page).GetMethod("SetIntrinsics",BindingFlags.Instance|BindingFlags.NonPublic, null, new Type[] { typeof(HttpContext) }, null);
            miSetIntrinsics.Invoke(this, new object[] {context});
        }

        public virtual void InitRecursive( Control namingContainer )
        {
            MethodInfo miInitRecursive = typeof(System.Web.UI.Control).GetMethod("InitRecursive", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(Control) }, null);
            miInitRecursive.Invoke(this, new object[] {null});
        }

        public new virtual void InitializeCulture()
        {
            base.InitializeCulture();
        }

        public override System.Web.SessionState.HttpSessionState Session
        {
            get
            {
                return null;
            }
        }
//        protected override IDictionary CreateValidatorParameters()
//        {
//            return null;
//        }

        public new void SetResult(string resultName)
        {
            base.SetResult(resultName);
        }

        public string Render(string newLine)
        {
            StringWriter sw = new StringWriter();
            HtmlTextWriter writer = new HtmlTextWriter(sw, "");
            writer.NewLine = newLine;
            base.Render(writer);
            writer.Flush();
            writer.Close();
            string result = sw.GetStringBuilder().ToString();
            return result;
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
