using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace TestAsyncConsole
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("In Main() - Start");
            Console.WriteLine("Thread Id: {0}", Environment.CurrentManagedThreadId);
            Console.WriteLine();

            SynchronizationContext.SetSynchronizationContext(new MySynchronizationContext());

            await TestTaskResult();

            //await TestRealIOBoundTasks();

            Console.WriteLine();
            Console.WriteLine("In Main() - End");
            Console.WriteLine("Thread Id: {0}", Environment.CurrentManagedThreadId);
        }

        private static async Task TestRealIOBoundTasks()
        {
            Console.WriteLine("TestRealIOBoundTasks() - Start");
            Console.WriteLine("Thread Id: {0}", Environment.CurrentManagedThreadId);

            var gitHubService = new GitHubService(Configuration.Get("GH_PAT"));
            int num = 0;

            Console.WriteLine("Calling \"await foreach\"");
            Console.WriteLine();

            await foreach (var pr in gitHubService.GetLatestPrsAsync(50, "dotnet", "runtime"))
            {
                Console.WriteLine(pr["title"]);
                Console.WriteLine($"Received {++num} PRs in total");
            }

            Console.WriteLine();
            Console.WriteLine("Finished \"await foreach\"");

            Console.WriteLine("Thread Id: {0}", Environment.CurrentManagedThreadId);
            Console.WriteLine("TestRealIOBoundTasks() - End");
        }

        /// <summary>
        /// Demonstrates the difference between 'await task' and 'task.Result'
        /// </summary>
        /// <returns></returns>
        private static async Task TestTaskResult()
        {
            Console.WriteLine("TestTaskResult() - Start");
            Console.WriteLine("Thread Id: {0}", Environment.CurrentManagedThreadId);

            Console.WriteLine("Calling GetListOfNumbersAsync()");

            var getNumbersTask = GetListOfNumbersAsync();

            Console.WriteLine("Back in TestTaskResult()");
            Console.WriteLine("Thread Id: {0}", Environment.CurrentManagedThreadId);

            //Console.WriteLine("Calling \"await getNumbersTask\"");
            //// Always 'await' a task.
            //// If it's completed already, it will immediately return the result synchronously.
            //var numbers = await getNumbersTask;
            //Console.WriteLine("Finished \"await getNumbersTask\"");

            Console.WriteLine("Calling \"getNumbersTask.Result\"");
            // Best to avoid Task.Result.
            // https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1.result?view=net-7.0
            var numbers = getNumbersTask.Result;
            Console.WriteLine("Finished \"getNumbersTask.Result\"");

            Console.WriteLine("Back in TestTaskResult()");

            Console.WriteLine("Total numbers: {0}", numbers.Count);

            Console.WriteLine("Thread Id: {0}", Environment.CurrentManagedThreadId);
            Console.WriteLine("TestTaskResult() - End");
        }

        private static async Task<List<int>> GetListOfNumbersAsync()
        {
            Console.WriteLine("GetListOfNumbersAsync() - Start");
            Console.WriteLine("Thread Id: {0}", Environment.CurrentManagedThreadId);
            Console.WriteLine();

            var iteration = 0;
            var numbers = new List<int>();
            var enumStart = iteration;
            var delay = 5000;

            Console.WriteLine("Entering do-while");
            do
            {
                //if (iteration == 1) throw new Exception("Crikey!");
                
                Console.WriteLine("Starting iteration: {0}", iteration +1);
                Console.WriteLine("Thread Id: {0}", Environment.CurrentManagedThreadId);
                Console.WriteLine("Calling \"await Task.Delay({0})\"", delay);

                await Task.Delay(delay);

                Console.WriteLine("Finished \"await Task.Delay({0})\"", delay);
                Console.WriteLine("Thread Id: {0}", Environment.CurrentManagedThreadId);

                var seq = Enumerable.Range(enumStart, 10);
                enumStart += 10;
                numbers = numbers.Concat(seq).ToList();

                Console.WriteLine("Finished iteration: {0}", iteration + 1);
                Console.WriteLine();
            } while (++iteration < 2);

            Console.WriteLine("Out of do-while");
            Console.WriteLine("Thread Id: {0}", Environment.CurrentManagedThreadId);
            Console.WriteLine("GetListOfNumbersAsync() - End");
            Console.WriteLine();

            return numbers;
        }
    }
}