using DataAggregationService.Interfaces;

namespace DataAggregationService
{
    static class ParserFactory
    {

        public static IApartmentParser CreateParser<TParser>() where TParser : IApartmentParser, new()
        {
            return new TParser();
        }
    }
}
