using System.Threading.Tasks;
using MarginTrading.OrderbookAggregator.Enums;

namespace MarginTrading.OrderbookAggregator.Services
{
    public interface IAlertService
    {
        void AlertRiskOfficer(string assetPairId, string message, EventTypeEnum eventType);
        void AlertStarted();
        Task AlertStopping();
    }
}