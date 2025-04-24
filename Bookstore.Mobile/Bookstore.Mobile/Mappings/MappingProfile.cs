

using AutoMapper;
using Bookstore.Mobile.Models;

namespace Bookstore.Mobile.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ----- CÁC MAPPING CẦN THIẾT CHO MAUI -----

            // Mapping các DTO tóm tắt cho Dashboard/Reports 
            CreateMap<BookDto, BookSummaryDto>();
            //CreateMap<PromotionDto, PromotionSummaryDto>();
            //CreateMap<CategoryDto, CategorySummaryDto>();
        }
    }
}