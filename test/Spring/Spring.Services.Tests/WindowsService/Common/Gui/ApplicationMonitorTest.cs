using System;
using System.Collections;
using System.Drawing.Imaging;
using System.Threading;

using DotNetMock.Dynamic;

using NUnit.Framework;
using Spring.Objects.Factory.Config;
using Spring.Threading;

namespace Spring.Services.WindowsService.Common.Gui
{
    class MockApplication : IApplication
    {
        static int count = 0;

        public static void ResetCount()
        {
            count = 0;
        }

        int c;

        public MockApplication()
        {
            c = count++;
        }

        public string Name
        {
            get
            {
                return "Mock" + c;
            }
        }

        public AppDomainSetup DomainSetup
        {
            get { throw new NotImplementedException(); }
        }

        public string ApplicationBase
        {
            get { throw new NotImplementedException(); }
        }

        public string FullPath
        {
            get { throw new NotImplementedException(); }
        }

        public string ServiceXmlFullPath
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsValid
        {
            get { throw new NotImplementedException(); }
        }

        public string WatcherXmlFullPath
        {
            get { throw new NotImplementedException(); }
        }

        public IConfigurableListableObjectFactory ObjectFactory
        {
            get { throw new NotImplementedException(); }
        }

        public IObjectFactoryPostProcessor ObjectFactoryPostProcessor
        {
            get { throw new NotImplementedException(); }
        }

        public string DefaultPrivateBinPath
        {
            get { throw new NotImplementedException(); }
        }
    }

    class MockService : ISpringService, ISpringServiceFactory
    {
        /// <summary>
        /// Detailed informations about the deployed applications
        /// </summary>
        public DeployInformation[] DeployInformations
        {
            get
            {
                return new DeployInformation[]
                    {
                        new DeployInformation(new MockApplication(), DeployStatus.Deployed, null),
                        new DeployInformation(new MockApplication(), DeployStatus.CannotDeploy, new Exception("fatal")),
                    };
            }
        }

        /// <summary>
        /// Obtain a reference (possibly remote) to the spring service
        /// </summary>
        public ISpringService SpringService
        {
            get
            {
                return new MockService();
            }
        }

        public string ServiceUrl
        {
            get { return "mock://mock.rem"; }
            set {  }
        }

    }

    class MyListBoxTester : ListBoxTester
    {
        public MyListBoxTester(string name, Form form) : base(name, form)
        {
        }

        public void SelectByStringExact (string what)
        {
            int index = Properties.FindStringExact(what);
            if (index != -1)
            {
                Select(index);
            }            
            else
            {
                Assert.Fail("listbox " + name + ": text " + what + " not found ");
            }
        }

    }

    [TestFixture]
	public class ApplicationMonitorTest
	{
        Form form;
        ISync started;
        LabelTester appStatus;
        MyListBoxTester boxTester;
        ApplicationMonitor monitor;
        LabelTester serviceStatus;

        [SetUp]
        public void SetUp ()
        {
            MockApplication.ResetCount();
            started = new Latch();
            new Thread(new ThreadStart(Go)).Start();
            started.Acquire();
            boxTester = new MyListBoxTester("applicationList", form);
            appStatus = new LabelTester("applicationStatus", form);
            serviceStatus = new LabelTester("serviceStatus", form);
        }

        [TearDown]
        public void TearDown ()
        {
            form.Close();
        }

        [Test, Explicit]
        public void Interactive ()
        {
            Go();
        }

        [Test]
		public void ListsEachApplication()
		{
            Assert.AreEqual(2, boxTester.Properties.Items.Count);                
		}

        [Test]
        public void AListBoxShowsApplicationNames ()
        {
            boxTester.SelectByStringExact("Mock1");
            Assert.AreEqual("Mock1", boxTester.Properties.Text);                            
        }

        [Test]
        public void ShowsApplicationStatus()
        {
            boxTester.Select(0);
            IApplication application = boxTester.Properties.SelectedItem as IApplication;
            Assert.AreEqual(application.Name + ": deployed", appStatus.Text);
            Assert.AreEqual("the service seems to run normally, 2 application(s) found", serviceStatus.Text);            
            ScreenShot ("application deployed status.gif");
    
            boxTester.Select(1);
            application = boxTester.Properties.SelectedItem as IApplication;
            Assert.AreEqual(application.Name + ": cannot deploy: fatal", appStatus.Text);
            ScreenShot ("application deploy error status.gif");
        }

        [Test]
        public void Updating ()
        {
            ButtonTester buttonTester = new ButtonTester("update", form);
            monitor.ServiceFactory = new StoleScreenShotWhileUpdating(this);
            buttonTester.Click();
        }

        private class StoleScreenShotWhileUpdating : ISpringServiceFactory
        {
            ApplicationMonitorTest test;

            public StoleScreenShotWhileUpdating (ApplicationMonitorTest test)
            {
                this.test = test;
            }

            public ISpringService SpringService
            {
                get
                {
                    Assert.AreEqual("updating ...", test.serviceStatus.Text);
                    test.ScreenShot("updating.gif");
                    return null;
                }
            }

            /// <summary>
            /// The .NET remoting url for the remote service
            /// </summary>
            public string ServiceUrl
            {
                get { return ""; }
                set {  }
            }

        }

