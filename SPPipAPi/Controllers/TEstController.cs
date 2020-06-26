using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SPPipAPi.Controllers
{
    public class TEstController : ApiController
    {
        // GET: api/TEst
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/TEst/5
        public string Get(int id,string test)
        {
            return "value";
        }

        // POST: api/TEst
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/TEst/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/TEst/5
        public void Delete(int id)
        {
        }
    }
}
