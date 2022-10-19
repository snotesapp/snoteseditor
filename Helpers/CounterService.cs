namespace BlazorApp1.Helpers
{
    public class CounterService
    {
        public int Count { get; private set; }

        public void Increment()
        {
            Count++;    
        }


    }
}
