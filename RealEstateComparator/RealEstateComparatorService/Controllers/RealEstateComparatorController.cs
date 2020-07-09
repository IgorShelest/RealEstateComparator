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
