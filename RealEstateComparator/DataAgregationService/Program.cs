using System.Threading.Tasks;

namespace DataAggregationService
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var dataAgregator = new DataAggregator();
            await dataAgregator.Run();
        }
    }
}
