using System.Collections.Generic;
using Forest.Engine;
using Microsoft.AspNetCore.Mvc;

namespace Forest.Web.AspNetCore.Controllers
{
    [Route("api/forest")]
    public class ForestController
    {
        public ForestController(IForestEngine forest)
        {
            
        }
        
        // GET api/forest
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new [] { "value1", "value2" };
        }
    }
}