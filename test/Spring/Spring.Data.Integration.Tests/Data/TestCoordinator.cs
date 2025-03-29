using Spring.Objects;

namespace Spring.Data;

public class TestCoordinator : ITestCoordinator
{
    private ITestObjectManager testObjectManager;

    public ITestObjectManager TestObjectManager
    {
        get { return testObjectManager; }
        set { testObjectManager = value; }
    }

    public void WorkOn(TestObject to1, TestObject to2)
    {
        testObjectManager.SaveTwoTestObjects(to1, to2);
    }
}
