using System;
using System.Collections.Immutable;
using Autofac;
using FluentAssertions;
using MarginTrading.OrderbookAggregator.Controllers;
using MarginTrading.OrderbookAggregator.Enums;
using MarginTrading.OrderbookAggregator.Models.Settings;
using NUnit.Framework;

namespace Tests.Integrational.Controllers
{
    public class SettingsRootControllerTests
    {
        private readonly OaIntegrationalTestSuit _testSuit = new OaIntegrationalTestSuit();
        
        [Test]
        public void Always_ShouldCorrectlyUpdateSettings()
        {
            //arrange
            var env = _testSuit.Build();
            env.Setup(b => b.RegisterType<SettingsRootController>().AsSelf());
            var container = env.CreateContainer();
            var sut = container.Resolve<SettingsRootController>();

            //act
            var settings = sut.Get();
            env.Sleep(new TimeSpan(1));
            settings.Exchanges.Add("ICM",
                new ExchangeSettings(ExchangeModeEnum.TakeAll,
                    ImmutableSortedDictionary<string, AssetPairSettings>.Empty,
                    new AssetPairSettings(new AssetPairMarkupsParams(-11, 11)), TimeSpan.FromSeconds(12)));
            sut.Set(settings);
            var settings2 = sut.Get();
            
            //assert
            settings2.Should().BeEquivalentTo(settings);
        }
    }
}