using Autofac;
using Autofac.Extensions.DependencyInjection;
using Lykke.SettingsReader;
using MarginTrading.Backend.Contracts.DataReaderClient;
using MarginTrading.OrderbookAggregator.Infrastructure.Implementation;
using MarginTrading.OrderbookAggregator.Settings;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;

namespace MarginTrading.OrderbookAggregator.Modules
{
    internal class OrderbookAggregatorExternalServicesModule : Module
    {
        private readonly IServiceCollection _services = new ServiceCollection();
        private readonly IReloadingManager<AppSettings> _settings;

        public OrderbookAggregatorExternalServicesModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            _services.AddSingleton<ITelemetryInitializer, UserAgentTelemetryInitializer>();
            _services.RegisterMtDataReaderClient(_settings.CurrentValue.MtDataReaderLiveServiceClient.ServiceUrl,
                _settings.CurrentValue.MtDataReaderLiveServiceClient.ApiKey, "MarginTrading.OrderbookAggregator");
            builder.Populate(_services);
        }
    }
}