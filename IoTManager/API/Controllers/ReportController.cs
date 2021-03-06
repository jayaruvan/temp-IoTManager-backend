using System;
using IoTManager.Core.Infrastructures;
using IoTManager.Model;
using IoTManager.Utility.Serializers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IoTManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController
    {
        private readonly IDeviceDataBus _deviceDataBus;
        private readonly ILogger _logger;

        public ReportController(IDeviceDataBus deviceDataBus, ILogger<ReportController> logger)
        {
            this._deviceDataBus = deviceDataBus;
            this._logger = logger;
        }

        [HttpPost("byRegion")]
        public ResponseSerializer GetReportByRegion(String cityName,String factoryName, [FromBody] DataStatisticRequestModel date)
        {
            return new ResponseSerializer(
                200,
                "success",
                this._deviceDataBus.GetReportByRegion(cityName,factoryName, date.StartTime, date.EndTime));
        }

        [HttpPost("byTime")]
        public ResponseSerializer GetReportByTime([FromBody] DataStatisticRequestModel date)
        {
            return new ResponseSerializer(
                200,
                "success",
                this._deviceDataBus.GetReportByTime(date.StartTime, date.EndTime));
        }

        [HttpPost("byType")]
        public ResponseSerializer GetReportByType([FromBody] DataStatisticRequestModel date)
        {
            return new ResponseSerializer(
                200,
                "success",
                this._deviceDataBus.GetReportByType(date.StartTime, date.EndTime));
        }

        [HttpPost("byTag")]
        public ResponseSerializer GetReportByTag([FromBody] DataStatisticRequestModel date)
        {
            return new ResponseSerializer(
                200,
                "success",
                this._deviceDataBus.GetReportByTag(date.StartTime, date.EndTime));
        }
    }
}