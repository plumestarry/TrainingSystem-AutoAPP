using AutoAPI.Context.Entity;
using AutoMapper;
using AutoMapper.Configuration;
using AutoShared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoAPI.Extensions
{
    public class AutoMapperProFile : Profile
    {
        public AutoMapperProFile()
        {
            CreateMap<RecordEntity, RecordDto>().ReverseMap();
            CreateMap<ConfigEntity, ConfigDto>().ReverseMap();
            CreateMap<UserEntity, UserDto>().ReverseMap();
        }
    }
}
