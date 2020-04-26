using ApplicationContextRepositories.Dto;
using Microsoft.AspNetCore.Mvc;
using RealEstateComparatorService.Services;

namespace RealEstateComparatorService.Controllers
{
    [ApiController]
    [Route("api/RealEstateComparator")]
    public class RealEstateComparatorController : ControllerBase
    {
        private readonly IRealEstateService _realEstateService;

        public RealEstateComparatorController(IRealEstateService realEstateService)
        {
            _realEstateService = realEstateService;
        }

        [HttpPost]
        public IActionResult ProposeBetterApartments(ApartmentSpecsDto apartmentSpecs)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var betterApartments = _realEstateService.GetBetterApartments(apartmentSpecs);

            return Ok(betterApartments);
        }
    }
}
