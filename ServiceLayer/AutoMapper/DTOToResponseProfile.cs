using AutoMapper;
using DAL.Entities;
using Utilities.Models.Responses.Groups;
using Utilities.Models.Results;

namespace ServiceLayer.AutoMapper;

public class DTOToResponseProfile : Profile
{
    public DTOToResponseProfile()
    {
        CreateMap<Group, UserGroupResult>()
            .ForMember(d => d.GroupId, o => o.MapFrom(s => s.GroupId))
            .ForMember(d => d.GroupName, o => o.MapFrom(s => s.GroupName));
    }
}
