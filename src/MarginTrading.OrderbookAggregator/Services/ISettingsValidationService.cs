using MarginTrading.OrderbookAggregator.Models.Settings;

namespace MarginTrading.OrderbookAggregator.Services
{
    public interface ISettingsValidationService
    {
        void Validate(SettingsRoot root);
    }
}