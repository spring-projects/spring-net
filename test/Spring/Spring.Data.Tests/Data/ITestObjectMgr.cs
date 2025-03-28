using Spring.Objects;

namespace Spring.Data;

public interface ITestObjectMgr
{
    void SaveTwoTestObjects(TestObject to1, TestObject to2);

    void DeleteTwoTestObjects(string name1, string name2);
}
