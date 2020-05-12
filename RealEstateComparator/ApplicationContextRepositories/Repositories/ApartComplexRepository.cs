﻿using System.Collections.Generic;
using ApplicationContexts;
using ApplicationContexts.Models;

namespace ApplicationContextRepositories
{
    public class ApartComplexRepository : IApartComplexRepository
    {
        private readonly IApplicationContext _applicationContext;

        public ApartComplexRepository(IApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public void UpdateDb(IEnumerable<ApartComplex> apartComplexes)
        {
            _applicationContext.ApartComplexes.AddRange(apartComplexes);
            _applicationContext.Save();
        }
    }
}