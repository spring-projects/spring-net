using System.Collections;
using System.Web.UI;
using Spring.Web.UI;

namespace Spring.TestSupport
{
    public class DictionaryModelPersistenceMedium : IModelPersistenceMedium
    {
        private Hashtable _storage = new Hashtable();

        public object LoadFromMedium(Control context)
        {
            return _storage[context];
        }

        public void SaveToMedium(Control context, object modelToSave)
        {
            _storage[context] = modelToSave;
        }
    }
}