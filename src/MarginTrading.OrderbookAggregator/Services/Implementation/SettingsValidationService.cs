using System;
using MarginTrading.OrderbookAggregator.Enums;
using MarginTrading.OrderbookAggregator.Infrastructure.Implementation;
using MarginTrading.OrderbookAggregator.Models.Settings;

namespace MarginTrading.OrderbookAggregator.Services.Implementation
{
    public class SettingsValidationService : ISettingsValidationService
    {
        public void Validate(SettingsRoot root)
        {
            root.OutdationCheckPeriod.RequiredBetween(TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(10),
                "root.OutdationCheckPeriod");
            root.Exchanges.RequiredNotNull("root.AssetPairs");

            foreach (var (exchangeName, exchangeSettings) in root.Exchanges)
                new ExchangesValidator(exchangeName, exchangeSettings).Validate();
        }

        private class ExchangesValidator
        {
            private readonly string _exchangeName;
            private readonly ExchangeSettings _exchangeSettings;

            public ExchangesValidator(string exchangeName, ExchangeSettings exchangeSettings)
            {
                _exchangeName = exchangeName;
                _exchangeSettings = exchangeSettings;
            }

            public void Validate()
            {
                _exchangeSettings.RequiredNotNull("_exchangeSettings for exchange " + _exchangeName);
                _exchangeSettings.Mode.RequiredEnum("_exchangeSettings.Mode for exchange " + _exchangeName);
                _exchangeSettings.OutdatingThreshold.RequiredBetween(TimeSpan.Zero, TimeSpan.FromHours(1),
                    "_exchangeSettings.OutdatingThreshold for exchange " + _exchangeName);
            }
        }
    }
}