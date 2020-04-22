using ApplicationContextRepositories.Dto;
using Microsoft.AspNetCore.Mvc;
using RealEstateComparatorService.Services;

namespace RealEstateComparatorService.Controllers
{
    [ApiController]
    [Route("api/RealEstateComparator")]
    public class RealEstateComparatorController : ControllerBase
    {
        [HttpPost]
        public IActionResult ProposeBetterApartments(ApartmentSpecsDto apartmentSpecs)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var realEstateService = new RealEstateService();
            var betterApartments = realEstateService.GetBetterApartments(apartmentSpecs);

            return Ok(betterApartments);
        }
    }
}
