using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Common;
using FluentAssertions;
using Lykke.SlackNotifications;
using MarginTrading.OrderbookAggregator.Controllers;
using MarginTrading.OrderbookAggregator.Enums;
using MarginTrading.OrderbookAggregator.Services;
using Moq;
using NUnit.Framework;

namespace MarginTrading.OrderbookAggregator.Tests.Integrational.Services
{
    public class WatchdogServiceTests
    {
        private readonly OaIntegrationalTestSuit _testSuit = new OaIntegrationalTestSuit();

        [Test]
        public async Task IfLastUpdateTooMuchTimeAgo_ShouldDetectOutdation()
        {
            //arrange
            var env = _testSuit.Build();
            env.SettingsRoot.Exchanges = env.SettingsRoot.Exchanges
                .Add("bitfinex", env.SettingsRoot.Exchanges.Values.First());
            env.Setup(b => b.RegisterType<StatusController>().AsSelf());
            var container = env.CreateContainer();
            var statusController = container.Resolve<StatusController>();
            var aggregatorService = container.Resolve<IOrderbookAggregatorService>();
            var watchdogService = container.Resolve<IWatchdogService>();

            //act
            var start = env.UtcNow = env.UtcNow.RoundToSecond();
            await aggregatorService.ProcessNewExternalOrderbookAsync(env.GetOrderbookMessage("bitmex",
                Generate.Decimals(1.01m)));
            await aggregatorService.ProcessNewExternalOrderbookAsync(env.GetOrderbookMessage("bitfinex",
                    Generate.Decimals(1.1m), "ETHUSD"));
            var lastBitfinexOrderbook = env.GetOrderbookMessage("bitfinex", Generate.Decimals(1.02m));
            await aggregatorService.ProcessNewExternalOrderbookAsync(lastBitfinexOrderbook);

            env.SleepSecs(9);

            var lastBitmexOrderbook = env.GetOrderbookMessage("bitmex", Generate.Decimals(1.03m));
            await aggregatorService.ProcessNewExternalOrderbookAsync(lastBitmexOrderbook);

            env.SleepSecs(2); // 10 secs outdation threshold configured 

            await ((TimerPeriod) watchdogService).Execute();

            var listResult = await statusController.List();

            //assert
            var bitfinexStatus = env.MakeOrderbookStatusModel("bitfinex", "BTCUSD", 1.02m);
            bitfinexStatus.Status = OrderbookStatusEnum.Outdated.ToString();
            bitfinexStatus.LastUpdateTime = lastBitfinexOrderbook.Timestamp;

            var bitfinexEthStatus = env.MakeOrderbookStatusModel("bitfinex", "ETHUSD", 1.1m);
            bitfinexEthStatus.Status = OrderbookStatusEnum.Outdated.ToString();
            bitfinexEthStatus.LastUpdateTime = lastBitfinexOrderbook.Timestamp;

            var bitmexStatus = env.MakeOrderbookStatusModel("bitmex", "BTCUSD", 1.03m);
            bitmexStatus.LastUpdateTime = lastBitmexOrderbook.Timestamp;

            listResult.Should().BeEquivalentTo(bitfinexStatus, bitfinexEthStatus, bitmexStatus);

            _testSuit.GetMock<ISlackNotificationsSender>().Verify(m =>
                m.SendAsync("mt-critical", "MT OrderbookAggregator",
                    $"Orderbooks from bitfinex stopped for: BTCUSD (at {start:g}), ETHUSD (at {start:g})"), Times.Once);
        }

        [Test]
        public async Task IfLastUpdateTooMuchTimeAgo_ShouldRepeatAlertNotTooOften()
        {
            //arrange
            var env = _testSuit.Build();
            env.SettingsRoot.Exchanges = env.SettingsRoot.Exchanges
                .Add("bitfinex", env.SettingsRoot.Exchanges.Values.First());
            var container = env.CreateContainer();
            var aggregatorService = container.Resolve<IOrderbookAggregatorService>();
            var watchdogService = container.Resolve<IWatchdogService>();

            //act
            var start = env.UtcNow = env.UtcNow.RoundToSecond();
            var lastBitfinexOrderbook = env.GetOrderbookMessage("bitfinex", Generate.Decimals(1.02m));
            await aggregatorService.ProcessNewExternalOrderbookAsync(lastBitfinexOrderbook);

            // 20 secs alert period configured 
            for (var i = 0; i < 6; i++)
            {
                env.SleepSecs(11);
                await ((TimerPeriod) watchdogService).Execute();
            }

            //assert
            _testSuit.GetMock<ISlackNotificationsSender>().Verify(m =>
                m.SendAsync("mt-critical", "MT OrderbookAggregator",
                    $"Orderbooks from bitfinex stopped for: BTCUSD (at {start:g})"), Times.Exactly(3));
        }

