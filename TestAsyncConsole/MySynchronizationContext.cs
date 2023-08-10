namespace TestAsyncConsole
{
    internal class MySynchronizationContext : SynchronizationContext
    {
        public override void Post(SendOrPostCallback d, object? state)
        {
            Console.WriteLine();
            Console.WriteLine(nameof(MySynchronizationContext));
            Console.WriteLine();

            base.Post(d, state);
        }
    }
}