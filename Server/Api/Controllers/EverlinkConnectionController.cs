using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grip.Bll.DTO.Everlink;
using Grip.Bll.Everlink;
using Grip.Bll.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Grip.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EverlinkConnectionController : ControllerBase
    {
        private readonly ILogger<EverlinkConnectionController> _logger;

        private readonly IEverlinkAdapterService _everlinkService;
        public EverlinkConnectionController(IEverlinkAdapterService everlinkService, ILogger<EverlinkConnectionController> logger)
        {
            _everlinkService = everlinkService;
            _logger = logger;
        }

        [HttpGet("[action]")]
        [Authorize(Policy = "Everlink", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<TableDTO> Query([FromQuery]string query){
            _logger.LogInformation($"Sending query {query} to EverlinkAdapterHub");
            var result = await _everlinkService.SendQuery(query);
            _logger.LogInformation($"Query {query} executed successfully");
            return result;
        }

        [HttpGet("[action]")]
        [Authorize(Policy = "Everlink", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> QueryZip([FromQuery]string query){
            _logger.LogInformation($"Sending query {query} to EverlinkAdapterHub");
            var result = await _everlinkService.SendQueryZip(query);
            _logger.LogInformation($"Query {query} executed successfully");
            return File(result, "application/zip", "result.zip");
        }

    }
}