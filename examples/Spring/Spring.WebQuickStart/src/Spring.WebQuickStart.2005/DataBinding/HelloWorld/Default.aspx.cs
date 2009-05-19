using System;
using Spring.DataBinding;

/// <summary>
/// Notice that your web page has to extend Spring.Web.UI.Page class
/// in order to enable data binding and many other features.
/// </summary>
public partial class DataBinding_HelloWorld_Default : Spring.Web.UI.Page
{
    private string name;

    public string Name
    {
        get { return name; }
        set { name = value; }
    }

    /// <summary>
    /// In order to declare data bindings, all you need to do is
    /// override InitializeDataBindings method and add all necessary
    /// data bindings to the BindingManager.
    /// </summary>
    protected override void InitializeDataBindings()
    {
        BindingManager.AddBinding("FindControl('txtName').Text", "Name");
        BindingManager.AddBinding("FindControl('lblName').Text", "Name", BindingDirection.TargetToSource);
    }
    
    protected void Page_Load(object sender, EventArgs e)
    {
        divGreeting.Visible = !string.IsNullOrEmpty(Name);
    }
}
