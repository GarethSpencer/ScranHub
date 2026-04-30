using AutoMapper;
using DAL.Entities;
using Utilities.Models.Results;

namespace RepositoryLayer.AutoMapper;

public class DTOToResponseProfile : Profile
{
    public DTOToResponseProfile()
    {
        CreateMap<Group, GroupResult>()
            .ForMember(d => d.GroupId, o => o.MapFrom(s => s.GroupId))
            .ForMember(d => d.GroupName, o => o.MapFrom(s => s.GroupName));
    }
}
