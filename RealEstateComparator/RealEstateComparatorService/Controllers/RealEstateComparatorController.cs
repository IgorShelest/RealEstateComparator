using DataAgregationService.Db;
using DataAgregationService.Models;
using Microsoft.AspNetCore.Mvc;
using RealEstateComparatorService.Classes;
using RealEstateComparatorService.Services;
using System.Collections.Generic;
using System.Linq;

namespace RealEstateComparatorService.Controllers
{
    [ApiController]
    [Route("api/RealEstateComparator")]
    public class RealEstateComparatorController : ControllerBase
    {
        [HttpPost]
        public IActionResult ProposeBetterApartments(ApartmentSpecifications apartmentSpecs)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var realEstateService = new RealEstateService();
            var betterApartments = realEstateService.GetBetterApartments(apartmentSpecs);

            return Ok(betterApartments);
        }
    }
}
