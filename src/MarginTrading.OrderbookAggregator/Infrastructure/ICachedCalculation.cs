namespace MarginTrading.OrderbookAggregator.Infrastructure
{
    public interface ICachedCalculation<out TResult>
    {
        TResult Get();
    }
}