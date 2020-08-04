using System.Linq;
using System.Threading.Tasks;
using ApplicationContextRepositories.Dto;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RealEstateComparatorService.Services;

namespace RealEstateComparatorService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RealEstateComparatorController : ControllerBase
    {
        private readonly IRealEstateService _realEstateService;
        private readonly IMapper _mapper;

        public RealEstateComparatorController(IRealEstateService realEstateService, IMapper mapper)
        {
            _realEstateService = realEstateService;
            _mapper = mapper;
        }

        /// <summary>
        /// Get list of better Apartments
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST https://{host}/api/RealEstateComparator
        ///     {
        ///        "City": "Львів",
        ///        "NumberOfRooms": 2,
        ///        "HasMultipleFloors": false,
        ///        "DwellingSpace": 65,
        ///        "RenovationPricePerMeter": 12000,
        ///        "OverallPrice": 10000000
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Returns list of better Apartments</response>
        /// <response code="400">If ApartmentSpecsDto is in wrong format</response>
        [HttpPost]
        public IActionResult ProposeBetterApartments(ApartmentSpecsDto apartmentSpecs)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var betterApartments = _realEstateService
                .GetBetterApartments(apartmentSpecs)
                .Select(apartment => _mapper.Map<ApartmentDto>(apartment));

            return Ok(betterApartments);
        }

        /// <summary>
        /// Get Apart Complex by Id
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET https://{host}/api/RealEstateComparator?complexId={id}
        ///
        /// </remarks>
        /// <response code="200">Returns specified Apart Complex</response>
        /// <response code="204">If there is no such Apart Complex</response>
        /// <response code="400">If complexId is in wrong format</response>
        [HttpGet]
        public async Task<IActionResult> GetApartComplex(int complexId)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var apartComplex = await _realEstateService
                .GetApartComplex(complexId);

            return Ok(_mapper.Map<ApartComplexDto>(apartComplex));
        }
    }
}
