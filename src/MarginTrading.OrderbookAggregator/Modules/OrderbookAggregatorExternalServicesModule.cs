using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Lykke.Service.Assets.Client;
using Lykke.Service.CandlesHistory.Client;
using Lykke.SettingsReader;
using MarginTrading.OrderbookAggregator.Infrastructure.Implementation;
using MarginTrading.OrderbookAggregator.Settings;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;

namespace MarginTrading.OrderbookAggregator.Modules
{
    internal class OrderbookAggregatorExternalServicesModule: Module
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
            
            builder.Populate(_services);
        }
    }
}