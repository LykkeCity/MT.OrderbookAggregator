using MarginTrading.OrderbookAggregator.Enums;

namespace MarginTrading.OrderbookAggregator.Services
{
    public interface IAlertSeverityLevelService
    {
        /// <summary>
        /// Takes an <paramref name="eventType"/> and returns where it should be written to - a slack channel type and 
        /// a trace level  
        /// </summary>
        (string SlackChannelType, TraceLevelGroupEnum TraceLevel) GetLevel(EventTypeEnum eventType);
    }
}