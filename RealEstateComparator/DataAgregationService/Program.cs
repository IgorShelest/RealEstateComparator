using System.Threading.Tasks;

namespace DataAgregationService
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var dataAgregator = new DataAgregator();
            await dataAgregator.Run();
        }
    }
}
