using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grip.Bll.Services.Interfaces;
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
        [AllowAnonymous]
        public async Task<IActionResult> Query([FromQuery]string query){
            _logger.LogInformation($"Sending query {query} to EverlinkAdapterHub");
            var result = await _everlinkService.SendQuary(query);
            _logger.LogInformation($"Query {query} executed with result {Encoding.UTF8.GetString(result)}");
            return Ok();
        }

    }
}