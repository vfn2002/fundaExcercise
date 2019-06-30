using Microsoft.AspNetCore.Mvc;
using Core.Models;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SourceController : ControllerBase
    {
        private readonly AgentContext _context;
        private readonly AppSettings _appSettings;
        private readonly ILogger<AgentController> _logger;
        private readonly IConfiguration _config;

        public SourceController(
            AgentContext context,
            ILogger<AgentController> logger,
            IOptions<AppSettings> options,
            IConfiguration config
        )
        {
            _context = context;
            _logger = logger;
            _appSettings = options.Value;
            _config = config;
        }

        [HttpGet("loadAgents/{category}/{city}/")]
        public async Task<ActionResult> FetchAgents(string category, string city) {
            try {
                var agents = ConvertSourceResultsToAgents
                (
                    await FetchResults($"/?type={category}&zo=/"),
                    ListingType.All
                );

                _context.AddRange(agents);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                return ExceptionResult(e);
            }
            
            return Ok();
        }

        [HttpGet("loadAgents/{category}/{city}/{filter}")]
        public async Task<ActionResult> FetchAgentsThatSellGardens(string category, string city, string filter) {
            try {
                var agents = ConvertSourceResultsToAgents
                (
                    await FetchResults($"/?type={category}&zo=/{city}/{filter}"),
                    ListingType.Garden
                );

                _context.AddRange(agents);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                return ExceptionResult(e);
            }

            return Ok();
        }

        private async Task<IEnumerable<SourceResult>> FetchResults(string requestParams)
        {
            _logger.LogDebug($"Started fetching agents.");

            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(_appSettings.ConnectionStrings.FundaApi);

                    var results = new List<SourceResult>();
                    var pagesToLoad = 1;
                    var isPagesToLoadSet = false;

                    for (var currentPage = 1; currentPage <= pagesToLoad; currentPage++)
                    {
                        var response = await client.GetAsync
                        (
                            $"{_config["Api:ApiKey"]}{requestParams}&page={currentPage}&pagesize={_appSettings.RequestSize}"
                        );

                        response.EnsureSuccessStatusCode();

                        var stringResult = await response.Content.ReadAsStringAsync();
                        var searchResult = JsonConvert.DeserializeObject<SourceResult>(stringResult);

                        if (!isPagesToLoadSet)
                        {
                            var requestPages = _appSettings.RequestPages;

                            pagesToLoad = requestPages == -1 
                                ? searchResult.Paging.AantalPaginas
                                : requestPages;
                            isPagesToLoadSet = true;
                        }

                        results.Add(searchResult);

                        _logger.LogDebug($"Page: {currentPage} of {pagesToLoad} loaded.");

                        Task.Delay(_appSettings.DebounceTimerMs).Wait();
                    }

                    return results;
                }
                catch
                {
                    throw;
                }
            }
        }
    
        private ActionResult ExceptionResult(Exception e)
        {
            _logger.LogError($"{e.ToString()}");

            return BadRequest(
                new {
                    message = "Something went wrong while fetching agents. See exception for more detail",
                    exception = e.ToString()
                }
            );
        }

        private IEnumerable<Agent> ConvertSourceResultsToAgents(IEnumerable<SourceResult> results, ListingType listingType)
        {
            var agents = new List<Agent>();

            foreach (var result in results)
            {
                foreach (var listing in result.Objects)
                {
                    var agent = agents.Find(x => x.Id == listing.MakelaarId && x.ListingType == listingType);

                    if (agent == null)
                    {
                        agents.Add(new Agent
                        {
                            Id = listing.MakelaarId,
                            Name = listing.MakelaarNaam,
                            Listings = 1,
                            ListingType = listingType
                        });
                    }
                    else
                    {
                        agent.Listings++;
                    }
                }
            }

            return agents;
        }
    }
}
