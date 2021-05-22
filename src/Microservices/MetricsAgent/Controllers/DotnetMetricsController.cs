using System.Linq;
using AutoMapper;
using Common;
using MetricsAgent.Controllers.Requests;
using MetricsAgent.Controllers.Responses;
using MetricsAgent.DataBase.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MetricsAgent.Controllers
{
    [ApiController]
    [Route("api/metrics/dotnet")]
    public class DotnetMetricsController : ControllerBase
    {
        private readonly IDotnetMetricsRepository _repository;
        private readonly ILogger<DotnetMetricsController> _logger;
        private readonly IMapper _mapper;

        public DotnetMetricsController(IDotnetMetricsRepository repository, ILogger<DotnetMetricsController> logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Get dotnet metrics by time period
        /// </summary>
        /// <param name="request">Request that hold time period filter</param>
        /// <returns>List of metrics that have been saved over a given time range</returns>
        /// <response code="200">if metrics found</response>
        /// <response code="404">if metrics not found</response>
        [HttpGet("from/{fromTime}/to/{toTime}")]
        public IActionResult GetByTimePeriod([FromRoute] ErrorsCountRequest request)
        {
            _logger.LogInformation(
                LogEvents.RequestReceived,
                "Get dotnet metrics by time period request received: {From}, {To}"
                ,request.FromTime.ToString("yyyy-M-d dddd"),
                request.ToTime.ToString("yyyy-M-d dddd"));

            var result = _repository.GetByTimePeriod(request.FromTime, request.ToTime);
            if (result is null)
            {
                return NotFound();
            }
            return Ok(new DotnetMetricsByTimePeriodResponse()
            {
                Metrics = result.Select(_mapper.Map<DotnetMetricResponse>)
            });
        }
    }
}