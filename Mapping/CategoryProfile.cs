using AutoMapper;
using e_commerce.Model;
using e_commerce.Model.Dtos;

namespace e_commerce.Mapping;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<Category, CategoryDto>().ReverseMap();
        CreateMap<Category, CreateCategoryDto>().ReverseMap();
    }
}