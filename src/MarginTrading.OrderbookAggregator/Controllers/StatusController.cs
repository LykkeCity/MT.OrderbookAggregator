using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.OrderbookAggregator.Contracts;
using MarginTrading.OrderbookAggregator.Contracts.Api;
using MarginTrading.OrderbookAggregator.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.OrderbookAggregator.Controllers
{
    [Route("api/[controller]")]
    public class StatusController : Controller, IStatusApi
    {
        private readonly IOrderbooksStatusService _orderbooksStatusService;

        public StatusController(IOrderbooksStatusService orderbooksStatusService)
        {
            _orderbooksStatusService = orderbooksStatusService;
        }

        /// <summary>
        /// Gets all status
        /// </summary>
        [HttpGet]
        public Task<IReadOnlyList<OrderbookStatusModel>> List()
        {
            return Task.FromResult(_orderbooksStatusService.GetAll());
        }

        /// <summary>
        /// Gets status for a single asset pair
        /// </summary>
        [HttpGet]
        [Route("byAssetPair/{assetPairId}")]
        public Task<IReadOnlyList<OrderbookStatusModel>> GetByAssetPair(string assetPairId)
        {
            return Task.FromResult(_orderbooksStatusService.GetByAssetPair(assetPairId));
        }
        
        /// <summary>
        /// Gets status for a single exchange
        /// </summary>
        [HttpGet]
        [Route("byExchange/{exchangeName}")]
        public Task<IReadOnlyList<OrderbookStatusModel>> GetByExchange(string exchangeName)
        {
            return Task.FromResult(_orderbooksStatusService.GetByExchange(exchangeName));
        }
    }
}