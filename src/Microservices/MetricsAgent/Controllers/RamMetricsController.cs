using System.Linq;
using Common;
using MetricsAgent.Controllers.Requests;
using MetricsAgent.Controllers.Responses;
using MetricsAgent.DataBase.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MetricsAgent.Controllers
{
    [Route("api/metrics/ram")]
    public class RamMetricsController : ApiController
    {
        private readonly IRamMetricsRepository _repository;
        private readonly ILogger<RamMetricsController> _logger;

        public RamMetricsController(IRamMetricsRepository repository, ILogger<RamMetricsController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// Get ram metrics by time period
        /// </summary>
        /// <param name="request">Request that hold time period filter</param>
        /// <returns>List of metrics that have been saved over a given time range</returns>
        /// <response code="200">if metrics found</response>
        /// <response code="404">if metrics not found</response>
        [HttpGet("from/{fromTime}/to/{toTime}")]
        public IActionResult GetByTimePeriod([FromRoute] AvailableSpaceInfoRequest request)
        {
            _logger.LogInformation(
                LogEvents.RequestReceived,
                "Get ram metrics by time period request received: {From}, {To}"
                ,request.FromTime.ToString("yyyy-M-d dddd"),
                request.ToTime.ToString("yyyy-M-d dddd"));

            var result = _repository.GetByTimePeriod(request.FromTime, request.ToTime);
            if (result is null)
            {
                return NotFound();
            }
            return Ok(new RamMetricsByTimePeriodResponse()
            {
                Metrics = result.Select(Mapper.Map<RamMetricResponse>)
            });
        }
    }
}