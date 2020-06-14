using DataAggregationService.Interfaces;

namespace DataAggregationService
{
    static class ParserFactory
    {

        public static IAggregator CreateParser<TParser>() where TParser : IAggregator, new()
        {
            return new TParser();
        }
    }
}
