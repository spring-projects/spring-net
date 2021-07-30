using System.Collections;
using Spring.Objects;

namespace Spring.Data
{
    public interface ITestObjectDao
    {
        void Create(string name, int age);
        void Update(TestObject to);
        void Delete(string name);
        TestObject FindByName(string name);
        IList FindAll();
        int GetCount();
        int GetCountByDelegate();

        int GetCount(int lowerAgeLimit);
        int GetCount(int lowerAgeLimit, string name);
        
        //
        int GetCountByAltMethod(int lowerAgeLimit);
        int GetCountByCommandSetter(int lowerAgeLimit);
        void Cleanup();
    }
}