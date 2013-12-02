using System.Collections.Generic;
using System.Web.Http;

namespace Spring.Mvc5QuickStart.Controllers
{
    public class SuffixController : ApiController
    {
        public string Suffix { get; set; }

        // GET /api/suffix
        public IEnumerable<string> Get()
        {
            return new string[] { string.Format("value1_{0}", Suffix), string.Format("value2_{0}", Suffix) };
        }

        // GET /api/suffix/5
        public string Get(int id)
        {
            return string.Format("value{0}_{1}", Suffix, id);
        }

        // POST /api/suffix
        public void Post(string value)
        {
        }

        // PUT /api/suffix/5
        public void Put(int id, string value)
        {
        }

        // DELETE /api/suffix/5
        public void Delete(int id)
        {
        }
    }

}