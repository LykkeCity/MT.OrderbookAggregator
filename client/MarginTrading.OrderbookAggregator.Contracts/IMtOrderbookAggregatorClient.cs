using JetBrains.Annotations;

namespace MarginTrading.OrderbookAggregator.Contracts
{
    [PublicAPI]
    public interface IMtOrderbookAggregatorClient
    {
        ISettingsRootApi SettingsRoot { get; }
        IStatusApi Status { get; }
    }
}