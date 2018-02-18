using System.Linq;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using MarginTrading.OrderbookAggregator.Controllers;
using MarginTrading.OrderbookAggregator.Services;
using NUnit.Framework;

namespace MarginTrading.OrderbookAggregator.Tests.Integrational.Controllers
{
    public class StatusControllerTests
    {
        private readonly OaIntegrationalTestSuit _testSuit = new OaIntegrationalTestSuit();

        [Test]
        public async Task Always_ShouldShowStatus()
        {
            //arrange
            var env = _testSuit.Build();
            env.SettingsRoot.Exchanges = env.SettingsRoot.Exchanges
                .Add("bitfinex", env.SettingsRoot.Exchanges.Values.First());
            env.Setup(b => b.RegisterType<StatusController>().AsSelf());
            var container = env.CreateContainer();
            var statusController = container.Resolve<StatusController>();
            var aggregatorService = container.Resolve<IOrderbookAggregatorService>();

            //act
            await aggregatorService.ProcessNewExternalOrderbookAsync(env.GetOrderbookMessage("bitmex",
                Generate.Decimals(1.01m)));
            await aggregatorService.ProcessNewExternalOrderbookAsync(env.GetOrderbookMessage("bitfinex",
                Generate.Decimals(1.02m), "ETHUSD"));

            var listResult = await statusController.List();
            var byAssetPairResult = await statusController.GetByAssetPair("BTCUSD");
            var byExchangeResult = await statusController.GetByExchange("bitmex");

            //assert
            listResult.Should().BeEquivalentTo(new[]
            {
                env.MakeOrderbookStatusModel("bitmex", "BTCUSD", 1.01m),
                env.MakeOrderbookStatusModel("bitfinex", "ETHUSD", 1.02m)
            });
            byAssetPairResult.Should().BeEquivalentTo(new[] {env.MakeOrderbookStatusModel("bitmex", "BTCUSD", 1.01m)});
            byExchangeResult.Should().BeEquivalentTo(new[] {env.MakeOrderbookStatusModel("bitmex", "BTCUSD", 1.01m)});
        }
    }
}