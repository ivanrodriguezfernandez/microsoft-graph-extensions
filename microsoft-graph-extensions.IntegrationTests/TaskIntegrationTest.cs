using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;

namespace microsoft_graph_extensions.IntegrationTests
{
    [TestFixture]
    public class TaskIntegrationTest
    {
        [Test]
        public async Task TaskWhen()
        {
            var myStask = new MyStask();
            var result = await myStask.Main();
        }
    }
    public class MyStask
    {
        public async Task<string> Test()
        {
            await Task.Delay(18);
            return await Task.FromResult("ResultTask1");
        }

        public async Task<string> Test2()
        {
            await Task.Delay(18);
            return await Task.FromResult("ResultTask2");
        }

        public async Task<string[]> Main()
        {
            var tasks = new List<Task> {Test(), Test2()};
            var continuation = Task.WhenAll(tasks);

            continuation.Wait();

            if (continuation.Status == TaskStatus.RanToCompletion)
            {
                var result = continuation;
            }

            //var status = t.Status;
            //var taskResult2 = await Task.WhenAll(Test(), Test2());
            //tasksResult
            //return tasksResult;
            return null;
        }
    }
    //http://www.tugberkugurlu.com/archive/how-and-where-concurrent-asynchronous-io-with-asp-net-web-api
    public class Example
    {
        public static void Main2()
        {
            var tasks = new Task<long>[10];
            for (int ctr = 1; ctr <= 10; ctr++)
            {
                int delayInterval = 18 * ctr;
                tasks[ctr - 1] = Task.Run(async () => {
                    long total = 0;
                    await Task.Delay(delayInterval);
                    var rnd = new Random();
                    // Generate 1,000 random numbers.
                    for (int n = 1; n <= 1000; n++)
                        total += rnd.Next(0, 1000);

                    return total;
                });
            }
            var continuation = Task.WhenAll(tasks);
            try
            {
                continuation.Wait();
            }
            catch (AggregateException)
            { }

            if (continuation.Status == TaskStatus.RanToCompletion)
            {
                long grandTotal = 0;
                foreach (var result in continuation.Result)
                {
                    grandTotal += result;
                    Console.WriteLine("Mean: {0:N2}, n = 1,000", result / 1000.0);
                }

                Console.WriteLine("\nMean of Means: {0:N2}, n = 10,000",
                    grandTotal / 10000);
            }
            // Display information on faulted tasks.
            else
            {
                foreach (var t in tasks)
                    Console.WriteLine("Task {0}: {1}", t.Id, t.Status);
            }

        }
    }

}