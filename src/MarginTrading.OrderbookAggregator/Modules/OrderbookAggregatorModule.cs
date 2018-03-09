using System.Linq;
using Autofac;
using Common.Log;
using Lykke.SettingsReader;
using MarginTrading.OrderbookAggregator.Infrastructure;
using MarginTrading.OrderbookAggregator.Infrastructure.Implementation;
using MarginTrading.OrderbookAggregator.Services;
using MarginTrading.OrderbookAggregator.Services.Implementation;
using MarginTrading.OrderbookAggregator.Settings;

namespace MarginTrading.OrderbookAggregator.Modules
{
    internal class OrderbookAggregatorModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;
        private readonly ILog _log;

        public OrderbookAggregatorModule(IReloadingManager<AppSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            RegisterDefaultImplementations(builder);

            builder.RegisterInstance(_settings.Nested(s => s.MarginTradingOrderbookAggregator)).SingleInstance();
            builder.RegisterInstance(_settings.Nested(s => s.RiskInformingSettings)).SingleInstance();
            builder.RegisterInstance(_log).As<ILog>().SingleInstance();
            builder.RegisterType<SystemService>().As<ISystem>().SingleInstance();

            builder.RegisterType<BrokerService>().As<IBrokerService>().InstancePerDependency();
            
            builder.RegisterInstance(new RabbitMqService(_log,
                    _settings.Nested(s => s.MarginTradingOrderbookAggregator.Db.QueuePersistanceRepositoryConnString)))
                .As<IRabbitMqService>().SingleInstance();
        }

        /// <summary>
        /// Scans for types in the current assembly and registers types which: <br/>
        /// - are named like 'SmthService' <br/>
        /// - implement an non-generic interface named like 'ISmthService' in the same assembly <br/>
        /// - are the only implementations of the 'ISmthService' interface <br/>
        /// - are not generic <br/><br/>
        /// Types like SmthRepository are also supported.
        /// Also registers startup for implementations of <see cref="ICustomStartup"/>.
        /// </summary>
        private void RegisterDefaultImplementations(ContainerBuilder builder)
        {
            var assembly = GetType().Assembly;
            var implementations = assembly.GetTypes()
                .Where(t => !t.IsInterface && !t.IsGenericType && (t.Name.EndsWith("Service") || t.Name.EndsWith("Repository")))
                .SelectMany(t =>
                    t.GetInterfaces()
                        .Where(i => i.Name.StartsWith('I') && i.Assembly == assembly)
                        .Select(i => (Implementation: t, Interface: i)))
                .GroupBy(t => t.Interface)
                .Where(gr => gr.Count() == 1)
                .SelectMany(gr => gr);

            foreach (var t in implementations)
            {
                var registrationBuilder = builder.RegisterType(t.Implementation).As(t.Interface).SingleInstance();
                if (typeof(ICustomStartup).IsAssignableFrom(t.Implementation))
                    registrationBuilder.OnActivated(args => ((ICustomStartup) args.Instance).Initialize()).AutoActivate();
            }
        }
    }
}