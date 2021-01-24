using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HttpClientTypedConsoleApp
{
    public interface IHello
    {
        void SayHi();
    }

    public static class MyClientDependencyExtension
    {
        public static IServiceCollection AddMyClient<TClient, TImplementation>(this IServiceCollection services)
            where TClient : class
            where TImplementation : class, TClient
        {
            services.TryAddTransient<MyClient>();

            services.AddTransient<TClient>(s =>
            {
                var myClient = s.GetService<MyClient>();
                myClient.Owner = typeof(TImplementation).Name;
                var activator = ActivatorUtilities.CreateFactory(typeof(TImplementation), new[] { typeof(MyClient) });
                return (TImplementation)activator.Invoke(s, new object[] { myClient });
            });
            return services;
        }

        public static IServiceCollection AddMyClient<TImplementation>(this IServiceCollection services)
            where TImplementation : class
        {
            services.TryAddTransient<MyClient>();

            services.AddTransient<TImplementation>(s =>
            {
                var myClient = s.GetService<MyClient>();
                myClient.Owner = typeof(TImplementation).Name;
                var activator = ActivatorUtilities.CreateFactory(typeof(TImplementation), new[] { typeof(MyClient) });
                return (TImplementation)activator.Invoke(s, new object[] { myClient });
            });
            return services;
        }
    }

    public class Hello : IHello
    {
        public Hello(MyClient myClient, HttpClient httpClient)
        {
            Console.WriteLine($"myClient.Owner={myClient.Owner}");
        }

        public void SayHi()
        {
            Console.WriteLine("Hi");
        }
    }

    public class Hello2
    {
        public Hello2(MyClient myClient)
        {
            Console.WriteLine($"myClient.Owner={myClient.Owner}");
        }

        public void SayHi()
        {
            Console.WriteLine("Hi");
        }
    }

    public class MyClient
    {
        public string Owner { get; set; }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();
            services.AddHttpClient<IHello, Hello>();
            services.AddMyClient<IHello, Hello>();
            services.AddMyClient<Hello2>();

            var container = services.BuildServiceProvider();

            var h = container.GetRequiredService<IHello>();
            h.SayHi();

            var h2 = container.GetRequiredService<Hello2>();
            h2.SayHi();
        }
    }
}