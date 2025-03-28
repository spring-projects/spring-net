using Spring.Objects;

namespace Spring.Data;

public interface ITestCoordinator
{
    ITestObjectManager TestObjectManager
    {
        get;
        set;
    }

    void WorkOn(TestObject to1, TestObject to2);
}
