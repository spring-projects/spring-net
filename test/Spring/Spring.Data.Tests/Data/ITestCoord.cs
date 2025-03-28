using Spring.Objects;

namespace Spring.Data;

public interface ITestCoord
{
    ITestObjectMgr TestObjectMgr
    {
        get;
        set;
    }

    void WorkOn(TestObject to1, TestObject to2);
}
