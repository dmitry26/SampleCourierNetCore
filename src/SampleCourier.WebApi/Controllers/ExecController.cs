using Dmo.Threading;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SampleCourier.Models;
using SampleCourier.WebApi.Requests;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SampleCourier.WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/Exec")]
    public class ExecController : Controller
    {
        private readonly RoutingSlipPublisher _routingSlipPub;
        private readonly ILogger _logger;
        private readonly RoutingSlipDbContextFactory _dbCtxFact;

        public ExecController(RoutingSlipPublisher routingSlipPub, RoutingSlipDbContextFactory dbCtxFact, ILogger<ExecController> logger)
        {
            _routingSlipPub = routingSlipPub;
            _dbCtxFact = dbCtxFact;
            _logger = logger;
        }

        /// <summary>
        /// Retrieve the last 10 records
        /// </summary>
        /// <returns></returns>
        // GET: api/Exec
        [HttpGet("")]
        public async Task<IActionResult> Get()
        {
            using var dbCtx = _dbCtxFact.CreateDbContext(Array.Empty<string>());

            var items = await dbCtx.Set<RoutingSlipState>()
                .Take(10)
                .OrderByDescending(x => x.CreateTime).ToArrayAsync().ConfigureAwait(false);

            var res = items.Select(state => new
            {
                TrackingNumber = state.CorrelationId,
                state.CreateTime,
                state.EndTime,
                state.State
            });

            return Ok(res);
        }

        /// <summary>
        /// Check status of the tracking number
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/Exec/aa973265-319a-4697-8b68-b74078dfd886
        [HttpGet("{id}", Name = "CheckStatus")]
        public async Task<IActionResult> CheckStatus(Guid id)
        {
            using var dbCtx = _dbCtxFact.CreateDbContext(Array.Empty<string>());
            var str = dbCtx.Set<RoutingSlipState>().Where(x => x.CorrelationId == id).ToString();
            var state = await dbCtx.Set<RoutingSlipState>().Where(x => x.CorrelationId == id).FirstOrDefaultAsync();

            if (state == null)
                return NotFound(new
                {
                    TrackingNumber = id,
                    Status = "Not Available"
                });

            return Ok(new
            {
                TrackingNumber = state.CorrelationId,
                state.CreateTime,
                state.EndTime,
                state.State
            });
        }

        /// <summary>
        /// Download a file
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        // POST: api/Exec
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]ExecuteReq req)
        {
            if (req == null)
                return BadRequest("req is null");

            try
            {
                var reqId = NewId.NextGuid();

                _logger.LogInformation($"Request ({reqId}): url = {req.Url}, count = {req.Count}");

                return await Execute(reqId, req.Url, req.Count).ConfigureAwait(false);
            }
            catch (UriFormatException)
            {
                return BadRequest($"The Request URL is invalid: {req.Url}");
            }
        }

        /// <summary>
        /// Download a predefined file
        /// </summary>
        /// <param name="id"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        // PUT: api/Exec/5/1
        [HttpPut("{id}/{count?}")]
        public async Task<IActionResult> Put(int id, int count = 1)
        {
            var reqId = NewId.NextGuid();

            _logger.LogInformation($"Request ({reqId}): url-id = {id}, count = {count}");

            var urlStr = GetUrlStr(id);

            if (string.IsNullOrEmpty(urlStr))
                return BadRequest($"Invalid id: {id}");

            return await Execute(reqId, urlStr, count).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete the record
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            using var dbCtx = _dbCtxFact.CreateDbContext(Array.Empty<string>());
            var stateToDelete = new RoutingSlipState(id);
            dbCtx.Entry(stateToDelete).State = EntityState.Deleted;

            try
            {
                var res = await dbCtx.SaveChangesAsync().ConfigureAwait(false);
                return Ok(res);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Ok(0);
            }
        }

        /// <summary>
        /// Delete all records older than today
        /// </summary>
        /// <returns></returns>
        // DELETE: api/ApiWithActions
        [HttpDelete("old")]
        public async Task<IActionResult> DeleteOld()
        {
            using var dbCtx = _dbCtxFact.CreateDbContext(Array.Empty<string>());
            var sql = @"
delete
from dbo.EfCoreRoutingSlipState
where isnull(EndTime,CreateTime) < cast(getdate() as date)
";
            var res = await dbCtx.Database.ExecuteSqlRawAsync(sql);

            return Ok(res);
        }

        private string GetUrlStr(int id)
        {
            return id switch
            {
                0 => "http://www.microsoft.com/index.html",

                1 => "http://i.imgur.com/Iroma7d.png",

                2 => "http://i.imgur.com/NK8eZUe.jpg",

                _ => null,
            };
        }

        private async Task<IActionResult> Execute(Guid reqId, string reqUrlStr, int count)
        {
            var sw = Stopwatch.StartNew();

            var reqUri = new Uri(reqUrlStr);

            if (count <= 0) count = 1;

            var receipts = await Enumerable.Range(0, count).ForEachAsync<int, Guid>(i => _routingSlipPub.Publish(reqUri)).ConfigureAwait(false);

            var t = Task.Run(() => _logger.LogInformation($"Sent successfully ({reqId}), duration = {sw.ElapsedMilliseconds}ms"));

            var res = receipts.Select(x => new
            {
                Url = reqUrlStr,
                TrackingNumber = x
            });

            return Accepted(res);
        }
    }
}