using System.Collections.Generic;
using System.Web.Http;

namespace Spring.Mvc4QuickStart.Controllers
{
    public class SuffixController : ApiController
    {
        public string Suffix { get; set; }

        // GET /api/values
        public IEnumerable<string> Get()
        {
            return new string[] { string.Format("value1{0}", Suffix), string.Format("value2{0}", Suffix) };
        }

        // GET /api/values/5
        public string Get(int id)
        {
            return string.Format("value{0}", Suffix);
        }

        // POST /api/values
        public void Post(string value)
        {
        }

        // PUT /api/values/5
        public void Put(int id, string value)
        {
        }

        // DELETE /api/values/5
        public void Delete(int id)
        {
        }
    }

}