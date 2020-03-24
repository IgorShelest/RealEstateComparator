using DataAgregationService.Enums;
using DataAgregationService.Parsers;

namespace DataAgregationService
{
    static class ParserFactory
    {
        public static IApartmentParser CreateParser(AvailableParsers parserType)
        {
            IApartmentParser parser = null;

            switch (parserType)
            {
                case AvailableParsers.LunUa:
                    parser = new LunUaApartmentParser();
                    break;
            }

            return parser;
        }
    }
}
