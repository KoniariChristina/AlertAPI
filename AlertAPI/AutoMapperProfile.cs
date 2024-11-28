using AlertAPI.DTO;
using AlertAPI.Models;
using AutoMapper;

namespace AlertAPI
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Alert, AlertDTO>();
            CreateMap<AlertDTO, Alert>();
            CreateMap<AlertIPAddress, AlertIPAddressDTO>();
            CreateMap<AlertIPAddressDTO, AlertIPAddress>();
            CreateMap<IPAddress, IPAddressDTO>();
            CreateMap<IPAddressDTO, IPAddress>();
        }
    }

    
}
