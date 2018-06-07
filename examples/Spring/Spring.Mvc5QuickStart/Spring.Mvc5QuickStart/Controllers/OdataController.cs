
using System.Linq;
using System.Web.Http;
using System.Web.Http.OData;

namespace Spring.Mvc5QuickStart.Controllers
{
    public class OdataController : ApiController
    {
        // GET /api/odata/5
        public string Get(int id)
        {
            return "value";
        }

        // GET /api/odata?$skip=1&$top=2
        // GET /api/odata?$filter=SomeProperty%20eq%203
        [EnableQuery]
        public IQueryable<SampleDataItem> Get()
        {
            var dataItems = new[]
                {
                        new SampleDataItem { SomeProperty = 1 },
                        new SampleDataItem { SomeProperty = 2 },
                        new SampleDataItem { SomeProperty = 3 },
                        new SampleDataItem { SomeProperty = 4 },
                };

            return dataItems.AsQueryable();
        }

        // POST /api/odata
        public void Post(string value)
        {
        }

        // PUT /api/odata/5
        public void Put(int id, string value)
        {
        }

        // DELETE /api/odata/5
        public void Delete(int id)
        {
        }
    }

    public class SampleDataItem
    {
        public int SomeProperty { get; set; }
    }
}