using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using SimpleInjector.Diagnostics;
using SimpleInjector.Lifestyles;

namespace SimpleInjectorMyClientTypedConsoleApp
{
    public interface IHello
    {
        void SayHi();
    }

    public static class MyClientDependencyExtension
    {
        private static bool MyClientIsRegistered = false;
        public static Container AddMyClient<TClient, TImplementation>(this Container services)
            where TClient : class
            where TImplementation : class, TClient
        {
            RegisterMyClient(services);

            var s = services;
            services.Register<TClient>(() =>
            {
                var myClient = s.GetInstance<MyClient>();
                myClient.Owner = typeof(TImplementation).Name;
                var activator = ActivatorUtilities.CreateFactory(typeof(TImplementation), new[] { typeof(MyClient) });
                return (TImplementation)activator.Invoke(s, new object[] { myClient });
            });
            return services;
        }

        private static void RegisterMyClient(Container services)
        {
            if (MyClientIsRegistered) return;
            services.Register<MyClient>();
            MyClientIsRegistered = true;
        }

        public static Container AddMyClient<TImplementation>(this Container services)
            where TImplementation : class
        {
            RegisterMyClient(services);

            var s = services;
            services.Register<TImplementation>(() =>
            {
                var myClient = s.GetInstance<MyClient>();
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
            Console.WriteLine($"myClient.HashCode={myClient.GetHashCode()}");
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
            Container services = new Container();
            services.Register<HttpClient>(() => new HttpClient());
            Registration registration = services.GetRegistration(typeof(HttpClient)).Registration;
            registration.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent, "because Microsoft");
            services.AddMyClient<IHello, Hello>();
            services.AddMyClient<Hello2>();

            Registration registration2 = services.GetRegistration(typeof(MyClient)).Registration;

            services.Verify();
            var container = services;

            container.GetInstance<IHello>().SayHi();
            container.GetInstance<Hello2>().SayHi();
            container.GetInstance<IHello>().SayHi();
            container.GetInstance<Hello2>().SayHi();
        }
    }
}
