using MetricsAgent.Controllers.Requests;
using Microsoft.AspNetCore.Mvc;

namespace MetricsAgent.Controllers
{
    public class NetworkController : ApiController
    {
        [HttpGet("from/{fromTime:datetime}/to/{toTime:datetime}")]
        public IActionResult GetMetrics([FromRoute] GetNetworkMetricsRequest request)
        {
            return Ok();
        }
    }
}