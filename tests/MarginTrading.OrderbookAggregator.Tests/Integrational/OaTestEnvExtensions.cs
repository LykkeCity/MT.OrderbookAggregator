using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Equivalency;
using MarginTrading.OrderbookAggregator.Contracts.Api;
using MarginTrading.OrderbookAggregator.Contracts.Messages;
using MarginTrading.OrderbookAggregator.Enums;

namespace MarginTrading.OrderbookAggregator.Tests.Integrational
{
    internal static class OaTestEnvExtensions
    {
        public static void VerifyMessagesSent(this IOaTestEnvironment testEnvironment, params object[] messages)
        {
            VerifyMessagesSentCore(testEnvironment, messages, o => o
                .Excluding(i => i.SelectedMemberPath.EndsWith(".Timestamp", StringComparison.OrdinalIgnoreCase)));
        }

        public static void VerifyMessagesSentWithTime(this IOaTestEnvironment testEnvironment, params object[] messages)
        {
            VerifyMessagesSentCore(testEnvironment, messages, o => o);
        }

        public static StartedMessage GetStartedMessage(this IOaTestEnvironment testEnvironment)
        {
            return new StartedMessage();
        }

        public static DateTime Sleep(this IOaTestEnvironment env, TimeSpan time)
        {
            return env.UtcNow += time;
        }

        public static DateTime SleepSecs(this IOaTestEnvironment env, double seconds)
        {
            return env.Sleep(TimeSpan.FromSeconds(seconds));
        }

        public static ExternalExchangeOrderbookMessage GetOrderbookMessage(this IOaTestEnvironment testEnvironment,
            string exchangeName, Generator<decimal> decimals, string assetPairId = "BTCUSD")
        {
            return new ExternalExchangeOrderbookMessage
            {
                Bids = new List<VolumePrice>
                {
                    new VolumePrice {Price = decimals.Next(), Volume = decimals.Next()},
                    new VolumePrice {Price = decimals.Next(), Volume = decimals.Next()}
                },
                Asks = new List<VolumePrice>
                {
                    new VolumePrice {Price = decimals.Next(), Volume = decimals.Next()},
                    new VolumePrice {Price = decimals.Next(), Volume = decimals.Next()}
                },
                AssetPairId = assetPairId,
                ExchangeName = exchangeName,
                Timestamp = testEnvironment.UtcNow,
            };
        }

        public static OrderbookStatusModel MakeOrderbookStatusModel(this IOaTestEnvironment env, string exchangeName,
            string assetPairId,
            decimal multiplier)
        {
            return new OrderbookStatusModel
            {
                Status = OrderbookStatusEnum.Valid.ToString(),
                BidsDepth = 2,
                AsksDepth = 2,
                ResultingsAsksDepth = 2,
                ResultingsBidsDepth = 2,
                AssetPairId = assetPairId,
                ExchangeName = exchangeName,
                LastUpdateTime = env.UtcNow,
                BestBid = 3 * multiplier,
                BestAsk = 5 * multiplier,
                ResultingBestBid = 3 * multiplier,
                ResultingBestAsk = 5 * multiplier,
            };
        }

        private static void VerifyMessagesSentCore(this IOaTestEnvironment testEnvironment,
            IEnumerable<object> messages,
            Func<EquivalencyAssertionOptions<object>, EquivalencyAssertionOptions<object>> config)
        {
            var sent = testEnvironment.StubRabbitMqService.GetSentMessages();
            var expected = messages.ToArray();
            
            sent.Should().HaveCount(expected.Length);
            
            for (var i = 0; i < expected.Length; i++)
            {
                var sentMessage = sent[i];
                var expectedMessage = expected[i];
                if (expectedMessage is StartedMessage)
                    sentMessage.Should().BeOfType<StartedMessage>();
                else
                    sentMessage.Should().BeEquivalentTo(expectedMessage);
            }
        }
    }
}