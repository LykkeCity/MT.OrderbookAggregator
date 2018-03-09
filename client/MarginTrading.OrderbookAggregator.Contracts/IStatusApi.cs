using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.OrderbookAggregator.Contracts.Api;
using Refit;

namespace MarginTrading.OrderbookAggregator.Contracts
{
    [PublicAPI]
    public interface IStatusApi
    {
        /// <summary>
        /// Gets all status
        /// </summary>
        [Get("/api/status/")]
        Task<IReadOnlyList<OrderbookStatusModel>> List();

        /// <summary>
        /// Gets status for a single asset pair
        /// </summary>
        [Get("/api/status/byAssetPair/{assetPairId}")]
        Task<IReadOnlyList<OrderbookStatusModel>> GetByAssetPair(string assetPairId);

        /// <summary>
        /// Gets status for a single exchange
        /// </summary>
        [Get("/api/status/byExchange/{exchangeName}")]
        Task<IReadOnlyList<OrderbookStatusModel>> GetByExchange(string exchangeName);
    }
}