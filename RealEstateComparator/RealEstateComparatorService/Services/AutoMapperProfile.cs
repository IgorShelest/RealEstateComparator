using ApplicationContextRepositories.Dto;
using ApplicationContexts.Models;
using AutoMapper;

namespace RealEstateComparatorService.Services
{
    public class AutoMapperProfile: Profile
    {
        public AutoMapperProfile()  
        {  
            CreateMap<Apartment, ApartmentDto>();  
        } 
    }
}