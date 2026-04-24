using AutoMapper;
using ProductAPI.DTOs;
using ProductAPI.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
//using AutoMapper;
//using ProductAPI.Models;
//using ProductAPI.DTOs;

namespace ProductAPI.Mappings
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            CreateMap<Product, ProductDTO>().ReverseMap();
        }
    }
}