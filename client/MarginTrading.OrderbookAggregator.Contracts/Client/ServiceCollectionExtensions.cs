using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace MarginTrading.OrderbookAggregator.Contracts.Client
{
    public static class ServiceCollectionExtensions
    {
        [PublicAPI]
        public static void RegisterMtOrderbookAggregatorClient(this IServiceCollection services, string url, string userAgent)
        {
            services.AddSingleton<IMtOrderbookAggregatorClient>(p => new MtOrderbookAggregatorClient(url, userAgent));
        }
    }
}