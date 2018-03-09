using Refit;

namespace MarginTrading.OrderbookAggregator.Contracts.Client
{
    internal class MtOrderbookAggregatorClient : IMtOrderbookAggregatorClient
    {
        public ISettingsRootApi SettingsRoot { get; }
        public IStatusApi Status { get; }

        public MtOrderbookAggregatorClient(string url, string userAgent)
        {
            var httpMessageHandler = new UserAgentHttpClientHandler(userAgent);
            var settings = new RefitSettings {HttpMessageHandlerFactory = () => httpMessageHandler};
            SettingsRoot = RestService.For<ISettingsRootApi>(url, settings);
            Status = RestService.For<IStatusApi>(url, settings);
        }
    }
}