        [Test]
        public void TheServiceFactoryCanBeSetToNull ()
        {
            monitor.ServiceFactory = null;
            Assert.AreEqual(String.Empty, appStatus.Text);
            Assert.AreEqual("service not available", serviceStatus.Text);
        }

        [Test]
        public void ErrorsWhileGettingTheReferenceToTheServiceAreVisibleToTheUser ()
        {
            DynamicMock mockFactory = new DynamicMock(typeof(ISpringServiceFactory));
            DynamicMock mockService = new DynamicMock(typeof(ISpringService));

            mockService.ExpectAndThrow("DeployInformations", new Exception("this and that"));
            mockFactory.ExpectAndReturn("SpringService", mockService.Object);

            monitor.ServiceFactory = mockFactory.Object as ISpringServiceFactory;
            Assert.AreEqual("service not available: this and that", serviceStatus.Text);
            Assert.AreEqual(String.Empty, appStatus.Text);
            ScreenShot ("service not available.gif");
        }

        private void ScreenShot (string name)
        {
            monitor.Refresh();
            AgileDocs.Core.ScreenShot.TakeScreenShot(
                monitor.Handle.ToInt32(), ImageFormat.Png, name);
        }

        ListView view;

        [Test, Explicit]
        public void ListView ()
        {
            view = new ListView();
            view.Dock = DockStyle.Fill;
            view.FullRowSelect = true;
            view.MultiSelect = false;
            view.HeaderStyle = ColumnHeaderStyle.Clickable;
            view.GridLines = true;
            view.View = View.Details;
            view.Sorting = SortOrder.Ascending;

            view.Columns.Add("Name", -1, HorizontalAlignment.Left);
            view.Columns.Add("Status", -1, HorizontalAlignment.Left);

            view.Items.Add(new ListViewItem(new string[] {"Mock1", "Deployed"}));
            view.Items.Add(new ListViewItem(new string[] {"Mock2", "Error"}));

            view.ColumnClick += new ColumnClickEventHandler(ColumnClick);

            view.SelectedIndexChanged +=new EventHandler(view_SelectedIndexChanged);

            view.MouseDown += new MouseEventHandler(view_MouseDown);

            form = new Form();
            form.Controls.Add(view);
            System.Windows.Forms.Application.Run(form);
        }

        // Implements the manual sorting of items by columns.
        class ListViewItemComparer : IComparer 
        {
            private int col;

            public ListViewItemComparer() 
            {
                col=0;
            }
            public ListViewItemComparer(int column) 
            {
                col=column;
            }
            public int Compare(object x, object y) 
            {
                ListViewItem a = x as ListViewItem;
                ListViewItem b = y as ListViewItem;
                return String.Compare(a.SubItems[col].Text, b.SubItems[col].Text);
                //return String.Compare(a.Text, b.Text);
            }
        }

        private void ColumnClick(object o, ColumnClickEventArgs e)
        {            
            // Set the ListViewItemSorter property to a new ListViewItemComparer object.
            view.ListViewItemSorter = new ListViewItemComparer(e.Column);
            // Call the sort method to manually sort the column based on the ListViewItemComparer implementation.
            view.Sort();
        }

        private void view_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView v = (ListView) sender;
            if (v.SelectedIndices.Count > 0)
            {
                int index = v.SelectedIndices[0];
                ListViewItem item = v.Items[index];
                form.Text = "item: " + item.SubItems[0].Text;
            }
        }

        [Test, Explicit]
        public void CreateMySplitControls()
        {
            // Create TreeView, ListView, and Splitter controls.
            TreeView treeView1 = new TreeView();
            ListView listView1 = new ListView();
            Splitter splitter1 = new Splitter();

            // Set the TreeView control to dock to the left side of the form.
            treeView1.Dock = DockStyle.Left;
            // Set the Splitter to dock to the left side of the TreeView control.
            splitter1.Dock = DockStyle.Left;
            // Set the minimum size the ListView control can be sized to.
            splitter1.MinExtra = 100;
            // Set the minimum size the TreeView control can be sized to.
            splitter1.MinSize = 75;
            // Set the ListView control to fill the remaining space on the form.
            listView1.Dock = DockStyle.Fill;
            // Add a TreeView and a ListView item to identify the controls on the form.
            treeView1.Nodes.Add("TreeView Node");
            listView1.Items.Add("ListView Item");

            // Add the controls in reverse order to the form to ensure proper location.
            form = new Form();
            form.Controls.AddRange(new Control[]{listView1, splitter1, treeView1});
            System.Windows.Forms.Application.Run(form);
        }

        private void Go()
        {
            form = new Form();
            form.Load += new EventHandler(form_Load);
            monitor = new ApplicationMonitor();
            monitor.ServiceFactory = new MockService();
            monitor.Dock = DockStyle.Fill;
            form.Controls.Add(monitor);
            System.Windows.Forms.Application.Run(form);
        }

        private void form_Load(object sender, EventArgs e)
        {
            started.Release();
        }

        ContextMenu menu = null;
        private void view_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ListView v = sender as ListView;
                v.ContextMenu = null;
                ListViewItem itemAt = v.GetItemAt(e.X,  e.Y);
                if (itemAt == null)
                {
                    return;
                }
                itemAt.Selected = true;
                menu = new ContextMenu();
                menu.MenuItems.Add("one");
                menu.MenuItems.Add("two");
                v.ContextMenu = menu;
            }
        }
    }
}
