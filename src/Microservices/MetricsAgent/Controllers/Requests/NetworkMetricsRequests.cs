using System;
using Microsoft.AspNetCore.Mvc;

namespace MetricsAgent.Controllers.Requests
{
    public record NetworkMetricsRequest
    (
        [FromRoute] DateTimeOffset FromTime,
        [FromRoute] DateTimeOffset ToTime
    );
}