        [Test]
        public async Task IfOrderbookReceivedAfterOutdation_ShouldDetectReversionToValidState()
        {
            //arrange
            var env = _testSuit.Build();
            env.SettingsRoot.Exchanges = env.SettingsRoot.Exchanges
                .Add("bitfinex", env.SettingsRoot.Exchanges.Values.First());
            env.Setup(b => b.RegisterType<StatusController>().AsSelf());
            var container = env.CreateContainer();
            var statusController = container.Resolve<StatusController>();
            var aggregatorService = container.Resolve<IOrderbookAggregatorService>();
            var watchdogService = container.Resolve<IWatchdogService>();

            //act
            var start = env.UtcNow = env.UtcNow.RoundToSecond();
            await aggregatorService.ProcessNewExternalOrderbookAsync(env.GetOrderbookMessage("bitmex",
                Generate.Decimals(1.01m)));
            await aggregatorService.ProcessNewExternalOrderbookAsync(env.GetOrderbookMessage("bitfinex",
                Generate.Decimals(1.1m), "ETHUSD"));
            var lastBitfinexOrderbook = env.GetOrderbookMessage("bitfinex", Generate.Decimals(1.02m));
            await aggregatorService.ProcessNewExternalOrderbookAsync(lastBitfinexOrderbook);

            env.SleepSecs(9);

            var lastBitmexOrderbook = env.GetOrderbookMessage("bitmex", Generate.Decimals(1.03m));
            await aggregatorService.ProcessNewExternalOrderbookAsync(lastBitmexOrderbook);

            env.SleepSecs(2); // 10 secs outdation threshold configured 

            await ((TimerPeriod) watchdogService).Execute();

            var outdatedResult = await statusController.List();

            await aggregatorService.ProcessNewExternalOrderbookAsync(env.GetOrderbookMessage("bitfinex",
                Generate.Decimals(1.2m), "ETHUSD"));

            await ((TimerPeriod) watchdogService).Execute();

            var afterReversionResult = await statusController.List();

            //assert
            var bitfinexStatus = env.MakeOrderbookStatusModel("bitfinex", "BTCUSD", 1.02m);
            bitfinexStatus.Status = OrderbookStatusEnum.Outdated.ToString();
            bitfinexStatus.LastUpdateTime = lastBitfinexOrderbook.Timestamp;

            var bitfinexEthStatus = env.MakeOrderbookStatusModel("bitfinex", "ETHUSD", 1.1m);
            bitfinexEthStatus.Status = OrderbookStatusEnum.Outdated.ToString();
            bitfinexEthStatus.LastUpdateTime = lastBitfinexOrderbook.Timestamp;

            var bitmexStatus = env.MakeOrderbookStatusModel("bitmex", "BTCUSD", 1.03m);
            bitmexStatus.LastUpdateTime = lastBitmexOrderbook.Timestamp;

            outdatedResult.Should().BeEquivalentTo(bitfinexStatus, bitfinexEthStatus, bitmexStatus);

            afterReversionResult.Should().BeEquivalentTo(
                bitfinexStatus,
                env.MakeOrderbookStatusModel("bitfinex", "ETHUSD", 1.2m), 
                bitmexStatus);

            _testSuit.GetMock<ISlackNotificationsSender>().Verify(m =>
                m.SendAsync("mt-critical", "MT OrderbookAggregator",
                    $"Orderbooks from bitfinex stopped for: BTCUSD (at {start:g}), ETHUSD (at {start:g})"), Times.Once);

            _testSuit.GetMock<ISlackNotificationsSender>().Verify(m =>
                m.SendAsync("mt-critical", "MT OrderbookAggregator",
                    $"Orderbooks from bitfinex started for: ETHUSD (at {env.UtcNow:g})"), Times.Once);
        }
    }
}