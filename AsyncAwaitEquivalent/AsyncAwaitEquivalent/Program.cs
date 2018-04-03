using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncAwaitEquivalent
{
    class Program
    {
        static void Main(string[] args)
        {
            WrapperWithAwaitAsync();
            WrapperWithoutAwaitAsync();

            Console.WriteLine("Main ends");
            Console.ReadKey();
        }

        public static async void WrapperWithoutAwaitAsync()
        {
            
            var stopwatch = Stopwatch.StartNew();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            ProcessAsync().ContinueWith(task =>
            {
                Console.WriteLine("Equivalent:");
                Console.WriteLine(stopwatch.Elapsed);
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public static async void WrapperWithAwaitAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            await ProcessAsync();
            Console.WriteLine("Await:");
            Console.WriteLine(stopwatch.Elapsed);
        }

        public static async Task ProcessAsync()
        {
#pragma warning disable 4014
            await Task.Delay(5000);
#pragma warning restore 4014
        }
    }
}
