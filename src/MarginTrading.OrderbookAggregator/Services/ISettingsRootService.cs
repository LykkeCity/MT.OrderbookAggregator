using MarginTrading.OrderbookAggregator.Models.Settings;

namespace MarginTrading.OrderbookAggregator.Services
{
    /// <remarks>
    /// No async here because of a lock inside
    /// </remarks>
    public interface ISettingsRootService
    {
        void Set(SettingsRoot settings);
        SettingsRoot Get();
    }
}