using Microsoft.AspNetCore.Mvc;
using Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;

namespace core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgentController : ControllerBase
    {
        private readonly AgentContext _context;
        private readonly ILogger<AgentController> _logger;
        private readonly AppSettings _appSettings;
        private readonly IConfiguration _config;
        private readonly SourceController _sourceController;

        public AgentController(
            AgentContext context,
            ILogger<AgentController> logger,
            IOptions<AppSettings> options,
            IConfiguration config,
            SourceController sourceController
        )
        {
            _context = context;
            _logger = logger;
            _appSettings = options.Value;
            _config = config;
            _sourceController = sourceController;
        }

        [HttpGet("getMostListings/{category}/{city}/{take}")]
        public ActionResult GetAgentsWithMostListings(string category, string city, int take) {
            return GetResult(category, city, take, ListingType.All);
        }

        [HttpGet("getMostGardenListings/{category}/{city}/{take}")]
        public ActionResult GetAgentsWithMostGardenListings(string category, string city, string filter, int take) {
            return GetResult(category, city, take, ListingType.Garden);
        }

        private ActionResult GetResult(string category, string city, int take, ListingType listingType)
        {
            var agents = _context.Agents
                .Where(x => x.ListingType == listingType)
                .OrderByDescending(x => x.Listings)
                .Take(take)
                .ToList();
            
            if (!agents.Any())
            {
                _logger.LogDebug($"No agents were found, fetching data...");

                var result = listingType == ListingType.All ? _sourceController.FetchAgents(category, city).Result
                                                            : _sourceController.FetchAgentsThatSellGardens(category, city, "tuin").Result;

                if (result is OkResult)
                {
                    return Ok
                    (
                        _context.Agents
                            .Where(x => x.ListingType == listingType)
                            .OrderByDescending(x => x.Listings)
                            .Take(take)
                            .ToList()
                    );
                }

                return NotFound(
                    new
                    {
                        error = "Something went wrong trying to fetch agents. Please try again later."
                    }
                );
            }

            return Ok(agents);  
        }
    }
}
