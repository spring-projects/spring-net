using Spring.Objects;

namespace Spring.Data;

public class TestCoord : ITestCoord
{
    private ITestObjectMgr testObjectMgr;

    public ITestObjectMgr TestObjectMgr
    {
        get { return testObjectMgr; }
        set { testObjectMgr = value; }
    }

    public void WorkOn(TestObject to1, TestObject to2)
    {
        testObjectMgr.SaveTwoTestObjects(to1, to2);
    }
}
