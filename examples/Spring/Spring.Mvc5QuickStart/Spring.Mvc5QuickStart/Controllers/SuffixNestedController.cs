using System.Collections.Generic;
using System.Web.Http;

namespace Spring.Mvc5QuickStart.Controllers
{
    public class SuffixNestedController : ApiController
    {
       //same public API as SuffixController, only here so that its config can be shown to
       // be properly read out of a separate 'child' config file

        public string Suffix { get; set; }

        // GET /api/suffixnested
        public IEnumerable<string> Get()
        {
            return new string[] { string.Format("value1_{0}", Suffix), string.Format("value2_{0}", Suffix) };
        }

        // GET /api/suffixnested/5
        public string Get(int id)
        {
            return string.Format("value{0}_{1}", Suffix, id);
        }

        // POST /api/suffixnested
        public void Post(string value)
        {
        }

        // PUT /api/suffixnested/5
        public void Put(int id, string value)
        {
        }

        // DELETE /api/suffixnested/5
        public void Delete(int id)
        {
        }
    }

}