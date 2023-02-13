using AutoMapper;
using gRPCMap4d.Protos;
using Map4dApiShared.Directions.Models;
using Route = Map4dApiShared.Directions.Models.Route;

namespace gRPCMap4d.AutoMapper
{
    /// <summary>
    /// Auto mapper register mapping
    /// </summary>
    public class MappingProfile : Profile
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MappingProfile()
        {
            #region route mapping

            CreateMap<Direction, RouteReply>();
            CreateMap<Route, RouteReply>();
            CreateMap<Leg, LegReply>();
            CreateMap<Description, DescriptionReply>();
            CreateMap<Location, LocationReply>();
            CreateMap<Step, StepReply>();
            CreateMap<DirectionETA, RouteETAReply>();

            #endregion
        }
    }
}
