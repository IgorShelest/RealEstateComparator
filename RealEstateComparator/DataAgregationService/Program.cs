using System;

namespace DataAgregationService
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("DataAgregationService Start");

            var dataAgregator = new DataAgregator();
            dataAgregator.Run();

            Console.WriteLine("DataAgregationService End");
        }
    }
}
