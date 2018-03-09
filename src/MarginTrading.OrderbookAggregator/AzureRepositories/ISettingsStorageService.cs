using JetBrains.Annotations;
using MarginTrading.OrderbookAggregator.Models.Settings;

namespace MarginTrading.OrderbookAggregator.AzureRepositories
{
    internal interface ISettingsStorageService
    {
        [CanBeNull] SettingsRoot Read();
        void Write(SettingsRoot model);
    }
}
