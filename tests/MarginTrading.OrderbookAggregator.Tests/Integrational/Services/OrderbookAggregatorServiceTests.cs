using System.Linq;
using System.Threading.Tasks;
using Autofac;
using MarginTrading.OrderbookAggregator.Contracts.Messages;
using MarginTrading.OrderbookAggregator.Infrastructure;
using MarginTrading.OrderbookAggregator.Services;
using NUnit.Framework;

namespace MarginTrading.OrderbookAggregator.Tests.Integrational.Services
{
    public class OrderbookAggregatorServiceTests
    {
        private readonly OaIntegrationalTestSuit _testSuit = new OaIntegrationalTestSuit();

        [Test]
        public async Task SimpleConfig_ShouldProcessSingleMessage()
        {
            //arrange
            var env = _testSuit.Build();
            var container = env.CreateContainer();
            var aggregatorService = container.Resolve<IOrderbookAggregatorService>();

            //act
            await aggregatorService.ProcessNewExternalOrderbookAsync(env.GetOrderbookMessage("bitmex",
                Generate.Decimals()));

            //assert
            env.VerifyMessagesSent(
                env.GetStartedMessage(),
                env.GetOrderbookMessage("bitmex", Generate.Decimals())
            );
        }

        [Test]
        public async Task SimpleConfig_ShouldApplyMarkups()
        {
            //arrange
            var env = _testSuit.Build();
            var btcusdSettings = env.AssetPairSettings.Single(s => s.AssetPairId == "BTCUSD");
            btcusdSettings.MultiplierMarkupBid = 0.9m;
            btcusdSettings.MultiplierMarkupAsk = 1.1m;
            var container = env.CreateContainer();
            var aggregatorService = container.Resolve<IOrderbookAggregatorService>();

            //act
            await aggregatorService.ProcessNewExternalOrderbookAsync(env.GetOrderbookMessage("bitmex",
                Generate.Decimals()));

            //assert
            var expectedOrderbook = env.GetOrderbookMessage("bitmex", Generate.Decimals());
            expectedOrderbook.Bids.ForEach(b => b.Price *= 0.9m);
            expectedOrderbook.Asks.ForEach(a => a.Price *= 1.1m);
            env.VerifyMessagesSent(
                env.GetStartedMessage(),
                expectedOrderbook
            );
        }

        [Test]
        public async Task SimpleConfig_ShouldTransformAssetPairIdMarkups()
        {
            //arrange
            var env = _testSuit.Build();
            var btcusdSettings = env.AssetPairSettings.Single(s => s.AssetPairId == "BTCUSD");
            btcusdSettings.AssetPairId = "BTCUSD.cy";
            var container = env.CreateContainer();
            var aggregatorService = container.Resolve<IOrderbookAggregatorService>();

            //act
            await aggregatorService.ProcessNewExternalOrderbookAsync(env.GetOrderbookMessage("bitmex",
                Generate.Decimals()));

            //assert
            env.VerifyMessagesSent(
                env.GetStartedMessage(),
                env.GetOrderbookMessage("bitmex", Generate.Decimals(), "BTCUSD.cy")
            );
        }

        [Test]
        public async Task SimpleConfig_ShouldProcessMultipleValidMessages()
        {
            //arrange
            var env = _testSuit.Build();
            env.SettingsRoot.Exchanges = env.SettingsRoot.Exchanges
                .Add("bitfinex", env.SettingsRoot.Exchanges.Values.First())
                .Add("Poloniex", env.SettingsRoot.Exchanges.Values.First());
            var container = env.CreateContainer();
            var aggregatorService = container.Resolve<IOrderbookAggregatorService>();

            //act
            await aggregatorService.ProcessNewExternalOrderbookAsync(env.GetOrderbookMessage("bitmex",
                Generate.Decimals(1.01m)));
            await aggregatorService.ProcessNewExternalOrderbookAsync(env.GetOrderbookMessage("bitfinex",
                Generate.Decimals(1.02m)));
            await aggregatorService.ProcessNewExternalOrderbookAsync(env.GetOrderbookMessage("Poloniex",
                Generate.Decimals(1.03m)));

            //assert
            env.VerifyMessagesSent(
                env.GetStartedMessage(),
                env.GetOrderbookMessage("bitmex", Generate.Decimals(1.01m)),
                env.GetOrderbookMessage("bitfinex", Generate.Decimals(1.02m)),
                env.GetOrderbookMessage("Poloniex", Generate.Decimals(1.03m)));
        }

        [Test]
        public async Task DeleteExchange_ShouldSkipIt()
        {
            //arrange
            var env = _testSuit.Build();
            env.SettingsRoot.Exchanges = env.SettingsRoot.Exchanges
                .Add("bitfinex", env.SettingsRoot.Exchanges.Values.First());
            var container = env.CreateContainer();
            var aggregatorService = container.Resolve<IOrderbookAggregatorService>();
            var settingsRootService = container.Resolve<ISettingsRootService>();

            //act
            await aggregatorService.ProcessNewExternalOrderbookAsync(env.GetOrderbookMessage("bitmex",
                Generate.Decimals(1.01m)));
            await aggregatorService.ProcessNewExternalOrderbookAsync(env.GetOrderbookMessage("bitfinex",
                Generate.Decimals(1.02m)));
            env.SettingsRoot.Exchanges = env.SettingsRoot.Exchanges.Remove("bitfinex");
            ((ICustomStartup) settingsRootService).Initialize();
            await aggregatorService.ProcessNewExternalOrderbookAsync(env.GetOrderbookMessage("bitmex",
                Generate.Decimals(1.03m)));
            await aggregatorService.ProcessNewExternalOrderbookAsync(env.GetOrderbookMessage("bitfinex",
                Generate.Decimals(1.04m)));

            //assert
            env.VerifyMessagesSent(
                env.GetStartedMessage(),
                env.GetOrderbookMessage("bitmex", Generate.Decimals(1.01m)),
                env.GetOrderbookMessage("bitfinex", Generate.Decimals(1.02m)),
                env.GetOrderbookMessage("bitmex", Generate.Decimals(1.03m)));
        }
    }
}