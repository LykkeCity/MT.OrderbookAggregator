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
                
                switch (_exchangeSettings.Mode)
                {
                    case ExchangeModeEnum.Disabled:
                        return;
                    case ExchangeModeEnum.TakeAll:
                        ValidateDefaultSettings(_exchangeSettings.DefaultSettings);
                        break;
                    case ExchangeModeEnum.UseOnlyExplicitlyConfigured:
                        _exchangeSettings.AssetPairs.RequiredNotNullOrEmpty(
                            "_exchangeSettings.AssetPairs for exchange " + _exchangeName);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(_exchangeSettings.Mode), _exchangeSettings.Mode,
                            string.Empty);
                }

                _exchangeSettings.OutdatingThreshold.RequiredBetween(TimeSpan.Zero, TimeSpan.FromHours(1),
                    "_exchangeSettings.OutdatingThreshold for exchange " + _exchangeName);
                ValidateAssetPairs();
            }

            private void ValidateDefaultSettings(AssetPairSettings defaultSettings)
            {
                defaultSettings.RequiredNotNull("defaultSettings for exchange " + _exchangeName);
                defaultSettings.Markups.RequiredNotNull("defaultSettings.Markups for exchange " + _exchangeName);
                defaultSettings.Markups.Bid.RequiredNotGreaterThan(0, "defaultSettings.Markups.Bid for exchange " + _exchangeName);
                defaultSettings.Markups.Ask.RequiredNotLessThan(0, "defaultSettings.Markups.Ask for exchange " + _exchangeName);
            }

            private void ValidateAssetPairs()
            {
                _exchangeSettings.AssetPairs.RequiredNotNull("_exchangeSettings.AssetPairs for exchange " + _exchangeName);
                foreach (var (assetPairId, assetPairSettings) in _exchangeSettings.AssetPairs)
                    new AssetPairValidator(_exchangeName, assetPairId, assetPairSettings).Validate();
            }
        }

        private class AssetPairValidator
        {
            private readonly string _exchangeName;
            private readonly string _assetPairId;
            private readonly AssetPairSettings _assetPairSettings;

            public AssetPairValidator(string exchangeName, string assetPairId, AssetPairSettings assetPairSettings)
            {
                _exchangeName = exchangeName;
                _assetPairId = assetPairId;
                _assetPairSettings = assetPairSettings;
            }

            public void Validate()
            {
                _assetPairSettings.RequiredNotNull(
                    $"_assetPairSettings for exchange {_exchangeName} and pair {_assetPairId}");
                _assetPairSettings.Markups.RequiredNotNull(
                    $"_assetPairSettings.Markups for exchange {_exchangeName} and pair {_assetPairId}");
                _assetPairSettings.Markups.Bid.RequiredNotGreaterThan(0,
                    $"_assetPairSettings.Markups.Bid for exchange {_exchangeName} and pair {_assetPairId}");
                _assetPairSettings.Markups.Ask.RequiredNotLessThan(0,
                    $"_assetPairSettings.Markups.Ask for exchange {_exchangeName} and pair {_assetPairId}");
            }
        }
    }
}