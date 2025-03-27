namespace Spring.Data.Objects.Factory.Support
{
    public class TestForm : Spring.Web.UI.Page
    {
        private ILog _log = LogManager.GetLogger(typeof(TestForm));

        public TestForm()
        {
            this.Load += new EventHandler(Page_Load);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _log.Debug("loaded page!");
        }
    }
}
