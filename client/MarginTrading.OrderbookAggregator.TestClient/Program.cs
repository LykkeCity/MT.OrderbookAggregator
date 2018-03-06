using System;
using System.Threading.Tasks;
using AsyncFriendlyStackTrace;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using JetBrains.Annotations;
using MarginTrading.OrderbookAggregator.Contracts;
using MarginTrading.OrderbookAggregator.Contracts.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Refit;

namespace MarginTrading.OrderbookAggregator.TestClient
{
    internal static class Program
    {
        private const string AssetPairId = "BTCUSD";
        private static int _counter;

        static async Task Main(string[] args)
        {
            try
            {
                await Run();
            }
            catch (ApiException e)
            {
                var str = e.Content;
                if (str.StartsWith('"'))
                {
                    str = TryDeserializeToString(str);
                }
                
                Console.WriteLine(str);
                Console.WriteLine(e.ToAsyncString());
            }
        }

        private static string TryDeserializeToString(string str)
        {
            try
            {
                return JsonConvert.DeserializeObject<string>(str);
            }
            catch
            {
                return str;
            }
        }

        private static async Task Run()
        {
            var services = new ServiceCollection();
            var builder = new ContainerBuilder();
            services.RegisterMtOrderbookAggregatorClient("http://mt-orderbook-aggregator.lykke-mt.svc.cluster.local", "TestClient");
            builder.Populate(services);
            var container = builder.Build();
            var client = container.Resolve<IMtOrderbookAggregatorClient>();
            
            await client.Status.List().Dump();
            await client.Status.GetByAssetPair(AssetPairId).Dump();
            await client.Status.GetByExchange("bitmex").Dump();

            var root = await client.SettingsRoot.Get().Dump();
            await client.SettingsRoot.Set(root).Dump();
            
            Console.WriteLine("Successfuly finished");
        }

        [CanBeNull]
        public static T Dump<T>(this T o)
        {
            var str = o is string s ? s : JsonConvert.SerializeObject(o);
            Console.WriteLine("{0}. {1}", ++_counter, str);
            return o;
        }
        
        [ItemCanBeNull]
        public static async Task<T> Dump<T>(this Task<T> t)
        {
            var obj = await t;
            obj.Dump();
            return obj;
        }
        
        public static async Task Dump(this Task o)
        {
            await o;
            "ok".Dump();
        }
    }
}