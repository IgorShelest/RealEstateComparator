﻿using DataAgregationService.Parsers;

namespace DataAgregationService
{
    static class ParserFactory
    {

        public static IApartmentParser CreateParser<TParser>() where TParser : IApartmentParser, new()
        {
            return new TParser();
        }
    }
}
