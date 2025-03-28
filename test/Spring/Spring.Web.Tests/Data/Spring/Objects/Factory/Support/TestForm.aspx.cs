using Microsoft.Extensions.Logging;

namespace Spring.Data.Objects.Factory.Support;

public class TestForm : Spring.Web.UI.Page
{
    private ILogger<TestForm> _log = LogManager.GetLogger<TestForm>();

    public TestForm()
    {
        this.Load += new EventHandler(Page_Load);
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        _log.LogDebug("loaded page!");
    }
}
